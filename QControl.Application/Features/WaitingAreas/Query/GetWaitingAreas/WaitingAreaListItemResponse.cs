namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;

public sealed record WaitingAreaListItemResponse
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public string? BranchArabicName { get; init; }

    public string? BranchEnglishName { get; init; }

    public int Number { get; init; }

    public string? DescriptiveName { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public int WindowsCount { get; init; }
}
