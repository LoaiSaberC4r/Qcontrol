using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.ServiceImages.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Application.Shared.Security;
using QControl.Application.Shared.Validation;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceImages.Command.UploadServiceIconImage;

internal sealed class UploadServiceIconImageCommandHandler
    : ICommandHandler<UploadServiceIconImageCommand, ServiceImageResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly IWriteRepository<ServiceImage> _imageWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadServiceIconImageCommandHandler> _logger;

    public UploadServiceIconImageCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IWriteReadRepository<ServiceImage> imageReadRepository,
        IWriteRepository<ServiceImage> imageWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<UploadServiceIconImageCommandHandler> logger)
        : this(
            serviceReadRepository,
            serviceWriteRepository,
            imageReadRepository,
            imageWriteRepository,
            concurrencyTokenManager,
            currentUser,
            AllowAllServiceDefinitionAccessValidator.Instance,
            mediaService,
            unitOfWork,
            logger)
    {
    }

    public UploadServiceIconImageCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IWriteReadRepository<ServiceImage> imageReadRepository,
        IWriteRepository<ServiceImage> imageWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IServiceDefinitionAccessValidator accessValidator,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<UploadServiceIconImageCommandHandler> logger)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _serviceWriteRepository = serviceWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceWriteRepository));
        _imageReadRepository = imageReadRepository
            ?? throw new ArgumentNullException(nameof(imageReadRepository));
        _imageWriteRepository = imageWriteRepository
            ?? throw new ArgumentNullException(nameof(imageWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
        _mediaService = mediaService
            ?? throw new ArgumentNullException(nameof(mediaService));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ServiceImageResponse>> Handle(
        UploadServiceIconImageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceImageResponse>.Fail(
                ServiceImageOperationHelpers.AuthenticationRequired("Icon"));
        }

        var validationError = BranchImageFileValidator.Validate(
            request.Image,
            "Services.Images.Required",
            ServiceFeatureMessages.ImageRequired,
            "Services.Images.InvalidFileType",
            ServiceFeatureMessages.InvalidImageType);

        if (validationError is not null)
        {
            return Result<ServiceImageResponse>.Fail(validationError);
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<ServiceImageResponse>.Fail(
                ServiceImageOperationHelpers.InvalidRowVersion("Icon"));
        }

        var loaded = await ServiceImageOperationHelpers.LoadServiceForMutationAsync(
            _serviceReadRepository,
            request.ServiceId,
            "Icon",
            cancellationToken);

        if (loaded.IsFailure)
        {
            return Result<ServiceImageResponse>.Fail(loaded.Errors);
        }

        var service = loaded.Value;
        var editAccess = _accessValidator.EnsureCanEdit(
            service,
            "ServiceImages");

        if (editAccess.IsFailure)
        {
            return Result<ServiceImageResponse>.Fail(editAccess.Errors);
        }

        var existingImages = await _imageReadRepository.ListAsync(
            new GetServiceImagesSpec(
                service.Id,
                ServiceImageType.Icon,
                useTracking: true),
            cancellationToken);
        var oldPaths = existingImages
            .Select(x => x.ImagePath)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        string newPath;
        try
        {
            newPath = await _mediaService.SaveAsync(
                request.Image!,
                $"Services/{service.Id}/Icon");
        }
        catch
        {
            return Result<ServiceImageResponse>.Fail(new Error(
                "Services.Images.Icon.MediaSaveFailed",
                ServiceFeatureMessages.MediaSaveFailed,
                ErrorType.Infrastructure));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(service, rowVersion);

        var serviceImage = existingImages.FirstOrDefault();
        if (serviceImage is null)
        {
            serviceImage = ServiceImage.Create(
                service.Id,
                newPath,
                ServiceImageType.Icon,
                displayOrder: 0,
                _currentUser.UserId.Value);

            await _imageWriteRepository.AddAsync(serviceImage, cancellationToken);
        }
        else
        {
            serviceImage.Replace(
                newPath,
                displayOrder: 0,
                _currentUser.UserId.Value);

            _imageWriteRepository.Update(serviceImage);

            if (existingImages.Count > 1)
            {
                _imageWriteRepository.DeleteRange(existingImages.Skip(1));
            }
        }

        service.Touch(_currentUser.UserId.Value);
        _serviceWriteRepository.Update(service);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            SafeRemove(newPath);
            return Result<ServiceImageResponse>.Fail(
                ServiceImageOperationHelpers.ConcurrencyConflict("Icon"));
        }
        catch (DbUpdateException)
        {
            SafeRemove(newPath);
            return Result<ServiceImageResponse>.Fail(new Error(
                "Services.Images.Icon.Conflict",
                ServiceFeatureMessages.ImageConflict,
                ErrorType.Conflict));
        }
        catch
        {
            SafeRemove(newPath);
            return Result<ServiceImageResponse>.Fail(new Error(
                "Services.Images.Icon.PersistenceFailed",
                ServiceFeatureMessages.MediaSaveFailed,
                ErrorType.Infrastructure));
        }

        SafeRemoveRange(oldPaths);

        return Result<ServiceImageResponse>.Ok(
            ServiceImageResponseFactory.FromEntity(
                serviceImage,
                ServiceFeatureMessages.IconUploaded));
    }

    private void SafeRemove(string path)
    {
        try
        {
            _mediaService.Remove(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Compensating service icon cleanup failed.");
        }
    }

    private void SafeRemoveRange(IEnumerable<string> paths)
    {
        try
        {
            _mediaService.RemoveRange(paths);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Old service icon cleanup failed.");
        }
    }
}
