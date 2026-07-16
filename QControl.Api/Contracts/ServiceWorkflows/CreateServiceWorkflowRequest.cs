namespace Qcontrol.Api.Contracts.ServiceWorkflows;

public sealed class CreateServiceWorkflowRequest
{
    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public IReadOnlyList<ServiceWorkflowStepRequest> Steps { get; init; } =
        Array.Empty<ServiceWorkflowStepRequest>();
}
