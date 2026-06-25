namespace Qcontrol.Application.Features.Terminals.Query.GetTerminalById;

public sealed record TerminalDetailsResponse
{
    public int Id { get; init; }

    public int WindowId { get; init; }

    public string WindowNumber { get; init; } = string.Empty;

    public int WaitingAreaId { get; init; }

    public int WaitingAreaNumber { get; init; }

    public int BranchId { get; init; }

    public string? BranchArabicName { get; init; }

    public string? BranchEnglishName { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}
