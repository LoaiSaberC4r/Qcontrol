namespace Qcontrol.Application.Features.Services.Shared;

using QControl.Domain.Enums;

internal sealed class ServiceHierarchyItem
{
    public int Id { get; init; }

    public int? ParentServiceId { get; init; }

    public ServiceScope Scope { get; init; }

    public int? OwnerBranchId { get; init; }

    public string? OwnerBranchArabicName { get; init; }

    public string? OwnerBranchEnglishName { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ServiceCode { get; init; }

    public bool IsServiceCodeRequired { get; init; }

    public string? ArabicUserMessage { get; init; }

    public string? EnglishUserMessage { get; init; }

    public bool IsActive { get; init; }

    public bool IsTicketIssuable { get; init; }

    public bool IsClientInputRequired { get; init; }

    public bool HasReservation { get; init; }

    public int OrderNo { get; init; }

    public int Priority { get; init; }

    public string? RangePrefix { get; init; }

    public int? RangeStartNumber { get; init; }

    public int? RangeEndNumber { get; init; }

    public int? WaitingDuration { get; init; }

    public int? NoOfTicketCopies { get; init; }

    public byte[] RowVersion { get; init; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; init; }

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public bool IsDeleted { get; init; }

    public DateTime? DeletedOnUtc { get; init; }

    public DateTime? RestoredOnUtc { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}

internal sealed class ServiceHierarchyState
{
    public bool HasChildren { get; init; }

    public bool EffectiveIsActive { get; init; }

    public bool CanIssueTicket { get; init; }
}

internal static class ServiceHierarchyCalculator
{
    public static IReadOnlyDictionary<int, ServiceHierarchyState> ComputeStates(
        IReadOnlyCollection<ServiceHierarchyItem> items)
    {
        var byId = items.ToDictionary(x => x.Id);
        var parentIds = items
            .Where(x => x.ParentServiceId.HasValue)
            .Select(x => x.ParentServiceId!.Value)
            .ToHashSet();
        var effectiveCache = new Dictionary<int, bool>();
        var states = new Dictionary<int, ServiceHierarchyState>(items.Count);

        foreach (var item in items)
        {
            var hasChildren = parentIds.Contains(item.Id);
            var effective = IsEffectivelyActive(
                item,
                byId,
                effectiveCache,
                new HashSet<int>());

            states[item.Id] = new ServiceHierarchyState
            {
                HasChildren = hasChildren,
                EffectiveIsActive = effective,
                CanIssueTicket =
                    effective &&
                    item.IsTicketIssuable &&
                    !hasChildren
            };
        }

        return states;
    }

    public static IReadOnlySet<int> GetDescendantIds(
        IReadOnlyCollection<ServiceHierarchyItem> items,
        int serviceId)
    {
        var childrenByParent = items
            .Where(x => x.ParentServiceId.HasValue)
            .GroupBy(x => x.ParentServiceId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.Select(child => child.Id).ToList());

        var descendants = new HashSet<int>();
        var queue = new Queue<int>();

        if (childrenByParent.TryGetValue(serviceId, out var children))
        {
            foreach (var childId in children)
            {
                queue.Enqueue(childId);
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!descendants.Add(current))
            {
                continue;
            }

            if (!childrenByParent.TryGetValue(current, out var nestedChildren))
            {
                continue;
            }

            foreach (var childId in nestedChildren)
            {
                queue.Enqueue(childId);
            }
        }

        return descendants;
    }

    private static bool IsEffectivelyActive(
        ServiceHierarchyItem item,
        IReadOnlyDictionary<int, ServiceHierarchyItem> byId,
        IDictionary<int, bool> cache,
        ISet<int> visiting)
    {
        if (cache.TryGetValue(item.Id, out var cached))
        {
            return cached;
        }

        if (!visiting.Add(item.Id))
        {
            cache[item.Id] = false;
            return false;
        }

        var effective = !item.IsDeleted && item.IsActive;

        if (effective &&
            item.ParentServiceId.HasValue &&
            byId.TryGetValue(item.ParentServiceId.Value, out var parent))
        {
            effective = IsEffectivelyActive(
                parent,
                byId,
                cache,
                visiting);
        }

        visiting.Remove(item.Id);
        cache[item.Id] = effective;

        return effective;
    }
}

internal static class ServiceHierarchySearch
{
    public static IReadOnlyDictionary<int, ServiceHierarchyItem> Apply(
        IReadOnlyDictionary<int, ServiceHierarchyItem> includedItems,
        string? searchText)
    {
        var normalizedSearchText = searchText?.Trim();
        if (string.IsNullOrEmpty(normalizedSearchText))
        {
            return includedItems;
        }

        var childrenByParentId = includedItems.Values
            .Where(x => x.ParentServiceId.HasValue)
            .GroupBy(x => x.ParentServiceId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.Select(child => child.Id).ToArray());
        var directMatches = includedItems.Values
            .Where(x => Matches(x, normalizedSearchText))
            .Select(x => x.Id)
            .ToArray();

        if (directMatches.Length == 0)
        {
            return new Dictionary<int, ServiceHierarchyItem>();
        }

        var retainedIds = new HashSet<int>(directMatches);

        foreach (var matchId in directMatches)
        {
            var currentId = matchId;
            var visitedAncestorIds = new HashSet<int>();

            while (includedItems.TryGetValue(currentId, out var current) &&
                   current.ParentServiceId.HasValue &&
                   visitedAncestorIds.Add(currentId) &&
                   includedItems.TryGetValue(
                       current.ParentServiceId.Value,
                       out var parent))
            {
                retainedIds.Add(parent.Id);
                currentId = parent.Id;
            }
        }

        var descendantQueue = new Queue<int>(directMatches);
        var expandedIds = new HashSet<int>();

        while (descendantQueue.Count > 0)
        {
            var currentId = descendantQueue.Dequeue();
            if (!expandedIds.Add(currentId) ||
                !childrenByParentId.TryGetValue(currentId, out var childIds))
            {
                continue;
            }

            foreach (var childId in childIds)
            {
                retainedIds.Add(childId);
                descendantQueue.Enqueue(childId);
            }
        }

        return includedItems
            .Where(x => retainedIds.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public static bool Matches(
        ServiceHierarchyItem item,
        string searchText)
        => item.ArabicName.Contains(searchText, StringComparison.Ordinal) ||
           item.EnglishName.Contains(
               searchText,
               StringComparison.OrdinalIgnoreCase) ||
           (item.ServiceCode?.Contains(
               searchText,
               StringComparison.OrdinalIgnoreCase) ?? false);
}
