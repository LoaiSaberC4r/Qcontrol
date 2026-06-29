namespace Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;

public sealed record CreateWaitingAreaResponse
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public int Number { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public string? DescriptiveName { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public Guid CreatedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public string Message { get; init; } = string.Empty;
}
