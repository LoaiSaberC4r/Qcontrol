using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Services;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Command.CreateService;

internal sealed class CreateServiceCommandHandler
    : ICommandHandler<CreateServiceCommand, ServiceResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IServiceTicketUsageChecker _ticketUsageChecker;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IServiceTicketUsageChecker ticketUsageChecker,
        ICurrentUser currentUser,
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

        var normalizedArabicName = request.ArabicName.Trim();
        var normalizedEnglishName = request.EnglishName.Trim();
        var normalizedRangePrefix = string.IsNullOrWhiteSpace(request.RangePrefix)
            ? null
            : request.RangePrefix.Trim();
        var requestedTicketIssuable = request.IsTicketIssuable.GetValueOrDefault();

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
                excludedServiceId: null,
                operation: "Create",
                cancellationToken);

        if (duplicateError is not null)
        {
            return Result<ServiceResponse>.Fail(duplicateError);
        }

        var service = Service.Create(
            parentServiceId: request.ParentServiceId,
            arabicName: normalizedArabicName,
            englishName: normalizedEnglishName,
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
