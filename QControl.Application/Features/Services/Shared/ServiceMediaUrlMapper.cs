namespace Qcontrol.Application.Features.Services.Shared;

internal static class ServiceMediaUrlMapper
{
    public static string? ToMediaUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var normalized = relativePath.Trim().Replace('\\', '/').TrimStart('/');

        if (normalized.StartsWith("Media/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["Media/".Length..];
        }

        return $"/Media/{normalized}";
    }
}
