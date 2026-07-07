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
using QControl.Application.Shared.Validation;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceImages.Command.UploadServiceAdsImages;

internal sealed class UploadServiceAdsImagesCommandHandler
    : ICommandHandler<UploadServiceAdsImagesCommand, IReadOnlyList<ServiceImageResponse>>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly IWriteRepository<ServiceImage> _imageWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadServiceAdsImagesCommandHandler> _logger;

    public UploadServiceAdsImagesCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IWriteReadRepository<ServiceImage> imageReadRepository,
        IWriteRepository<ServiceImage> imageWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<UploadServiceAdsImagesCommandHandler> logger)
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

    public async Task<Result<IReadOnlyList<ServiceImageResponse>>> Handle(
        UploadServiceAdsImagesCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(
                ServiceImageOperationHelpers.AuthenticationRequired("Ads"));
        }

        if (request.Images.Count == 0)
        {
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(new Error(
                "Services.Images.Ads.Required",
                ServiceFeatureMessages.ImagesRequired,
                ErrorType.Validation));
        }

        foreach (var image in request.Images)
        {
            var validationError = BranchImageFileValidator.Validate(
                image,
                "Services.Images.Ads.Required",
                ServiceFeatureMessages.ImagesRequired,
                "Services.Images.InvalidFileType",
                ServiceFeatureMessages.InvalidImageType);

            if (validationError is not null)
            {
                return Result<IReadOnlyList<ServiceImageResponse>>.Fail(
                    validationError);
            }
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(
                ServiceImageOperationHelpers.InvalidRowVersion("Ads"));
        }

        var loaded = await ServiceImageOperationHelpers.LoadServiceForMutationAsync(
            _serviceReadRepository,
            request.ServiceId,
            "Ads",
            cancellationToken);

        if (loaded.IsFailure)
        {
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(loaded.Errors);
        }

        var service = loaded.Value;
        var existingAds = await _imageReadRepository.ListAsync(
            new GetServiceImagesSpec(
                service.Id,
                ServiceImageType.Ads),
            cancellationToken);
        var nextDisplayOrder =
            existingAds.Select(x => x.DisplayOrder).DefaultIfEmpty(0).Max() + 1;

        List<string> savedPaths;
        try
        {
            savedPaths = await _mediaService.SaveAsync(
                request.Images,
                $"Services/{service.Id}/Ads");
        }
        catch
        {
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(new Error(
                "Services.Images.Ads.MediaSaveFailed",
                ServiceFeatureMessages.MediaSaveFailed,
                ErrorType.Infrastructure));
        }

        var serviceImages = savedPaths
            .Select((path, index) => ServiceImage.Create(
                service.Id,
                path,
                ServiceImageType.Ads,
                nextDisplayOrder + index,
                _currentUser.UserId.Value))
            .ToList();

        _concurrencyTokenManager.SetOriginalRowVersion(service, rowVersion);

        await _imageWriteRepository.AddRangeAsync(
            serviceImages,
            cancellationToken);

        service.Touch(_currentUser.UserId.Value);
        _serviceWriteRepository.Update(service);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            SafeRemoveRange(savedPaths);
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(
                ServiceImageOperationHelpers.ConcurrencyConflict("Ads"));
        }
        catch
        {
            SafeRemoveRange(savedPaths);
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(new Error(
                "Services.Images.Ads.PersistenceFailed",
                ServiceFeatureMessages.MediaSaveFailed,
                ErrorType.Infrastructure));
        }

        return Result<IReadOnlyList<ServiceImageResponse>>.Ok(
            serviceImages
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .Select(x => ServiceImageResponseFactory.FromEntity(
                    x,
                    ServiceFeatureMessages.AdsUploaded))
                .ToList());
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
                "Compensating service ads cleanup failed.");
        }
    }
}
