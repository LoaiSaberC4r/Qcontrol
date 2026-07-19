namespace Qcontrol.Api.Contracts.ServiceSchedules;

public sealed class CreateServiceScheduleRequest
{
    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    public IReadOnlyCollection<DayOfWeek> WorkDays { get; init; } =
        Array.Empty<DayOfWeek>();

    public bool IsSlotCodeRequired { get; init; }

    public string? SlotCode { get; init; }
}
