namespace Qcontrol.Api.Contracts.ServiceSchedules;

public sealed class UpdateServiceScheduleRequest
{
    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    public IReadOnlyCollection<DayOfWeek> WorkDays { get; init; } =
        Array.Empty<DayOfWeek>();

    public bool IsSlotCodeRequired { get; init; }

    public string? SlotCode { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
