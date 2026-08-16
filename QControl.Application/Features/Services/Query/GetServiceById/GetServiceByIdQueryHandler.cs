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
    private readonly IServiceVisibilityPolicy _visibilityPolicy;

    public GetServiceByIdQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IWriteReadRepository<ServiceImage> imageReadRepository,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext,
        IServiceVisibilityPolicy visibilityPolicy)
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
        _visibilityPolicy = visibilityPolicy
            ?? throw new ArgumentNullException(nameof(visibilityPolicy));
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

        var visibility = _visibilityPolicy.EnsureCanView(
            item.Scope,
            item.OwnerBranchId,
            "Services.GetById");
        if (visibility.IsFailure)
        {
            return Result<ServiceDetailsResponse>.Fail(visibility.Errors);
        }

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        allItems = allItems
            .Where(x => _visibilityPolicy.CanView(x.Scope, x.OwnerBranchId))
            .ToList();
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var byId = allItems.ToDictionary(x => x.Id);

        byId.TryGetValue(item.ParentServiceId ?? 0, out var parent);

        var images = await _imageReadRepository.ListAsync(
            new GetServiceImagesSpec(item.Id),
            cancellationToken);
        var imagesForResponse = ServiceImageResponseFactory.ToImagesForResponse(
            images,
            includeAds: true);
        var state = states[item.Id];
        var customInputs = !state.HasChildren && item.IsClientInputRequired
            ? await LoadCustomInputsAsync(item.Id, cancellationToken)
            : null;

        var response = ServiceResponseFactory.ToDetails(
            item,
            state,
            parent,
            await BuildAccessContextAsync(item.Id, cancellationToken),
            imagesForResponse,
            customInputs);

        return Result<ServiceDetailsResponse>.Ok(response);
    }

    private async Task<IReadOnlyList<ServiceCustomInputResponse>>
        LoadCustomInputsAsync(
            int serviceId,
            CancellationToken cancellationToken)
    {
        var items = await _serviceReadRepository.Query()
            .AsNoTracking()
            .Where(x => x.Id == serviceId)
            .SelectMany(x => x.CustomInputs)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Id)
            .Select(x => new ServiceCustomInputReadModel
            {
                ServiceId = x.ServiceId,
                CustomInputId = x.Id,
                Name = x.Name,
                LabelEn = x.LabelEn,
                LabelAr = x.LabelAr,
                Type = x.Type,
                IsRequired = x.IsRequired,
                MinLength = x.MinLength,
                MaxLength = x.MaxLength,
                MinValue = x.MinValue,
                MaxValue = x.MaxValue,
                StartWith = x.StartWith,
                Order = x.Order
            })
            .ToListAsync(cancellationToken);

        return items
            .Select(ServiceCustomInputResponseFactory.FromReadModel)
            .ToList();
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
