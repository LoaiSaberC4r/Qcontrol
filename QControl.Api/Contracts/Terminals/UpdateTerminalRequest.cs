namespace Qcontrol.Api.Contracts.Terminals;

public sealed class UpdateTerminalRequest
{
    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string RowVersion { get; init; } = string.Empty;
}
