namespace Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;

public sealed record UpdateWaitingAreaResponse
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public int Number { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public string? DescriptiveName { get; init; }

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }

    public string Message { get; init; } = string.Empty;
}
