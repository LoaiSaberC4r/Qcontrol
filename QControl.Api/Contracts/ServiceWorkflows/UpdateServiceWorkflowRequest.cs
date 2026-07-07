namespace Qcontrol.Api.Contracts.ServiceWorkflows;

public sealed class UpdateServiceWorkflowRequest
{
    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public IReadOnlyList<ServiceWorkflowStepRequest> Steps { get; init; } =
        Array.Empty<ServiceWorkflowStepRequest>();

    public string RowVersion { get; init; } = string.Empty;
}
