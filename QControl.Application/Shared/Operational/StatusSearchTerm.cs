namespace QControl.Application.Shared.Operational;

internal static class StatusSearchTerm
{
    public static bool TryParse(
        string? value,
        out bool isActive)
    {
        isActive = false;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = Normalize(value);

        if (normalized is "inactive" or "\u063a\u064a\u0631 \u0646\u0634\u0637")
        {
            isActive = false;
            return true;
        }

        if (normalized is "active" or "\u0646\u0634\u0637")
        {
            isActive = true;
            return true;
        }

        return false;
    }

    private static string Normalize(string value)
    {
        return string.Join(
                ' ',
                value.Trim()
                    .ToLowerInvariant()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Trim();
    }
}
