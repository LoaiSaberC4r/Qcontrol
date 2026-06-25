namespace Qcontrol.Application.Features.Displays.Query.GetDeletedDisplays;

public sealed record DeletedDisplayListItemResponse
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public string? BranchArabicName { get; init; }

    public string? BranchEnglishName { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public DateTime? DeletedOnUtc { get; init; }

    public DateTime? RestoredOnUtc { get; init; }
}
