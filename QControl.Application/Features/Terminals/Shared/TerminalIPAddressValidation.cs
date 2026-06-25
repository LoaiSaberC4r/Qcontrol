using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Qcontrol.Application.Features.Terminals.Shared;

internal static class TerminalIPAddressValidation
{
    public static bool BeValidIPv4(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var ipAddress = value.Trim();

        if (ipAddress.Contains(':'))
        {
            return false;
        }

        var parts = ipAddress.Split('.');

        if (parts.Length != 4)
        {
            return false;
        }

        foreach (var part in parts)
        {
            if (!byte.TryParse(
                    part,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out _))
            {
                return false;
            }
        }

        return IPAddress.TryParse(ipAddress, out var parsed) &&
            parsed.AddressFamily == AddressFamily.InterNetwork &&
            parsed.ToString() == ipAddress;
    }
}
