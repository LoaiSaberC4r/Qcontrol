using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Abstraction.Services;
using QControl.Application.Shared.Operational;
using QControl.Application.Shared.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Command.UpdateService;

internal sealed class UpdateServiceCommandHandler
    : ICommandHandler<UpdateServiceCommand, ServiceResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService>? _branchServiceReadRepository;
    private readonly IWriteReadRepository<BranchServiceSegment>?
        _branchServiceSegmentReadRepository;
    private readonly IWriteReadRepository<Segment>? _segmentReadRepository;
    private readonly IWriteRepository<BranchServiceSegment>?
        _branchServiceSegmentWriteRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly IServiceTicketUsageChecker _ticketUsageChecker;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        IServiceTicketUsageChecker ticketUsageChecker,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
        : this(
            serviceReadRepository,
            branchServiceReadRepository: null,
            branchServiceSegmentReadRepository: null,
            segmentReadRepository: null,
            branchServiceSegmentWriteRepository: null,
            serviceWriteRepository,
            concurrencyTokenManager,
            ticketUsageChecker,
            currentUser,
            SystemLevelServiceBranchContext.Instance,
            AllowAllServiceDefinitionAccessValidator.Instance,
            unitOfWork)
    {
    }

    public UpdateServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService>? branchServiceReadRepository,
        IWriteReadRepository<BranchServiceSegment>?
            branchServiceSegmentReadRepository,
        IWriteReadRepository<Segment>? segmentReadRepository,
        IWriteRepository<BranchServiceSegment>?
            branchServiceSegmentWriteRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        IServiceTicketUsageChecker ticketUsageChecker,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext,
        IServiceDefinitionAccessValidator accessValidator,
        IUnitOfWork unitOfWork)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository;
        _branchServiceSegmentReadRepository =
            branchServiceSegmentReadRepository;
        _segmentReadRepository = segmentReadRepository;
        _branchServiceSegmentWriteRepository =
            branchServiceSegmentWriteRepository;
        _serviceWriteRepository = serviceWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _ticketUsageChecker = ticketUsageChecker
            ?? throw new ArgumentNullException(nameof(ticketUsageChecker));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ServiceResponse>> Handle(
        UpdateServiceCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Update.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var service = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceForMutationSpec(
                request.Id,
                includeCustomInputs: true),
            cancellationToken);

        if (service is null)
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Update.NotFound",
                ServiceFeatureMessages.NotFound,
                ErrorType.NotFound));
        }

        if (service.IsDeleted)
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Update.Deleted",
                ServiceFeatureMessages.Deleted,
                ErrorType.Conflict));
        }

        var editAccess = _accessValidator.EnsureCanEdit(
            service,
            "Services.Update");

        if (editAccess.IsFailure)
        {
            return Result<ServiceResponse>.Fail(editAccess.Errors);
        }

        var normalizedArabicName = request.ArabicName.Trim();
        var normalizedEnglishName = request.EnglishName.Trim();
        var normalizedServiceCode =
            ServiceCodeNormalizer.Normalize(request.ServiceCode);
        var normalizedRangePrefix = string.IsNullOrWhiteSpace(request.RangePrefix)
            ? null
            : request.RangePrefix.Trim();
        var requestedTicketIssuable =
            request.IsTicketIssuable.GetValueOrDefault();

        var duplicateServiceCodeError =
            await ServiceRuleChecks.ValidateServiceCodeIsUniqueAsync(
                _serviceReadRepository,
                normalizedServiceCode,
                service.Id,
                "Services.Update.ServiceCodeAlreadyExists",
                cancellationToken);

        if (duplicateServiceCodeError is not null)
        {
            return Result<ServiceResponse>.Fail(duplicateServiceCodeError);
        }

        var ticketSettingsError = ServiceRuleChecks.ValidateTicketSettings(
            requestedTicketIssuable,
            normalizedRangePrefix,
            request.RangeStartNumber,
            request.RangeEndNumber,
            request.WaitingDuration,
            request.NoOfTicketCopies,
            operation: "Update");

        if (ticketSettingsError is not null)
        {
            return Result<ServiceResponse>.Fail(ticketSettingsError);
        }

        if (request.ParentServiceId.HasValue)
        {
            var parentError = await ServiceRuleChecks.ValidateParentAsync(
                _serviceReadRepository,
                _ticketUsageChecker,
                request.ParentServiceId.Value,
                service.Id,
                service.Scope,
                service.OwnerBranchId,
                operation: "Update",
                cancellationToken);

            if (parentError is not null)
            {
                return Result<ServiceResponse>.Fail(parentError);
            }
        }

        var firstChildId = await _serviceReadRepository.FirstOrDefaultAsync(
            new ServiceHasChildrenSpec(service.Id),
            cancellationToken);
        var hasChildren = firstChildId > 0;
        if (requestedTicketIssuable && hasChildren)
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Update.HasChildrenCannotBeTicketIssuable",
                ServiceFeatureMessages.HasChildrenCannotBeTicketIssuable,
                ErrorType.Conflict));
        }

        if (hasChildren && request.IsClientInputRequired)
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Update.ParentClientInputNotAllowed",
                ServiceFeatureMessages.ParentClientInputNotAllowed,
                ErrorType.Conflict));
        }

        if (hasChildren && request.CustomInputs is { Count: > 0 })
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Update.ParentCustomInputsNotAllowed",
                ServiceFeatureMessages.ParentCustomInputsNotAllowed,
                ErrorType.Conflict));
        }

        if (!hasChildren)
        {
            var customInputError = ServiceCustomInputRuleChecks.Validate(
                request.CustomInputs?
                    .Cast<ServiceCustomInputDefinitionCommand>()
                    .ToArray(),
                request.IsClientInputRequired,
                "Update",
                validateIds: true);
            if (customInputError is not null)
            {
                return Result<ServiceResponse>.Fail(customInputError);
            }

            var customInputIdError = await ValidateCustomInputIdsAsync(
                service,
                request.CustomInputs,
                cancellationToken);
            if (customInputIdError is not null)
            {
                return Result<ServiceResponse>.Fail(customInputIdError);
            }
        }

        if (service.IsTicketIssuable && !requestedTicketIssuable)
        {
            var hasHistoricalTickets =
                await _ticketUsageChecker.HasHistoricalTicketsAsync(
                    service.Id,
                    cancellationToken);

            if (hasHistoricalTickets)
            {
                return Result<ServiceResponse>.Fail(new Error(
                    "Services.Update.HasHistoricalTickets",
                    ServiceFeatureMessages.HistoricalTicketsPreventCategory,
                    ErrorType.Conflict));
            }
        }

        var duplicateError =
            await ServiceRuleChecks.ValidateDuplicateNamesAsync(
                _serviceReadRepository,
                normalizedArabicName,
                normalizedEnglishName,
                request.ParentServiceId,
                service.Scope,
                service.OwnerBranchId,
                service.Id,
                operation: "Update",
                cancellationToken);

        if (duplicateError is not null)
        {
            return Result<ServiceResponse>.Fail(duplicateError);
        }

        var quotaError = await ValidateAndRecalculateDefaultQuotasAsync(
            service,
            requestedTicketIssuable,
            hasChildren,
            request.RangeStartNumber,
            request.RangeEndNumber,
            cancellationToken);
        if (quotaError is not null)
        {
            return Result<ServiceResponse>.Fail(quotaError);
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            service,
            rowVersion);

        service.ChangeParent(
            request.ParentServiceId,
            _currentUser.UserId.Value);

        ReconcileCustomInputs(
            service,
            request.IsClientInputRequired,
            hasChildren ? null : request.CustomInputs,
            _currentUser.UserId.Value);

        service.Update(
            arabicName: normalizedArabicName,
            englishName: normalizedEnglishName,
            serviceCode: normalizedServiceCode,
            isServiceCodeRequired: request.IsServiceCodeRequired,
            arabicUserMessage: request.ArabicUserMessage,
            englishUserMessage: request.EnglishUserMessage,
            isTicketIssuable: requestedTicketIssuable,
            isClientInputRequired: request.IsClientInputRequired,
            hasReservation: request.HasReservation,
            orderNo: request.OrderNo,
            priority: request.Priority,
            rangePrefix: normalizedRangePrefix,
            rangeStartNumber: request.RangeStartNumber,
            rangeEndNumber: request.RangeEndNumber,
            waitingDuration: request.WaitingDuration,
            noOfTicketCopies: request.NoOfTicketCopies,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        _serviceWriteRepository.Update(service);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Update.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException ex)
            when (ServiceUniqueConstraintErrorMapper.TryMapUpdate(
                ex,
                out var error))
        {
            return Result<ServiceResponse>.Fail(error);
        }

        var response = await ServiceRuleChecks.BuildServiceResponseAsync(
            _serviceReadRepository,
            service.Id,
            await BuildAccessContextAsync(service.Id, cancellationToken),
            ServiceFeatureMessages.UpdateSuccess,
            cancellationToken);

        return response is null
            ? Result<ServiceResponse>.Fail(new Error(
                "Services.Update.NotFoundAfterSave",
                ServiceFeatureMessages.NotFound,
                ErrorType.Infrastructure))
            : Result<ServiceResponse>.Ok(response);
    }

    private async Task<ServiceResponseAccessContext> BuildAccessContextAsync(
        int serviceId,
        CancellationToken cancellationToken)
    {
        var assignedServiceIds = Array.Empty<int>();

        if (_currentBranchContext.ActiveBranchId.HasValue)
        {
            var activeBranchId = _currentBranchContext.ActiveBranchId.Value;
            assignedServiceIds = _branchServiceReadRepository is null
                ? Array.Empty<int>()
                : await _branchServiceReadRepository.Query()
                .Where(x =>
                    x.BranchId == activeBranchId &&
                    x.ServiceId == serviceId)
                .Select(x => x.ServiceId)
                .ToArrayAsync(cancellationToken);
        }

        return new ServiceResponseAccessContext(
            _currentBranchContext.IsSystemLevelActor,
            _currentBranchContext.IsBranchActor,
            _currentBranchContext.ActiveBranchId,
            assignedServiceIds);
    }

    private async Task<Error?> ValidateCustomInputIdsAsync(
        Service service,
        IReadOnlyCollection<UpdateServiceCustomInputCommand>? requestedInputs,
        CancellationToken cancellationToken)
    {
        var requestedIds = requestedInputs?
            .Where(x => x.CustomInputId.HasValue)
            .Select(x => x.CustomInputId!.Value)
            .ToHashSet() ?? new HashSet<int>();
        if (requestedIds.Count == 0)
        {
            return null;
        }

        var ownedIds = service.CustomInputs
            .Where(x => requestedIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToHashSet();
        var missingIds = requestedIds
            .Where(id => !ownedIds.Contains(id))
            .ToArray();
        if (missingIds.Length == 0)
        {
            return null;
        }

        var belongsToAnotherService = await _serviceReadRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id != service.Id)
            .SelectMany(x => x.CustomInputs)
            .AnyAsync(x => missingIds.Contains(x.Id), cancellationToken);

        return belongsToAnotherService
            ? new Error(
                "Services.Update.CustomInputOwnershipConflict",
                ServiceFeatureMessages.CustomInputOwnershipConflict,
                ErrorType.Conflict)
            : new Error(
                "Services.Update.CustomInputNotFound",
                ServiceFeatureMessages.CustomInputNotFound,
                ErrorType.NotFound);
    }

    private static void ReconcileCustomInputs(
        Service service,
        bool isClientInputRequired,
        IReadOnlyCollection<UpdateServiceCustomInputCommand>? requestedInputs,
        Guid modifiedByApplicationUserId)
    {
        if (!isClientInputRequired)
        {
            foreach (var customInput in service.CustomInputs.Where(x => x.IsActive))
            {
                service.DeactivateCustomInput(
                    customInput,
                    modifiedByApplicationUserId);
            }

            return;
        }

        var inputs = requestedInputs ??
            Array.Empty<UpdateServiceCustomInputCommand>();
        var requestedIds = inputs
            .Where(x => x.CustomInputId.HasValue)
            .Select(x => x.CustomInputId!.Value)
            .ToHashSet();

        foreach (var omittedInput in service.CustomInputs
                     .Where(x => x.IsActive && !requestedIds.Contains(x.Id))
                     .ToArray())
        {
            service.DeactivateCustomInput(
                omittedInput,
                modifiedByApplicationUserId);
        }

        var existingById = service.CustomInputs.ToDictionary(x => x.Id);
        foreach (var requestedInput in inputs)
        {
            if (!requestedInput.CustomInputId.HasValue)
            {
                service.AddCustomInput(
                    requestedInput.Name,
                    requestedInput.LabelEn,
                    requestedInput.LabelAr,
                    requestedInput.Type,
                    requestedInput.IsRequired,
                    requestedInput.MinLength,
                    requestedInput.MaxLength,
                    requestedInput.MinValue,
                    requestedInput.MaxValue,
                    requestedInput.StartWith,
                    requestedInput.Order,
                    modifiedByApplicationUserId);
                continue;
            }

            var existing = existingById[requestedInput.CustomInputId.Value];
            service.UpdateCustomInput(
                existing,
                requestedInput.Name,
                requestedInput.LabelEn,
                requestedInput.LabelAr,
                requestedInput.Type,
                requestedInput.IsRequired,
                requestedInput.MinLength,
                requestedInput.MaxLength,
                requestedInput.MinValue,
                requestedInput.MaxValue,
                requestedInput.StartWith,
                requestedInput.Order,
                modifiedByApplicationUserId);
            service.RestoreCustomInput(existing, modifiedByApplicationUserId);
        }
    }

    private async Task<Error?> ValidateAndRecalculateDefaultQuotasAsync(
        Service service,
        bool requestedTicketIssuable,
        bool hasChildren,
        int? rangeStartNumber,
        int? rangeEndNumber,
        CancellationToken cancellationToken)
    {
        var rangeChanged =
            service.RangeStartNumber != rangeStartNumber ||
            service.RangeEndNumber != rangeEndNumber;
        if (!rangeChanged ||
            !requestedTicketIssuable ||
            hasChildren ||
            !rangeStartNumber.HasValue ||
            !rangeEndNumber.HasValue ||
            _branchServiceReadRepository is null ||
            _branchServiceSegmentReadRepository is null)
        {
            return null;
        }

        var capacity =
            BranchServiceSegmentQuotaCalculator.CalculateCapacity(
                rangeStartNumber,
                rangeEndNumber);
        if (capacity.IsFailure)
        {
            return capacity.Errors[0];
        }

        var branchServiceIds = await _branchServiceReadRepository.Query()
            .AsNoTracking()
            .Where(x => x.ServiceId == service.Id)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);
        if (branchServiceIds.Length == 0)
        {
            return null;
        }

        var totals = await _branchServiceSegmentReadRepository.Query()
            .AsNoTracking()
            .Where(x =>
                branchServiceIds.Contains(x.BranchServiceId) &&
                !x.Segment.IsSystemDefault)
            .GroupBy(x => x.BranchServiceId)
            .Select(x => new
            {
                BranchServiceId = x.Key,
                Total = x.Sum(item => item.Quota)
            })
            .ToListAsync(cancellationToken);
        if (totals.Any(x => x.Total > capacity.Value))
        {
            return new Error(
                "Services.Update.RangeSmallerThanAllocatedSegmentQuotas",
                ServiceFeatureMessages
                    .RangeSmallerThanAllocatedSegmentQuotas,
                ErrorType.Validation);
        }

        var totalByBranchService = totals.ToDictionary(
            x => x.BranchServiceId,
            x => x.Total);
        var defaults = await _branchServiceSegmentReadRepository.Query()
            .AsTracking()
            .Where(x =>
                branchServiceIds.Contains(x.BranchServiceId) &&
                x.Segment.IsSystemDefault)
            .ToListAsync(cancellationToken);
        var defaultBranchServiceIds = defaults
            .Select(x => x.BranchServiceId)
            .ToHashSet();
        var missingDefaultBranchServiceIds = branchServiceIds
            .Where(x => !defaultBranchServiceIds.Contains(x))
            .ToArray();
        if (missingDefaultBranchServiceIds.Length > 0 &&
            (_segmentReadRepository is null ||
             _branchServiceSegmentWriteRepository is null))
        {
            return new Error(
                "BranchServiceSegments.DefaultAssignmentNotFound",
                BranchServiceSegmentMessages.RelationshipNotFound,
                ErrorType.Conflict);
        }

        var now = DateTime.UtcNow;
        foreach (var defaultAssignment in defaults)
        {
            var allocated = totalByBranchService.GetValueOrDefault(
                defaultAssignment.BranchServiceId);
            defaultAssignment.UpdateQuota(
                capacity.Value - allocated,
                _currentUser.UserId!.Value,
                now);
        }

        if (missingDefaultBranchServiceIds.Length > 0)
        {
            var defaultSegmentId = await _segmentReadRepository!.Query()
                .AsNoTracking()
                .Where(x => x.IsSystemDefault)
                .Select(x => x.Id)
                .SingleOrDefaultAsync(cancellationToken);
            if (defaultSegmentId <= 0)
            {
                return new Error(
                    "BranchServiceSegments.DefaultSegmentNotFound",
                    BranchServiceSegmentMessages.SegmentNotFound,
                    ErrorType.Infrastructure);
            }

            var additions = missingDefaultBranchServiceIds
                .Select(branchServiceId =>
                {
                    var allocated = totalByBranchService.GetValueOrDefault(
                        branchServiceId);
                    return BranchServiceSegment.Create(
                        branchServiceId,
                        defaultSegmentId,
                        capacity.Value - allocated,
                        _currentUser.UserId!.Value);
                })
                .ToList();
            await _branchServiceSegmentWriteRepository!.AddRangeAsync(
                additions,
                cancellationToken);
        }

        return null;
    }
}
