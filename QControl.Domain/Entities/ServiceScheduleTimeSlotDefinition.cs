namespace QControl.Domain.Entities;

public sealed record ServiceScheduleTimeSlotDefinition(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);
