using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

internal sealed class GetServiceWorkflowForMutationSpec
    : Specification<ServiceWorkflow>
{
    public GetServiceWorkflowForMutationSpec(
        int branchId,
        int leafServiceId,
        int workflowId)
    {
        UseTracking();
        AddCriteria(x =>
            x.Id == workflowId &&
            x.BranchId == branchId &&
            x.LeafServiceId == leafServiceId);
        Include(x => x.Steps);
    }
}

internal sealed class ServiceWorkflowDuplicateArabicNameSpec
    : Specification<ServiceWorkflow, int>
{
    public ServiceWorkflowDuplicateArabicNameSpec(
        int branchId,
        int leafServiceId,
        string arabicName,
        int? excludedWorkflowId = null)
    {
        UseNoTracking();
        AddCriteria(x => x.BranchId == branchId);
        AddCriteria(x => x.LeafServiceId == leafServiceId);
        AddCriteria(x => x.ArabicName == arabicName);

        if (excludedWorkflowId.HasValue)
        {
            var workflowId = excludedWorkflowId.Value;
            AddCriteria(x => x.Id != workflowId);
        }

        Select(x => x.Id);
    }
}

internal sealed class ServiceWorkflowDuplicateEnglishNameSpec
    : Specification<ServiceWorkflow, int>
{
    public ServiceWorkflowDuplicateEnglishNameSpec(
        int branchId,
        int leafServiceId,
        string englishName,
        int? excludedWorkflowId = null)
    {
        UseNoTracking();
        AddCriteria(x => x.BranchId == branchId);
        AddCriteria(x => x.LeafServiceId == leafServiceId);
        AddCriteria(x => x.EnglishName == englishName);

        if (excludedWorkflowId.HasValue)
        {
            var workflowId = excludedWorkflowId.Value;
            AddCriteria(x => x.Id != workflowId);
        }

        Select(x => x.Id);
    }
}
