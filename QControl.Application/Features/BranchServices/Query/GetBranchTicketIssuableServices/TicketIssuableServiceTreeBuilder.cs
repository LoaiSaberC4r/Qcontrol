using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

internal static class TicketIssuableServiceTreeBuilder
{
    public static IReadOnlyList<TicketIssuableServiceTreeNodeResponse> Build(
        IReadOnlyCollection<ServiceHierarchyItem> allItems,
        IReadOnlySet<int> eligibleLeafIds)
    {
        if (allItems.Count == 0 || eligibleLeafIds.Count == 0)
        {
            return Array.Empty<TicketIssuableServiceTreeNodeResponse>();
        }

        var itemsById = new Dictionary<int, ServiceHierarchyItem>(allItems.Count);
        foreach (var item in allItems)
        {
            itemsById.TryAdd(item.Id, item);
        }

        var includedServiceIds = CollectRequiredServiceIds(
            itemsById,
            eligibleLeafIds);

        if (includedServiceIds.Count == 0)
        {
            return Array.Empty<TicketIssuableServiceTreeNodeResponse>();
        }

        var includedItems = includedServiceIds
            .Where(itemsById.ContainsKey)
            .Select(serviceId => itemsById[serviceId])
            .ToArray();
        var childrenByParentId = includedItems
            .Where(item => item.ParentServiceId.HasValue)
            .GroupBy(item => item.ParentServiceId!.Value)
            .ToDictionary(
                group => group.Key,
                group => Order(group).ToArray());
        var roots = Order(includedItems.Where(item =>
            !item.ParentServiceId.HasValue ||
            !includedServiceIds.Contains(item.ParentServiceId.Value)));
        var emittedServiceIds = new HashSet<int>();
        var result = new List<TicketIssuableServiceTreeNodeResponse>();

        foreach (var root in roots)
        {
            var node = BuildNode(
                root,
                childrenByParentId,
                eligibleLeafIds,
                emittedServiceIds,
                new HashSet<int>());

            if (node is not null)
            {
                result.Add(node);
            }
        }

        return result;
    }

    private static HashSet<int> CollectRequiredServiceIds(
        IReadOnlyDictionary<int, ServiceHierarchyItem> itemsById,
        IReadOnlySet<int> eligibleLeafIds)
    {
        var includedServiceIds = new HashSet<int>();

        foreach (var eligibleLeafId in eligibleLeafIds)
        {
            var currentId = eligibleLeafId;
            var pathIds = new HashSet<int>();

            while (pathIds.Add(currentId) &&
                   itemsById.TryGetValue(currentId, out var current))
            {
                includedServiceIds.Add(currentId);

                if (!current.ParentServiceId.HasValue)
                {
                    break;
                }

                currentId = current.ParentServiceId.Value;
            }
        }

        return includedServiceIds;
    }

    private static TicketIssuableServiceTreeNodeResponse? BuildNode(
        ServiceHierarchyItem item,
        IReadOnlyDictionary<int, ServiceHierarchyItem[]> childrenByParentId,
        IReadOnlySet<int> eligibleLeafIds,
        ISet<int> emittedServiceIds,
        ISet<int> pathIds)
    {
        if (!pathIds.Add(item.Id) || !emittedServiceIds.Add(item.Id))
        {
            return null;
        }

        var children = new List<TicketIssuableServiceTreeNodeResponse>();

        if (childrenByParentId.TryGetValue(item.Id, out var childItems))
        {
            foreach (var childItem in childItems)
            {
                var child = BuildNode(
                    childItem,
                    childrenByParentId,
                    eligibleLeafIds,
                    emittedServiceIds,
                    pathIds);

                if (child is not null)
                {
                    children.Add(child);
                }
            }
        }

        pathIds.Remove(item.Id);
        var isEligibleLeaf = eligibleLeafIds.Contains(item.Id);

        return new TicketIssuableServiceTreeNodeResponse
        {
            ServiceId = item.Id,
            ArabicName = item.ArabicName,
            EnglishName = item.EnglishName,
            ArabicUserMessage = isEligibleLeaf
                ? item.ArabicUserMessage
                : null,
            EnglishUserMessage = isEligibleLeaf
                ? item.EnglishUserMessage
                : null,
            IsTicketIssuable = isEligibleLeaf,
            RangePrefix = isEligibleLeaf ? item.RangePrefix : null,
            RangeStartNumber = isEligibleLeaf
                ? item.RangeStartNumber
                : null,
            RangeEndNumber = isEligibleLeaf
                ? item.RangeEndNumber
                : null,
            Children = children
        };
    }

    private static IOrderedEnumerable<ServiceHierarchyItem> Order(
        IEnumerable<ServiceHierarchyItem> items) =>
        items
            .OrderBy(item => item.OrderNo)
            .ThenBy(item => item.ArabicName)
            .ThenBy(item => item.Id);
}
