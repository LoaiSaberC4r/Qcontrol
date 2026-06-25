namespace Qcontrol.Application.Features.Displays.Command.UpdateDisplay;

public sealed record UpdateDisplayResponse
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }

    public string Message { get; init; } = string.Empty;
}
