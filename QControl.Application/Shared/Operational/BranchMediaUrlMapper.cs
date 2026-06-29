namespace QControl.Application.Shared.Operational;

internal static class BranchMediaUrlMapper
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
