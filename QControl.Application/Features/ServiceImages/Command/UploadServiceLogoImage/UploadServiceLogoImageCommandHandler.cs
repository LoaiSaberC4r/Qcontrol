using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.ServiceImages.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Application.Shared.Validation;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceImages.Command.UploadServiceLogoImage;

internal sealed class UploadServiceLogoImageCommandHandler
    : ICommandHandler<UploadServiceLogoImageCommand, ServiceImageResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly IWriteRepository<ServiceImage> _imageWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadServiceLogoImageCommandHandler> _logger;

    public UploadServiceLogoImageCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IWriteReadRepository<ServiceImage> imageReadRepository,
        IWriteRepository<ServiceImage> imageWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<UploadServiceLogoImageCommandHandler> logger)
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
        _mediaService = mediaService
            ?? throw new ArgumentNullException(nameof(mediaService));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ServiceImageResponse>> Handle(
        UploadServiceLogoImageCommand request,
        CancellationToken cancellationToken)
    {
        return await ReplaceSingleImageAsync(
            request.ServiceId,
            request.Image,
            request.RowVersion,
            ServiceImageType.Logo,
            "Logo",
            ServiceFeatureMessages.LogoUploaded,
            cancellationToken);
    }

    private async Task<Result<ServiceImageResponse>> ReplaceSingleImageAsync(
        int serviceId,
        IFormFile? image,
        string rowVersionValue,
        ServiceImageType imageType,
        string operation,
        string successMessage,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceImageResponse>.Fail(
                ServiceImageOperationHelpers.AuthenticationRequired(operation));
        }

        var validationError = BranchImageFileValidator.Validate(
            image,
            "Services.Images.Required",
            ServiceFeatureMessages.ImageRequired,
            "Services.Images.InvalidFileType",
            ServiceFeatureMessages.InvalidImageType);

        if (validationError is not null)
        {
            return Result<ServiceImageResponse>.Fail(validationError);
        }

        if (!RowVersionConverter.TryDecode(rowVersionValue, out var rowVersion))
        {
            return Result<ServiceImageResponse>.Fail(
                ServiceImageOperationHelpers.InvalidRowVersion(operation));
        }

        var loaded = await ServiceImageOperationHelpers.LoadServiceForMutationAsync(
            _serviceReadRepository,
            serviceId,
            operation,
            cancellationToken);

        if (loaded.IsFailure)
        {
            return Result<ServiceImageResponse>.Fail(loaded.Errors);
        }

        var service = loaded.Value;
        var existingImages = await _imageReadRepository.ListAsync(
            new GetServiceImagesSpec(
                service.Id,
                imageType,
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
                image!,
                $"Services/{service.Id}/{imageType}");
        }
        catch
        {
            return Result<ServiceImageResponse>.Fail(new Error(
                $"Services.Images.{operation}.MediaSaveFailed",
                ServiceFeatureMessages.MediaSaveFailed,
                ErrorType.Infrastructure));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            service,
            rowVersion);

        var serviceImage = existingImages.FirstOrDefault();
        if (serviceImage is null)
        {
            serviceImage = ServiceImage.Create(
                service.Id,
                newPath,
                imageType,
                displayOrder: 0,
                _currentUser.UserId.Value);

            await _imageWriteRepository.AddAsync(
                serviceImage,
                cancellationToken);
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
                ServiceImageOperationHelpers.ConcurrencyConflict(operation));
        }
        catch (DbUpdateException)
        {
            SafeRemove(newPath);
            return Result<ServiceImageResponse>.Fail(new Error(
                $"Services.Images.{operation}.Conflict",
                ServiceFeatureMessages.ImageConflict,
                ErrorType.Conflict));
        }
        catch
        {
            SafeRemove(newPath);
            return Result<ServiceImageResponse>.Fail(new Error(
                $"Services.Images.{operation}.PersistenceFailed",
                ServiceFeatureMessages.MediaSaveFailed,
                ErrorType.Infrastructure));
        }

        SafeRemoveRange(oldPaths);

        return Result<ServiceImageResponse>.Ok(
            ServiceImageResponseFactory.FromEntity(
                serviceImage,
                successMessage));
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
                "Compensating service image cleanup failed for {Path}.",
                path);
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
                "Old service image cleanup failed.");
        }
    }
}
