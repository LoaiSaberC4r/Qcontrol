namespace Qcontrol.Application.Features.Displays.Command.CreateDisplay;

public sealed record CreateDisplayResponse
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public Guid CreatedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public string Message { get; init; } = string.Empty;
}
