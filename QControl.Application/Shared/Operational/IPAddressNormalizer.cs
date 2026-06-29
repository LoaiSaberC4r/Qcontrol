using System.Net;
using System.Net.Sockets;

namespace QControl.Application.Shared.Operational;

internal static class IPAddressNormalizer
{
    public static bool TryNormalize(
        string? value,
        out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Contains('/') ||
            candidate.Contains("://", StringComparison.Ordinal) ||
            candidate.StartsWith("[", StringComparison.Ordinal) ||
            candidate.EndsWith("]", StringComparison.Ordinal))
        {
            return false;
        }

        if (!IPAddress.TryParse(candidate, out var ipAddress))
        {
            return false;
        }

        if (ipAddress.AddressFamily is not
            (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
        {
            return false;
        }

        normalized = ipAddress.ToString();
        return true;
    }

    public static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return TryNormalize(value, out var normalized)
            ? normalized
            : value.Trim();
    }
}
