using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Displays.Command.UpdateDisplay;

public sealed record UpdateDisplayCommand
    : ICommand<UpdateDisplayResponse>
{
    public int Id { get; init; }

    public int RequestId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;
}
