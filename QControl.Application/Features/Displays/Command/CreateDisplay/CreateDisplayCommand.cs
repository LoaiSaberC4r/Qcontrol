using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Displays.Command.CreateDisplay;

public sealed record CreateDisplayCommand
    : ICommand<CreateDisplayResponse>
{
    public int BranchId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;
}
