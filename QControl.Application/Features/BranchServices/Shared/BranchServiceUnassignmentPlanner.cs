using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServices.Shared;

internal static class BranchServiceUnassignmentPlanner
{
    public static Error? TryBuildPlan(
        int leafServiceId,
        IReadOnlyCollection<ServiceHierarchyItem> hierarchyItems,
        IReadOnlySet<int> assignedServiceIds,
        out IReadOnlyList<int> serviceIdsToUnassign)
    {
        serviceIdsToUnassign = Array.Empty<int>();

        var itemsById = hierarchyItems.ToDictionary(x => x.Id);

        if (!itemsById.TryGetValue(leafServiceId, out var selectedLeaf))
        {
            return new Error(
                "BranchServices.Unassign.ServiceNotFound",
                ServiceFeatureMessages.NotFound,
                ErrorType.NotFound);
        }

        var parentIds = hierarchyItems
            .Where(x => x.ParentServiceId.HasValue)
            .Select(x => x.ParentServiceId!.Value)
            .ToHashSet();

        if (parentIds.Contains(leafServiceId))
        {
            return new Error(
                "BranchServices.Unassign.ServiceNotLeaf",
                ServiceFeatureMessages.BranchServicesUnassignServiceNotLeaf,
                ErrorType.Conflict);
        }

        if (!assignedServiceIds.Contains(leafServiceId))
        {
            return new Error(
                "BranchServices.Unassign.AssignmentNotFound",
                ServiceFeatureMessages
                    .BranchServicesUnassignAssignmentNotFound,
                ErrorType.NotFound);
        }

        foreach (var assignedServiceId in assignedServiceIds)
        {
            if (!itemsById.ContainsKey(assignedServiceId))
            {
                return InvalidHierarchyError();
            }
        }

        var otherAssignedLeafIds = assignedServiceIds
            .Where(serviceId =>
                serviceId != leafServiceId &&
                !parentIds.Contains(serviceId))
            .ToArray();

        var ancestorsRequiredByOtherLeaves = new HashSet<int>();

        foreach (var otherLeafId in otherAssignedLeafIds)
        {
            var hierarchyError = AddPathToRoot(
                otherLeafId,
                itemsById,
                ancestorsRequiredByOtherLeaves);

            if (hierarchyError is not null)
            {
                return hierarchyError;
            }
        }

        var result = new List<int>
        {
            leafServiceId
        };

        var currentParentId = selectedLeaf.ParentServiceId;
        var visited = new HashSet<int>
        {
            selectedLeaf.Id
        };

        while (currentParentId.HasValue)
        {
            if (!visited.Add(currentParentId.Value))
            {
                return InvalidHierarchyError();
            }

            if (!itemsById.TryGetValue(
                    currentParentId.Value,
                    out var currentParent))
            {
                return InvalidHierarchyError();
            }

            if (ancestorsRequiredByOtherLeaves.Contains(currentParent.Id))
            {
                break;
            }

            if (assignedServiceIds.Contains(currentParent.Id))
            {
                result.Add(currentParent.Id);
            }

            currentParentId = currentParent.ParentServiceId;
        }

        serviceIdsToUnassign = result;

        return null;
    }

    private static Error? AddPathToRoot(
        int startServiceId,
        IReadOnlyDictionary<int, ServiceHierarchyItem> itemsById,
        ISet<int> pathServiceIds)
    {
        var currentServiceId = startServiceId;
        var visited = new HashSet<int>();

        while (true)
        {
            if (!visited.Add(currentServiceId))
            {
                return InvalidHierarchyError();
            }

            if (!itemsById.TryGetValue(
                    currentServiceId,
                    out var currentService))
            {
                return InvalidHierarchyError();
            }

            pathServiceIds.Add(currentService.Id);

            if (!currentService.ParentServiceId.HasValue)
            {
                return null;
            }

            currentServiceId = currentService.ParentServiceId.Value;
        }
    }

    private static Error InvalidHierarchyError()
        => new(
            "BranchServices.Unassign.InvalidHierarchy",
            ServiceFeatureMessages
                .BranchServicesUnassignInvalidHierarchy,
            ErrorType.Conflict);
}