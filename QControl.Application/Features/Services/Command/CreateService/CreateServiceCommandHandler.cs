using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Abstraction.Services;
using QControl.Application.Shared.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Command.CreateService;

internal sealed class CreateServiceCommandHandler
    : ICommandHandler<CreateServiceCommand, ServiceResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IServiceTicketUsageChecker _ticketUsageChecker;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IServiceTicketUsageChecker ticketUsageChecker,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
        : this(
            serviceReadRepository,
            serviceWriteRepository,
            ticketUsageChecker,
            currentUser,
            SystemLevelServiceBranchContext.Instance,
            AllowAllServiceDefinitionAccessValidator.Instance,
            unitOfWork)
    {
    }

    public CreateServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IServiceTicketUsageChecker ticketUsageChecker,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext,
        IServiceDefinitionAccessValidator accessValidator,
        IUnitOfWork unitOfWork)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _serviceWriteRepository = serviceWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceWriteRepository));
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
        CreateServiceCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceResponse>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var createAccess = _accessValidator.EnsureCanCreateGlobal(
            "Services.GlobalCreate");

        if (createAccess.IsFailure)
        {
            return Result<ServiceResponse>.Fail(createAccess.Errors);
        }

        var normalizedArabicName = request.ArabicName.Trim();
        var normalizedEnglishName = request.EnglishName.Trim();
        var normalizedServiceCode =
            ServiceCodeNormalizer.Normalize(request.ServiceCode);
        var normalizedRangePrefix = string.IsNullOrWhiteSpace(request.RangePrefix)
            ? null
            : request.RangePrefix.Trim();
        var requestedTicketIssuable = request.IsTicketIssuable.GetValueOrDefault();

        var customInputError = ServiceCustomInputRuleChecks.Validate(
            request.CustomInputs?
                .Cast<ServiceCustomInputDefinitionCommand>()
                .ToArray(),
            request.IsClientInputRequired,
            "Create",
            validateIds: false);
        if (customInputError is not null)
        {
            return Result<ServiceResponse>.Fail(customInputError);
        }

        var duplicateServiceCodeError =
            await ServiceRuleChecks.ValidateServiceCodeIsUniqueAsync(
                _serviceReadRepository,
                normalizedServiceCode,
                excludedServiceId: null,
                "Services.Create.ServiceCodeAlreadyExists",
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
            operation: "Create");

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
                currentServiceId: null,
                ServiceScope.Global,
                expectedOwnerBranchId: null,
                operation: "Create",
                cancellationToken);

            if (parentError is not null)
            {
                return Result<ServiceResponse>.Fail(parentError);
            }
        }

        var duplicateError =
            await ServiceRuleChecks.ValidateDuplicateNamesAsync(
                _serviceReadRepository,
                normalizedArabicName,
                normalizedEnglishName,
                request.ParentServiceId,
                ServiceScope.Global,
                ownerBranchId: null,
                excludedServiceId: null,
                operation: "Create",
                cancellationToken);

        if (duplicateError is not null)
        {
            return Result<ServiceResponse>.Fail(duplicateError);
        }

        var service = Service.CreateGlobal(
            parentServiceId: request.ParentServiceId,
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
            createdByApplicationUserId: _currentUser.UserId.Value);

        foreach (var customInput in request.CustomInputs ??
                 Array.Empty<CreateServiceCustomInputCommand>())
        {
            service.AddCustomInput(
                customInput.Name,
                customInput.LabelEn,
                customInput.LabelAr,
                customInput.Type,
                customInput.IsRequired,
                customInput.MinLength,
                customInput.MaxLength,
                customInput.MinValue,
                customInput.MaxValue,
                customInput.StartWith,
                customInput.Order,
                _currentUser.UserId.Value);
        }

        await _serviceWriteRepository.AddAsync(
            service,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ServiceUniqueConstraintErrorMapper.TryMapCreate(
                ex,
                out var error))
        {
            return Result<ServiceResponse>.Fail(error);
        }

        var response = await ServiceRuleChecks.BuildServiceResponseAsync(
            _serviceReadRepository,
            service.Id,
            new ServiceResponseAccessContext(
                _currentBranchContext.IsSystemLevelActor,
                _currentBranchContext.IsBranchActor,
                _currentBranchContext.ActiveBranchId,
                assignedServiceIds: Array.Empty<int>()),
            ServiceFeatureMessages.CreateSuccess,
            cancellationToken);

        return response is null
            ? Result<ServiceResponse>.Fail(new Error(
                "Services.Create.NotFoundAfterSave",
                ServiceFeatureMessages.NotFound,
                ErrorType.Infrastructure))
            : Result<ServiceResponse>.Ok(response);
    }
}
