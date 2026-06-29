namespace Qcontrol.Application.Features.Terminals.Shared;

internal sealed record TerminalWindowContext
{
    public int WindowId { get; init; }

    public int WaitingAreaId { get; init; }

    public int BranchId { get; init; }

    public string WindowNumber { get; init; } = string.Empty;

    public int WaitingAreaNumber { get; init; }

    public bool BranchIsActive { get; init; }

    public bool WaitingAreaIsActive { get; init; }

    public bool WindowIsActive { get; init; }
}
