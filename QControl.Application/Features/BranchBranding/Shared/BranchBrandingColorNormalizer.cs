using System.Text.RegularExpressions;

namespace Qcontrol.Application.Features.BranchBranding.Shared;

internal static partial class BranchBrandingColorNormalizer
{
    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();
        if (!HexColorRegex().IsMatch(candidate))
        {
            return false;
        }

        normalized = candidate.ToUpperInvariant();
        return true;
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexColorRegex();
}
