using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

internal sealed class GetServiceWorkflowForMutationSpec
    : Specification<ServiceWorkflow>
{
    public GetServiceWorkflowForMutationSpec(int workflowId)
    {
        UseTracking();
        AddCriteria(x => x.Id == workflowId);
        Include(x => x.Steps);
    }
}

internal sealed class ServiceWorkflowDuplicateArabicNameSpec
    : Specification<ServiceWorkflow, int>
{
    public ServiceWorkflowDuplicateArabicNameSpec(
        string arabicName,
        int? excludedWorkflowId = null)
    {
        UseNoTracking();
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
        string englishName,
        int? excludedWorkflowId = null)
    {
        UseNoTracking();
        AddCriteria(x => x.EnglishName == englishName);

        if (excludedWorkflowId.HasValue)
        {
            var workflowId = excludedWorkflowId.Value;
            AddCriteria(x => x.Id != workflowId);
        }

        Select(x => x.Id);
    }
}
