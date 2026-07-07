using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Command.RestoreService;

internal sealed class RestoreServiceCommandHandler
    : ICommandHandler<RestoreServiceCommand, ServiceRestoreResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _serviceWriteRepository = serviceWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ServiceRestoreResponse>> Handle(
        RestoreServiceCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceRestoreResponse>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Result<ServiceRestoreResponse>.Fail(new Error(
                "Services.Restore.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var service = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceForMutationSpec(request.Id),
            cancellationToken);

        if (service is null)
        {
            return Result<ServiceRestoreResponse>.Fail(new Error(
                "Services.Restore.NotFound",
                ServiceFeatureMessages.NotFound,
                ErrorType.NotFound));
        }

        if (!service.IsDeleted)
        {
            return Result<ServiceRestoreResponse>.Fail(new Error(
                "Services.Restore.NotDeleted",
                ServiceFeatureMessages.NotDeleted,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            service,
            rowVersion);

        service.Restore(
            _dateTimeProvider.UtcNow,
            _currentUser.UserId.Value);

        _serviceWriteRepository.Update(service);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ServiceRestoreResponse>.Fail(new Error(
                "Services.Restore.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        var response = await ServiceRuleChecks.BuildRestoreResponseAsync(
            _serviceReadRepository,
            service.Id,
            ServiceFeatureMessages.RestoreSuccess,
            cancellationToken);

        return response is null
            ? Result<ServiceRestoreResponse>.Fail(new Error(
                "Services.Restore.NotFoundAfterSave",
                ServiceFeatureMessages.NotFound,
                ErrorType.Infrastructure))
            : Result<ServiceRestoreResponse>.Ok(response);
    }
}
