using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Command.ApproveServiceGlobalizationRequest;

internal sealed class ApproveServiceGlobalizationRequestCommandHandler
    : ICommandHandler<
        ApproveServiceGlobalizationRequestCommand,
        ApproveServiceGlobalizationRequestResponse>
{
    private readonly IWriteReadRepository<ServiceGlobalizationRequest>
        _requestReadRepository;
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveServiceGlobalizationRequestCommandHandler(
        IWriteReadRepository<ServiceGlobalizationRequest> requestReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext,
        IDateTimeProvider dateTimeProvider,
        IConcurrencyTokenManager concurrencyTokenManager,
        IUnitOfWork unitOfWork)
    {
        _requestReadRepository = requestReadRepository
            ?? throw new ArgumentNullException(nameof(requestReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ApproveServiceGlobalizationRequestResponse>>
        Handle(
            ApproveServiceGlobalizationRequestCommand request,
            CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "ServiceGlobalizationRequests.Approve.Unauthenticated",
                ServiceGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!_currentBranchContext.IsSystemLevelActor)
        {
            return Failure(
                "ServiceGlobalizationRequests.Approve.TechnicalAdminRequired",
                ServiceGlobalizationRequestMessages.TechnicalAdminRequired,
                ErrorType.Security);
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Failure(
                "ServiceGlobalizationRequests.Approve.InvalidRowVersion",
                ServiceGlobalizationRequestMessages.InvalidRowVersion,
                ErrorType.Validation);
        }

        var entity = await _requestReadRepository.Query()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == request.RequestId,
                cancellationToken);

        if (entity is null)
        {
            return Failure(
                "ServiceGlobalizationRequests.Approve.NotFound",
                ServiceGlobalizationRequestMessages.RequestNotFound,
                ErrorType.NotFound);
        }

        request.BranchIdForInvalidation = entity.BranchId;

        var statusError = ValidatePendingStatus(
            entity.Status,
            "ServiceGlobalizationRequests.Approve");
        if (statusError is not null)
        {
            return Result<ApproveServiceGlobalizationRequestResponse>.Fail(
                statusError);
        }

        _concurrencyTokenManager.SetOriginalRowVersion(entity, rowVersion);

        var itemServiceIds = entity.Items
            .Select(x => x.ServiceId)
            .Distinct()
            .ToArray();

        if (itemServiceIds.Length == 0 ||
            !itemServiceIds.Contains(entity.RootServiceId))
        {
            return Failure(
                "ServiceGlobalizationRequests.Approve.RequestItemMismatch",
                ServiceGlobalizationRequestMessages.RequestItemMismatch,
                ErrorType.Conflict);
        }

        var services = await _serviceReadRepository.Query()
            .IgnoreQueryFilters()
            .Where(x => itemServiceIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (services.Count != itemServiceIds.Length)
        {
            return Failure(
                "ServiceGlobalizationRequests.Approve.SubmittedServiceNotFound",
                ServiceGlobalizationRequestMessages.SubmittedServiceNotFound,
                ErrorType.Conflict);
        }

        var serviceValidation = ValidateSubmittedServices(
            entity,
            services);
        if (serviceValidation is not null)
        {
            return Result<ApproveServiceGlobalizationRequestResponse>.Fail(
                serviceValidation);
        }

        var hierarchyValidation = await ValidateHierarchyAsync(
            entity,
            services,
            cancellationToken);
        if (hierarchyValidation is not null)
        {
            return Result<ApproveServiceGlobalizationRequestResponse>.Fail(
                hierarchyValidation);
        }

        var duplicateValidation = await ValidateDuplicateGlobalNamesAsync(
            services,
            cancellationToken);
        if (duplicateValidation is not null)
        {
            return Result<ApproveServiceGlobalizationRequestResponse>.Fail(
                duplicateValidation);
        }

        var reviewedOnUtc = _dateTimeProvider.UtcNow;
        var reviewerId = _currentUser.UserId.Value;

        foreach (var service in services)
        {
            service.PromoteToGlobal(reviewedOnUtc, reviewerId);
        }

        entity.Approve(reviewerId, reviewedOnUtc);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure(
                "ServiceGlobalizationRequests.Approve.ConcurrencyConflict",
                ServiceGlobalizationRequestMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }
        catch (DbUpdateException ex)
            when (ServiceGlobalizationRequestUniqueConstraintErrorMapper
                .TryMapReview(ex, out var error))
        {
            return Result<ApproveServiceGlobalizationRequestResponse>.Fail(
                error);
        }
        catch (DbUpdateException)
        {
            return Failure(
                "ServiceGlobalizationRequests.Approve.PersistenceConflict",
                ServiceGlobalizationRequestMessages.RequestItemMismatch,
                ErrorType.Conflict);
        }

        var promotedIds = services
            .Select(x => x.Id)
            .OrderBy(x => x)
            .ToArray();
        request.PromotedServiceIdsForInvalidation = promotedIds;

        return Result<ApproveServiceGlobalizationRequestResponse>.Ok(
            new ApproveServiceGlobalizationRequestResponse
            {
                RequestId = entity.Id,
                Status = entity.Status,
                BranchId = entity.BranchId,
                PromotedServiceIds = promotedIds,
                ReviewedByApplicationUserId = reviewerId,
                ReviewedOnUtc = reviewedOnUtc,
                RowVersion = RowVersionConverter.ToBase64(entity.RowVersion),
                Message =
                    ServiceGlobalizationRequestMessages.ApprovedSuccessfully
            });
    }

    private static Error? ValidateSubmittedServices(
        ServiceGlobalizationRequest request,
        IReadOnlyCollection<Service> services)
    {
        foreach (var service in services)
        {
            if (service.IsDeleted)
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.SubmittedServiceDeleted",
                    ServiceGlobalizationRequestMessages.SubmittedServiceDeleted,
                    ErrorType.Conflict);
            }

            if (service.Scope != ServiceScope.BranchScoped)
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.ScopeChanged",
                    ServiceGlobalizationRequestMessages
                        .SubmittedServiceScopeChanged,
                    ErrorType.Conflict);
            }

            if (service.OwnerBranchId != request.BranchId)
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.OwnerChanged",
                    ServiceGlobalizationRequestMessages
                        .SubmittedServiceOwnerChanged,
                    ErrorType.Conflict);
            }
        }

        return null;
    }

    private async Task<Error?> ValidateHierarchyAsync(
        ServiceGlobalizationRequest request,
        IReadOnlyCollection<Service> services,
        CancellationToken cancellationToken)
    {
        var servicesById = services.ToDictionary(x => x.Id);
        var serviceIds = servicesById.Keys.ToHashSet();

        if (!servicesById.TryGetValue(
                request.RootServiceId,
                out var rootService))
        {
            return new Error(
                "ServiceGlobalizationRequests.Approve.RequestItemMismatch",
                ServiceGlobalizationRequestMessages.RequestItemMismatch,
                ErrorType.Conflict);
        }

        if (request.RequestType ==
            ServiceGlobalizationRequestType.BranchServiceTree &&
            rootService.ParentServiceId.HasValue)
        {
            return InvalidHierarchy();
        }

        if (request.RequestType ==
            ServiceGlobalizationRequestType.LeafUnderGlobalParent)
        {
            if (services.Count != 1)
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.RequestItemMismatch",
                    ServiceGlobalizationRequestMessages.RequestItemMismatch,
                    ErrorType.Conflict);
            }

            if (!rootService.ParentServiceId.HasValue ||
                serviceIds.Contains(rootService.ParentServiceId.Value))
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.GlobalParentRequired",
                    ServiceGlobalizationRequestMessages.GlobalParentRequired,
                    ErrorType.Conflict);
            }
        }

        var outsideParentIds = services
            .Where(x =>
                x.ParentServiceId.HasValue &&
                !serviceIds.Contains(x.ParentServiceId.Value))
            .Select(x => x.ParentServiceId!.Value)
            .Distinct()
            .ToArray();

        var outsideParents = outsideParentIds.Length == 0
            ? new Dictionary<int, ParentProjection>()
            : await _serviceReadRepository.Query()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => outsideParentIds.Contains(x.Id))
                .Select(x => new ParentProjection
                {
                    Id = x.Id,
                    Scope = x.Scope,
                    IsDeleted = x.IsDeleted
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var service in services)
        {
            if (!service.ParentServiceId.HasValue)
            {
                continue;
            }

            var parentId = service.ParentServiceId.Value;
            if (serviceIds.Contains(parentId))
            {
                continue;
            }

            if (!outsideParents.TryGetValue(parentId, out var parent))
            {
                return InvalidHierarchy();
            }

            if (parent.IsDeleted)
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.GlobalParentDeleted",
                    ServiceGlobalizationRequestMessages.GlobalParentDeleted,
                    ErrorType.Conflict);
            }

            if (parent.Scope != ServiceScope.Global)
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.GlobalParentChanged",
                    ServiceGlobalizationRequestMessages.GlobalParentChanged,
                    ErrorType.Conflict);
            }
        }

        return null;
    }

    private async Task<Error?> ValidateDuplicateGlobalNamesAsync(
        IReadOnlyCollection<Service> services,
        CancellationToken cancellationToken)
    {
        var submittedIds = services
            .Select(x => x.Id)
            .ToHashSet();
        var parentIds = services
            .Select(x => x.ParentServiceId)
            .Distinct()
            .ToArray();
        var arabicNames = services
            .Select(x => x.ArabicName)
            .Distinct()
            .ToArray();
        var englishNames = services
            .Select(x => x.EnglishName)
            .Distinct()
            .ToArray();

        var candidates = await _serviceReadRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Scope == ServiceScope.Global &&
                !submittedIds.Contains(x.Id) &&
                parentIds.Contains(x.ParentServiceId) &&
                (arabicNames.Contains(x.ArabicName) ||
                 englishNames.Contains(x.EnglishName)))
            .Select(x => new DuplicateCandidateProjection
            {
                Id = x.Id,
                ParentServiceId = x.ParentServiceId,
                ArabicName = x.ArabicName,
                EnglishName = x.EnglishName
            })
            .ToListAsync(cancellationToken);

        foreach (var service in services)
        {
            var duplicateArabic = candidates.Any(x =>
                x.ParentServiceId == service.ParentServiceId &&
                string.Equals(
                    x.ArabicName,
                    service.ArabicName,
                    StringComparison.OrdinalIgnoreCase));

            if (duplicateArabic)
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.DuplicateArabicName",
                    ServiceGlobalizationRequestMessages
                        .DuplicateGlobalArabicName,
                    ErrorType.Conflict);
            }

            var duplicateEnglish = candidates.Any(x =>
                x.ParentServiceId == service.ParentServiceId &&
                string.Equals(
                    x.EnglishName,
                    service.EnglishName,
                    StringComparison.OrdinalIgnoreCase));

            if (duplicateEnglish)
            {
                return new Error(
                    "ServiceGlobalizationRequests.Approve.DuplicateEnglishName",
                    ServiceGlobalizationRequestMessages
                        .DuplicateGlobalEnglishName,
                    ErrorType.Conflict);
            }
        }

        return null;
    }

    private static Error? ValidatePendingStatus(
        ServiceGlobalizationRequestStatus status,
        string codePrefix)
    {
        return status switch
        {
            ServiceGlobalizationRequestStatus.Pending => null,
            ServiceGlobalizationRequestStatus.Approved => new Error(
                $"{codePrefix}.AlreadyApproved",
                ServiceGlobalizationRequestMessages.AlreadyApproved,
                ErrorType.Conflict),
            ServiceGlobalizationRequestStatus.Rejected => new Error(
                $"{codePrefix}.AlreadyRejected",
                ServiceGlobalizationRequestMessages.AlreadyRejected,
                ErrorType.Conflict),
            _ => new Error(
                $"{codePrefix}.NotPending",
                ServiceGlobalizationRequestMessages.NotPending,
                ErrorType.Conflict)
        };
    }

    private static Error InvalidHierarchy()
    {
        return new Error(
            "ServiceGlobalizationRequests.Approve.InvalidHierarchy",
            ServiceGlobalizationRequestMessages.SubmittedHierarchyInvalid,
            ErrorType.Conflict);
    }

    private static Result<ApproveServiceGlobalizationRequestResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<ApproveServiceGlobalizationRequestResponse>.Fail(
            new Error(code, message, type));

    private sealed class ParentProjection
    {
        public int Id { get; init; }

        public ServiceScope Scope { get; init; }

        public bool IsDeleted { get; init; }
    }

    private sealed class DuplicateCandidateProjection
    {
        public int Id { get; init; }

        public int? ParentServiceId { get; init; }

        public string ArabicName { get; init; } = string.Empty;

        public string EnglishName { get; init; } = string.Empty;
    }
}
