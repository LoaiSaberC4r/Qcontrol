namespace Qcontrol.Api.Contracts.ServiceWorkflows;

public sealed class UpdateServiceWorkflowRequest
{
    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public IReadOnlyList<ServiceWorkflowStepRequest> Steps { get; init; } =
        Array.Empty<ServiceWorkflowStepRequest>();

    public string RowVersion { get; init; } = string.Empty;
}
