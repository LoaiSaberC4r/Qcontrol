using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceImages.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Query.GetServices;

internal sealed class GetServicesQueryHandler
    : IQueryHandler<GetServicesQuery, Pagination<ServiceResponse>>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<ServiceImage> _imageReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetServicesQueryHandler(
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
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var imagesByServiceId =
            await LoadImagesForPageAsync(pageItems, cancellationToken);

        var responses = pageItems
            .Select(item =>
            {
                imagesByServiceId.TryGetValue(
                    item.Id,
                    out var images);

                return ServiceResponseFactory.FromItem(
                    item,
                    states[item.Id],
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
        var query = _serviceReadRepository.Query();

        if (request.IsDeleted == true)
        {
            query = query
                .IgnoreQueryFilters()
                .Where(x => x.IsDeleted);
        }
        else if (request.IsDeleted == false)
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
