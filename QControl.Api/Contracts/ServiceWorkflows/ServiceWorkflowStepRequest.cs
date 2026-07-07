namespace Qcontrol.Api.Contracts.ServiceWorkflows;

public sealed class ServiceWorkflowStepRequest
{
    public int ServiceId { get; init; }

    public int StepOrder { get; init; }
}
