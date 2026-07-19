namespace QControl.Domain.Entities;

public sealed class ServiceScheduleTimeSlot
{
    private ServiceScheduleTimeSlot()
    {
    }

    public int Id { get; private set; }

    public int ServiceScheduleId { get; private set; }

    public ServiceSchedule ServiceSchedule { get; private set; } = null!;

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    internal static ServiceScheduleTimeSlot Create(
        ServiceScheduleTimeSlotDefinition definition)
    {
        return new ServiceScheduleTimeSlot
        {
            DayOfWeek = definition.DayOfWeek,
            StartTime = definition.StartTime,
            EndTime = definition.EndTime
        };
    }
}
