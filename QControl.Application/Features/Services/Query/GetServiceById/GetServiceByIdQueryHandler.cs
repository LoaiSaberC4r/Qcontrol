using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceImages.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Query.GetServiceById;

internal sealed class GetServiceByIdQueryHandler
    : IQueryHandler<GetServiceByIdQuery, ServiceDetailsResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService> _branchServiceReadRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;

    public GetServiceByIdQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IWriteReadRepository<ServiceImage> imageReadRepository,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(nameof(branchServiceReadRepository));
        _imageReadRepository = imageReadRepository
            ?? throw new ArgumentNullException(nameof(imageReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
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
            await BuildAccessContextAsync(item.Id, cancellationToken),
            imagesForResponse);

        return Result<ServiceDetailsResponse>.Ok(response);
    }

    private async Task<ServiceResponseAccessContext> BuildAccessContextAsync(
        int serviceId,
        CancellationToken cancellationToken)
    {
        var assignedServiceIds = Array.Empty<int>();

        if (_currentBranchContext.ActiveBranchId.HasValue)
        {
            var activeBranchId = _currentBranchContext.ActiveBranchId.Value;
            assignedServiceIds = await _branchServiceReadRepository.Query()
                .Where(x =>
                    x.BranchId == activeBranchId &&
                    x.ServiceId == serviceId)
                .Select(x => x.ServiceId)
                .ToArrayAsync(cancellationToken);
        }

        return new ServiceResponseAccessContext(
            _currentBranchContext.IsSystemLevelActor,
            _currentBranchContext.IsBranchActor,
            _currentBranchContext.ActiveBranchId,
            assignedServiceIds);
    }
}
