using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.ServiceImages.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Query.GetServiceById;

internal sealed class GetServiceByIdQueryHandler
    : IQueryHandler<GetServiceByIdQuery, ServiceDetailsResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetServiceByIdQueryHandler(
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

    public async Task<Result<ServiceDetailsResponse>> Handle(
        GetServiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceDetailsResponse>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var item = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceHierarchyItemByIdSpec(request.Id),
            cancellationToken);

        if (item is null)
        {
            return Result<ServiceDetailsResponse>.Fail(new Error(
                "Services.GetById.NotFound",
                ServiceFeatureMessages.NotFound,
                ErrorType.NotFound));
        }

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var byId = allItems.ToDictionary(x => x.Id);

        byId.TryGetValue(item.ParentServiceId ?? 0, out var parent);

        var images = await _imageReadRepository.ListAsync(
            new GetServiceImagesSpec(item.Id),
            cancellationToken);
        var imagesForResponse = ServiceImageResponseFactory.ToImagesForResponse(
            images,
            includeAds: true);

        var response = ServiceResponseFactory.ToDetails(
            item,
            states[item.Id],
            parent,
            imagesForResponse);

        return Result<ServiceDetailsResponse>.Ok(response);
    }
}
