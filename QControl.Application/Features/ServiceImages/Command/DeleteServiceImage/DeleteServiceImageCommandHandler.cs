using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.ServiceImages.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceImages.Command.DeleteServiceImage;

internal sealed class DeleteServiceImageCommandHandler
    : ICommandHandler<DeleteServiceImageCommand, ServiceImageDeleteResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly IWriteRepository<ServiceImage> _imageWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteServiceImageCommandHandler> _logger;

    public DeleteServiceImageCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<ServiceImage> imageReadRepository,
        IWriteRepository<ServiceImage> imageWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteServiceImageCommandHandler> logger)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _imageReadRepository = imageReadRepository
            ?? throw new ArgumentNullException(nameof(imageReadRepository));
        _imageWriteRepository = imageWriteRepository
            ?? throw new ArgumentNullException(nameof(imageWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _mediaService = mediaService
            ?? throw new ArgumentNullException(nameof(mediaService));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ServiceImageDeleteResponse>> Handle(
        DeleteServiceImageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceImageDeleteResponse>.Fail(
                ServiceImageOperationHelpers.AuthenticationRequired("Delete"));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<ServiceImageDeleteResponse>.Fail(
                ServiceImageOperationHelpers.InvalidRowVersion("Delete"));
        }

        var loaded = await ServiceImageOperationHelpers.LoadServiceForMutationAsync(
            _serviceReadRepository,
            request.ServiceId,
            "Delete",
            cancellationToken);

        if (loaded.IsFailure)
        {
            return Result<ServiceImageDeleteResponse>.Fail(loaded.Errors);
        }

        var image = await _imageReadRepository.FirstOrDefaultAsync(
            new GetServiceImageForMutationSpec(
                request.ServiceId,
                request.ImageId),
            cancellationToken);

        if (image is null)
        {
            return Result<ServiceImageDeleteResponse>.Fail(new Error(
                "Services.Images.Delete.NotFound",
                ServiceFeatureMessages.ImageNotFound,
                ErrorType.NotFound));
        }

        var imagePath = image.ImagePath;
        var imageType = image.ImageType;

        _concurrencyTokenManager.SetOriginalRowVersion(image, rowVersion);
        _imageWriteRepository.Delete(image);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ServiceImageDeleteResponse>.Fail(
                ServiceImageOperationHelpers.ConcurrencyConflict("Delete"));
        }

        try
        {
            _mediaService.Remove(imagePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Service image cleanup failed for service {ServiceId}, image {ImageId}.",
                request.ServiceId,
                request.ImageId);
        }

        return Result<ServiceImageDeleteResponse>.Ok(
            new ServiceImageDeleteResponse
            {
                Id = request.ImageId,
                ServiceId = request.ServiceId,
                ImageType = imageType,
                Message = ServiceFeatureMessages.ImageDeleted
            });
    }
}
