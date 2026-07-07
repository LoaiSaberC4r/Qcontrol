using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Query.GetServicesTree;

internal sealed class GetServicesTreeQueryHandler
    : IQueryHandler<GetServicesTreeQuery, IReadOnlyList<ServiceTreeNodeResponse>>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetServicesTreeQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        ICurrentUser currentUser)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<IReadOnlyList<ServiceTreeNodeResponse>>> Handle(
        GetServicesTreeQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);

        var includedItems = allItems
            .Where(x => request.IncludeDeleted || !x.IsDeleted)
            .Where(x => request.IncludeInactive || states[x.Id].EffectiveIsActive)
            .ToDictionary(x => x.Id);

        var roots = includedItems.Values
            .Where(x =>
                !x.ParentServiceId.HasValue ||
                !includedItems.ContainsKey(x.ParentServiceId.Value))
            .OrderBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
            .Select(x => BuildNode(x, includedItems, states))
            .ToList();

        return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Ok(roots);
    }

    private static ServiceTreeNodeResponse BuildNode(
        ServiceHierarchyItem item,
        IReadOnlyDictionary<int, ServiceHierarchyItem> includedItems,
        IReadOnlyDictionary<int, ServiceHierarchyState> states)
    {
        var state = states[item.Id];
        var children = includedItems.Values
            .Where(x => x.ParentServiceId == item.Id)
            .OrderBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
            .Select(x => BuildNode(x, includedItems, states))
            .ToList();

        return new ServiceTreeNodeResponse
        {
            Id = item.Id,
            ParentServiceId = item.ParentServiceId,
            ArabicName = item.ArabicName,
            EnglishName = item.EnglishName,
            IsActive = item.IsActive,
            EffectiveIsActive = state.EffectiveIsActive,
            IsTicketIssuable = item.IsTicketIssuable,
            HasChildren = state.HasChildren,
            CanIssueTicket = state.CanIssueTicket,
            IsDeleted = item.IsDeleted,
            OrderNo = item.OrderNo,
            Children = children
        };
    }
}
