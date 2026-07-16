namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

internal static class ServiceWorkflowNameNormalizer
{
    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
