using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Services;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Command.UpdateService;

internal sealed class UpdateServiceCommandHandler
    : ICommandHandler<UpdateServiceCommand, ServiceResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly IServiceTicketUsageChecker _ticketUsageChecker;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        IServiceTicketUsageChecker ticketUsageChecker,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _serviceWriteRepository = serviceWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _ticketUsageChecker = ticketUsageChecker
            ?? throw new ArgumentNullException(nameof(ticketUsageChecker));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
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
            new GetServiceForMutationSpec(request.Id),
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

        var normalizedArabicName = request.ArabicName.Trim();
        var normalizedEnglishName = request.EnglishName.Trim();
        var normalizedRangePrefix = string.IsNullOrWhiteSpace(request.RangePrefix)
            ? null
            : request.RangePrefix.Trim();
        var requestedTicketIssuable =
            request.IsTicketIssuable.GetValueOrDefault();

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
                service.Id,
                operation: "Update",
                cancellationToken);

        if (duplicateError is not null)
        {
            return Result<ServiceResponse>.Fail(duplicateError);
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            service,
            rowVersion);

        service.ChangeParent(
            request.ParentServiceId,
            _currentUser.UserId.Value);

        service.Update(
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
            ServiceFeatureMessages.UpdateSuccess,
            cancellationToken);

        return response is null
            ? Result<ServiceResponse>.Fail(new Error(
                "Services.Update.NotFoundAfterSave",
                ServiceFeatureMessages.NotFound,
                ErrorType.Infrastructure))
            : Result<ServiceResponse>.Ok(response);
    }
}
