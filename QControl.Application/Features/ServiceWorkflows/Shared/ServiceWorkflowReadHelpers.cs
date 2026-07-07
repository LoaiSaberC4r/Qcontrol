using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

internal static class ServiceWorkflowReadHelpers
{
    public static async Task<
        IReadOnlyDictionary<int, IReadOnlyList<ServiceWorkflowContainingServiceResponse>>>
        LoadContainingWorkflowsByServiceAsync(
            IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
            IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
            IReadOnlyCollection<int> serviceIds,
            IReadOnlyDictionary<int, ServiceHierarchyItem> servicesById,
            IReadOnlyDictionary<int, ServiceHierarchyState> serviceStates,
            CancellationToken cancellationToken)
    {
        var emptyResult = serviceIds
            .Distinct()
            .ToDictionary(
                x => x,
                _ => (IReadOnlyList<ServiceWorkflowContainingServiceResponse>)
                    Array.Empty<ServiceWorkflowContainingServiceResponse>());

        if (serviceIds.Count == 0)
        {
            return emptyResult;
        }

        var requestedServiceIds = serviceIds
            .Distinct()
            .ToHashSet();

        var matchingPairs = await stepReadRepository.Query()
            .Where(x => requestedServiceIds.Contains(x.ServiceId))
            .Select(x => new
            {
                x.ServiceId,
                WorkflowId = x.ServiceWorkflowId
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var workflowIds = matchingPairs
            .Select(x => x.WorkflowId)
            .Distinct()
            .ToList();

        if (workflowIds.Count == 0)
        {
            return emptyResult;
        }

        var workflows = await workflowReadRepository.Query()
            .Where(x => workflowIds.Contains(x.Id))
            .Select(x => new ServiceWorkflowBasicProjection
            {
                WorkflowId = x.Id,
                ArabicName = x.ArabicName,
                EnglishName = x.EnglishName,
                IsActive = x.IsActive,
                RowVersion = x.RowVersion,
                CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                LastModifiedByApplicationUserId =
                    x.LastModifiedByApplicationUserId,
                DeactivatedByApplicationUserId =
                    x.DeactivatedByApplicationUserId,
                DeactivatedOnUtc = x.DeactivatedOnUtc,
                ReactivatedByApplicationUserId =
                    x.ReactivatedByApplicationUserId,
                ReactivatedOnUtc = x.ReactivatedOnUtc,
                CreatedOnUtc = x.CreatedOnUtc,
                ModifiedOnUtc = x.ModifiedOnUtc
            })
            .ToListAsync(cancellationToken);

        var workflowsById = workflows.ToDictionary(x => x.WorkflowId);

        var steps = await stepReadRepository.Query()
            .Where(x => workflowIds.Contains(x.ServiceWorkflowId))
            .OrderBy(x => x.ServiceWorkflowId)
            .ThenBy(x => x.StepOrder)
            .Select(x => new ServiceWorkflowStepProjection
            {
                WorkflowId = x.ServiceWorkflowId,
                ServiceId = x.ServiceId,
                StepOrder = x.StepOrder
            })
            .ToListAsync(cancellationToken);

        var stepsByWorkflowId = steps
            .GroupBy(x => x.WorkflowId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyCollection<ServiceWorkflowStepProjection>)
                    x.ToList());

        var workflowIdsByServiceId = matchingPairs
            .GroupBy(x => x.ServiceId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(pair => pair.WorkflowId)
                    .Distinct()
                    .ToList());

        foreach (var serviceId in requestedServiceIds)
        {
            if (!workflowIdsByServiceId.TryGetValue(
                    serviceId,
                    out var containingWorkflowIds))
            {
                continue;
            }

            var containingWorkflows = containingWorkflowIds
                .Where(workflowsById.ContainsKey)
                .Select(workflowId =>
                    ServiceWorkflowResponseFactory.ToContainingService(
                        workflowsById[workflowId],
                        stepsByWorkflowId.TryGetValue(
                            workflowId,
                            out var workflowSteps)
                            ? workflowSteps
                            : Array.Empty<ServiceWorkflowStepProjection>(),
                        servicesById,
                        serviceStates,
                        serviceId))
                .OrderBy(x => x.ArabicName)
                .ThenBy(x => x.WorkflowId)
                .ToList();

            emptyResult[serviceId] = containingWorkflows;
        }

        return emptyResult;
    }
}
