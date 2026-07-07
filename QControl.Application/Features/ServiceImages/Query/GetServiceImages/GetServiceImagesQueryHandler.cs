using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.ServiceImages.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceImages.Query.GetServiceImages;

internal sealed class GetServiceImagesQueryHandler
    : IQueryHandler<GetServiceImagesQuery, IReadOnlyList<ServiceImageResponse>>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetServiceImagesQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<ServiceImage> imageReadRepository,
        ICurrentUser currentUser)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _imageReadRepository = imageReadRepository
            ?? throw new ArgumentNullException(nameof(imageReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<IReadOnlyList<ServiceImageResponse>>> Handle(
        GetServiceImagesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(
                ServiceImageOperationHelpers.AuthenticationRequired("Get"));
        }

        var service = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceHierarchyItemByIdSpec(request.ServiceId),
            cancellationToken);

        if (service is null)
        {
            return Result<IReadOnlyList<ServiceImageResponse>>.Fail(
                new Error(
                    "Services.Images.Get.ServiceNotFound",
                    ServiceFeatureMessages.NotFound,
                    ErrorType.NotFound));
        }

        var images = await _imageReadRepository.ListAsync(
            new GetServiceImagesSpec(
                request.ServiceId,
                request.ImageType),
            cancellationToken);

        return Result<IReadOnlyList<ServiceImageResponse>>.Ok(
            images
                .Select(x => ServiceImageResponseFactory.FromEntity(x))
                .ToList());
    }
}
