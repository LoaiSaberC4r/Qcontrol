namespace Qcontrol.Application.Features.Terminals.Command.DeactivateTerminal;

public sealed record DeactivateTerminalResponse
{
    public int Id { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
