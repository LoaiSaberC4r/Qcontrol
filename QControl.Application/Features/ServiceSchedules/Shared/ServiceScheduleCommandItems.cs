namespace Qcontrol.Application.Features.ServiceSchedules.Shared;

public sealed record ServiceScheduleDayCommandItem
{
    public DayOfWeek DayOfWeek { get; init; }

    public IReadOnlyCollection<ServiceScheduleTimeSlotCommandItem> TimeSlots
        { get; init; } = Array.Empty<ServiceScheduleTimeSlotCommandItem>();
}

public sealed record ServiceScheduleTimeSlotCommandItem
{
    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }
}
