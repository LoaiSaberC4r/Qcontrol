using System.Net;
using System.Net.Sockets;

namespace Qcontrol.Application.Features.Windows.Shared;

internal static class WindowIPAddressValidation
{
    public static bool BeValidIPAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Contains(':'))
        {
            return IPAddress.TryParse(normalizedValue, out var ipv6) &&
                   ipv6.AddressFamily == AddressFamily.InterNetworkV6;
        }

        var parts = normalizedValue.Split('.');

        return parts.Length == 4 &&
               parts.All(part =>
                   part.Length > 0 &&
                   byte.TryParse(part, out _));
    }
}
