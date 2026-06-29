namespace QControl.Application.Shared.Operational;

internal static class RowVersionConverter
{
    public const int SqlServerRowVersionLength = 8;

    public static string ToBase64(byte[] rowVersion) =>
        Convert.ToBase64String(rowVersion);

    public static bool TryDecode(
        string? value,
        out byte[] rowVersion)
    {
        rowVersion = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.StartsWith("\"", StringComparison.Ordinal) &&
            candidate.EndsWith("\"", StringComparison.Ordinal) &&
            candidate.Length >= 2)
        {
            candidate = candidate[1..^1];
        }

        try
        {
            var decoded = Convert.FromBase64String(candidate);
            if (decoded.Length != SqlServerRowVersionLength)
            {
                return false;
            }

            rowVersion = decoded;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
