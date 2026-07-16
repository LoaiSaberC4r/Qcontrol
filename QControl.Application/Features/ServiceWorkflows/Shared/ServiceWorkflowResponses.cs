using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

public sealed class ServiceWorkflowStepCommandItem
{
    public int ServiceId { get; init; }

    public int StepOrder { get; init; }
}

public sealed class ServiceWorkflowStepResponse
{
    public int StepOrder { get; init; }

    public int ServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public int? ParentServiceId { get; init; }

    public string? ParentArabicName { get; init; }

    public string? ParentEnglishName { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public bool EffectiveIsActive { get; init; }

    public bool IsTicketIssuable { get; init; }

    public bool IsMatchedService { get; init; }
}

public sealed class ServiceWorkflowResponse
{
    public int WorkflowId { get; init; }

    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public string OwnerServiceArabicName { get; init; } = string.Empty;

    public string OwnerServiceEnglishName { get; init; } = string.Empty;

    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public bool IsDefault { get; init; }

    public bool IsActive { get; init; }

    public int StepsCount { get; init; }

    public int? StartServiceId { get; init; }

    public IReadOnlyList<ServiceWorkflowStepResponse> Steps { get; init; } =
        Array.Empty<ServiceWorkflowStepResponse>();

    public string RowVersion { get; init; } = string.Empty;

    public Guid CreatedByApplicationUserId { get; init; }

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public Guid? DeactivatedByApplicationUserId { get; init; }

    public DateTime? DeactivatedOnUtc { get; init; }

    public Guid? ReactivatedByApplicationUserId { get; init; }

    public DateTime? ReactivatedOnUtc { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }

    public string? Message { get; init; }
}

public sealed class ServiceWorkflowListItemResponse
{
    public int WorkflowId { get; init; }

    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public bool IsDefault { get; init; }

    public int? StartServiceId { get; init; }

    public string? StartServiceArabicName { get; init; }

    public string? StartServiceEnglishName { get; init; }

    public int StepsCount { get; init; }

    public bool IsActive { get; init; }

    public IReadOnlyList<ServiceWorkflowStepResponse> Steps { get; init; } =
        Array.Empty<ServiceWorkflowStepResponse>();

    public string RowVersion { get; init; } = string.Empty;

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}

public sealed class ServiceWorkflowStartOptionsResponse
{
    public int ServiceId { get; init; }

    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public bool HasWorkflows { get; init; }

    public int? DefaultWorkflowId { get; init; }

    public string Mode { get; init; } = string.Empty;

    public bool RequiresWorkflowSelection { get; init; }

    public int? AutoApplyWorkflowId { get; init; }

    public IReadOnlyList<ServiceWorkflowStartOptionItemResponse> Workflows
    {
        get; init;
    } = Array.Empty<ServiceWorkflowStartOptionItemResponse>();
}

public sealed class ServiceWorkflowStartOptionItemResponse
{
    public int WorkflowId { get; init; }

    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public bool IsActive { get; init; }

    public bool IsDefault { get; init; }

    public int? StartServiceId { get; init; }

    public int StepsCount { get; init; }

    public IReadOnlyList<ServiceWorkflowStepResponse> Steps { get; init; } =
        Array.Empty<ServiceWorkflowStepResponse>();
}

public sealed class ServiceWorkflowCandidateServiceResponse
{
    public int ServiceId { get; init; }

    public int? ParentServiceId { get; init; }

    public string? ParentArabicName { get; init; }

    public string? ParentEnglishName { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public bool EffectiveIsActive { get; init; }

    public bool IsTicketIssuable { get; init; }
}

public sealed class ServiceWorkflowActivationResponse
{
    public int WorkflowId { get; init; }

    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public bool IsDefault { get; init; }

    public bool IsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}

internal sealed class ServiceWorkflowBasicProjection
{
    public int WorkflowId { get; init; }

    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public bool IsDefault { get; init; }

    public bool IsActive { get; init; }

    public byte[] RowVersion { get; init; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; init; }

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public Guid? DeactivatedByApplicationUserId { get; init; }

    public DateTime? DeactivatedOnUtc { get; init; }

    public Guid? ReactivatedByApplicationUserId { get; init; }

    public DateTime? ReactivatedOnUtc { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}

internal sealed class ServiceWorkflowStepProjection
{
    public int WorkflowId { get; init; }

    public int ServiceId { get; init; }

    public int StepOrder { get; init; }
}

internal static class ServiceWorkflowResponseFactory
{
    public static ServiceWorkflowResponse ToDetails(
        ServiceWorkflowBasicProjection workflow,
        IReadOnlyCollection<ServiceWorkflowStepProjection> steps,
        IReadOnlyDictionary<int, ServiceHierarchyItem> servicesById,
        IReadOnlyDictionary<int, ServiceHierarchyState> serviceStates,
        string? message = null)
    {
        var orderedSteps = steps
            .OrderBy(x => x.StepOrder)
            .ThenBy(x => x.ServiceId)
            .Select(x => ToStepResponse(
                x,
                servicesById,
                serviceStates,
                matchedServiceId: null))
            .ToList();

        return new ServiceWorkflowResponse
        {
            WorkflowId = workflow.WorkflowId,
            BranchId = workflow.BranchId,
            LeafServiceId = workflow.LeafServiceId,
            OwnerServiceArabicName = servicesById.TryGetValue(
                workflow.LeafServiceId,
                out var ownerService)
                ? ownerService.ArabicName
                : string.Empty,
            OwnerServiceEnglishName = ownerService?.EnglishName ??
                string.Empty,
            ArabicName = workflow.ArabicName,
            EnglishName = workflow.EnglishName,
            IsDefault = workflow.IsDefault,
            IsActive = workflow.IsActive,
            StepsCount = orderedSteps.Count,
            StartServiceId = orderedSteps.FirstOrDefault()?.ServiceId,
            Steps = orderedSteps,
            RowVersion = RowVersionConverter.ToBase64(workflow.RowVersion),
            CreatedByApplicationUserId = workflow.CreatedByApplicationUserId,
            LastModifiedByApplicationUserId =
                workflow.LastModifiedByApplicationUserId,
            DeactivatedByApplicationUserId =
                workflow.DeactivatedByApplicationUserId,
            DeactivatedOnUtc = workflow.DeactivatedOnUtc,
            ReactivatedByApplicationUserId =
                workflow.ReactivatedByApplicationUserId,
            ReactivatedOnUtc = workflow.ReactivatedOnUtc,
            CreatedOnUtc = workflow.CreatedOnUtc,
            ModifiedOnUtc = workflow.ModifiedOnUtc,
            Message = message
        };
    }

    public static ServiceWorkflowStepResponse ToStepResponse(
        ServiceWorkflowStepProjection step,
        IReadOnlyDictionary<int, ServiceHierarchyItem> servicesById,
        IReadOnlyDictionary<int, ServiceHierarchyState> serviceStates,
        int? matchedServiceId)
    {
        servicesById.TryGetValue(step.ServiceId, out var service);

        ServiceHierarchyItem? parent = null;
        if (service?.ParentServiceId is int parentServiceId)
        {
            servicesById.TryGetValue(parentServiceId, out parent);
        }

        var state = service is not null &&
            serviceStates.TryGetValue(service.Id, out var currentState)
                ? currentState
                : null;

        return new ServiceWorkflowStepResponse
        {
            StepOrder = step.StepOrder,
            ServiceId = step.ServiceId,
            ArabicName = service?.ArabicName ?? string.Empty,
            EnglishName = service?.EnglishName ?? string.Empty,
            ParentServiceId = service?.ParentServiceId,
            ParentArabicName = parent?.ArabicName,
            ParentEnglishName = parent?.EnglishName,
            IsActive = service?.IsActive ?? false,
            IsDeleted = service?.IsDeleted ?? false,
            EffectiveIsActive = state?.EffectiveIsActive ?? false,
            IsTicketIssuable = service?.IsTicketIssuable ?? false,
            IsMatchedService = matchedServiceId.HasValue &&
                step.ServiceId == matchedServiceId.Value
        };
    }
}
