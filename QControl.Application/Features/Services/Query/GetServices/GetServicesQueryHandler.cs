using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceImages.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Query.GetServices;

internal sealed class GetServicesQueryHandler
    : IQueryHandler<GetServicesQuery, Pagination<ServiceResponse>>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService> _branchServiceReadRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;
    private readonly IServiceVisibilityPolicy _visibilityPolicy;

    public GetServicesQueryHandler(
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

    public async Task<Result<Pagination<ServiceResponse>>> Handle(
        GetServicesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<ServiceResponse>>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (request.IsDeleted == true &&
            !_currentBranchContext.IsSystemLevelActor)
        {
            return Result<Pagination<ServiceResponse>>.Fail(new Error(
                "Services.List.IncludeDeletedForbidden",
                ServiceFeatureMessages.ForeignBranchServiceForbidden,
                ErrorType.Security));
        }

        var visibilityContext = _visibilityPolicy.EnsureCanUseVisibilityContext(
            "Services.List");
        if (visibilityContext.IsFailure)
        {
            return Result<Pagination<ServiceResponse>>.Fail(
                visibilityContext.Errors);
        }

        if (request.IsAssignedToCurrentBranch.HasValue &&
            !_currentBranchContext.ActiveBranchId.HasValue)
        {
            return Result<Pagination<ServiceResponse>>.Fail(new Error(
                "Services.List.ActiveBranchRequired",
                ServiceFeatureMessages.ActiveBranchRequired,
                ErrorType.Security));
        }

        request.SearchText ??= string.Empty;

        var query = BuildFilteredQuery(request);

        var totalCount = await query.CountAsync(cancellationToken);

        var pageItems = await query
            .OrderBy(x => x.ParentServiceId ?? 0)
            .ThenBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ServiceProjection.ToHierarchyItem)
            .ToListAsync(cancellationToken);

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        allItems = allItems
            .Where(x => _visibilityPolicy.CanView(x.Scope, x.OwnerBranchId))
            .ToList();
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var imagesByServiceId =
            await LoadImagesForPageAsync(pageItems, cancellationToken);
        var accessContext = await BuildAccessContextAsync(
            pageItems.Select(x => x.Id),
            cancellationToken);

        var responses = pageItems
            .Select(item =>
            {
                imagesByServiceId.TryGetValue(
                    item.Id,
                    out var images);

                return ServiceResponseFactory.FromItem(
                    item,
                    states[item.Id],
                    accessContext,
                    images);
            })
            .ToList();

        var response = new Pagination<ServiceResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: responses);

        return Result<Pagination<ServiceResponse>>.Ok(response);
    }

    private IQueryable<Service> BuildFilteredQuery(GetServicesQuery request)
    {
        var query = _serviceReadRepository.Query()
            .IgnoreQueryFilters();

        query = _visibilityPolicy.ApplyVisibleServices(query);

        if (request.IsDeleted == true)
        {
            query = query.Where(x => x.IsDeleted);
        }
        else if (request.IsDeleted == false)
        {
            query = query.Where(x => !x.IsDeleted);
        }
        else
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (request.IsActive.HasValue)
        {
            var isActive = request.IsActive.Value;
            query = query.Where(x => x.IsActive == isActive);
        }

        if (request.IsTicketIssuable.HasValue)
        {
            var isTicketIssuable = request.IsTicketIssuable.Value;
            query = query.Where(x => x.IsTicketIssuable == isTicketIssuable);
        }

        if (request.ParentServiceId.HasValue)
        {
            var parentServiceId = request.ParentServiceId.Value;
            query = query.Where(x => x.ParentServiceId == parentServiceId);
        }

        if (request.Scope.HasValue)
        {
            var scope = request.Scope.Value;
            query = query.Where(x => x.Scope == scope);
        }

        if (request.OwnerBranchId.HasValue)
        {
            var ownerBranchId = request.OwnerBranchId.Value;
            query = query.Where(x => x.OwnerBranchId == ownerBranchId);
        }

        if (request.IsAssignedToCurrentBranch.HasValue &&
            _currentBranchContext.ActiveBranchId.HasValue)
        {
            var activeBranchId = _currentBranchContext.ActiveBranchId.Value;
            var isAssigned = request.IsAssignedToCurrentBranch.Value;
            var branchServices = _branchServiceReadRepository.Query();

            query = isAssigned
                ? query.Where(x => branchServices.Any(bs =>
                    bs.BranchId == activeBranchId &&
                    bs.ServiceId == x.Id))
                : query.Where(x => !branchServices.Any(bs =>
                    bs.BranchId == activeBranchId &&
                    bs.ServiceId == x.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            query = query.Where(x =>
                x.ArabicName.Contains(searchText) ||
                x.EnglishName.Contains(searchText) ||
                (x.RangePrefix != null &&
                    x.RangePrefix.Contains(searchText)));
        }

        return query;
    }

    private async Task<ServiceResponseAccessContext> BuildAccessContextAsync(
        IEnumerable<int> serviceIds,
        CancellationToken cancellationToken)
    {
        var ids = serviceIds.ToArray();
        var assignedServiceIds = Array.Empty<int>();

        if (ids.Length > 0 &&
            _currentBranchContext.ActiveBranchId.HasValue)
        {
            var activeBranchId = _currentBranchContext.ActiveBranchId.Value;
            assignedServiceIds = await _branchServiceReadRepository.Query()
                .Where(x =>
                    x.BranchId == activeBranchId &&
                    ids.Contains(x.ServiceId))
                .Select(x => x.ServiceId)
                .ToArrayAsync(cancellationToken);
        }

        return new ServiceResponseAccessContext(
            _currentBranchContext.IsSystemLevelActor,
            _currentBranchContext.IsBranchActor,
            _currentBranchContext.ActiveBranchId,
            assignedServiceIds);
    }

    private async Task<IReadOnlyDictionary<int, ServiceImagesForResponse>>
        LoadImagesForPageAsync(
            IReadOnlyCollection<ServiceHierarchyItem> pageItems,
            CancellationToken cancellationToken)
    {
        if (pageItems.Count == 0)
        {
            return new Dictionary<int, ServiceImagesForResponse>();
        }

        var images = await _imageReadRepository.ListAsync(
            new GetServiceImagesForServicesSpec(
                pageItems.Select(x => x.Id),
                includeAds: false),
            cancellationToken);

        return ServiceImageResponseFactory.GroupForServices(
            images,
            includeAds: false);
    }
}
