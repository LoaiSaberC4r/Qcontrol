using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Application.Shared.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Command.DeleteService;

internal sealed class DeleteServiceCommandHandler
    : ICommandHandler<DeleteServiceCommand, ServiceDeleteResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
        : this(
            serviceReadRepository,
            serviceWriteRepository,
            concurrencyTokenManager,
            currentUser,
            AllowAllServiceDefinitionAccessValidator.Instance,
            dateTimeProvider,
            unitOfWork)
    {
    }

    public DeleteServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IServiceDefinitionAccessValidator accessValidator,
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
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ServiceDeleteResponse>> Handle(
        DeleteServiceCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceDeleteResponse>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Result<ServiceDeleteResponse>.Fail(new Error(
                "Services.Delete.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var service = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceForMutationSpec(request.Id),
            cancellationToken);

        if (service is null)
        {
            return Result<ServiceDeleteResponse>.Fail(new Error(
                "Services.Delete.NotFound",
                ServiceFeatureMessages.NotFound,
                ErrorType.NotFound));
        }

        if (service.IsDeleted)
        {
            return Result<ServiceDeleteResponse>.Fail(new Error(
                "Services.Delete.AlreadyDeleted",
                ServiceFeatureMessages.AlreadyDeleted,
                ErrorType.Conflict));
        }

        var editAccess = _accessValidator.EnsureCanEdit(
            service,
            "Services.Delete");

        if (editAccess.IsFailure)
        {
            return Result<ServiceDeleteResponse>.Fail(editAccess.Errors);
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            service,
            rowVersion);

        service.SoftDelete(
            _dateTimeProvider.UtcNow,
            _currentUser.UserId.Value);

        _serviceWriteRepository.Update(service);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ServiceDeleteResponse>.Fail(new Error(
                "Services.Delete.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<ServiceDeleteResponse>.Ok(new ServiceDeleteResponse
        {
            Id = service.Id,
            IsDeleted = service.IsDeleted,
            IsActive = service.IsActive,
            RowVersion = RowVersionConverter.ToBase64(service.RowVersion),
            Message = ServiceFeatureMessages.DeleteSuccess
        });
    }
}
