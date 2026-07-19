namespace QControl.Domain.Entities;

public sealed class ServiceScheduleWorkDay
{
    private ServiceScheduleWorkDay()
    {
    }

    public int Id { get; private set; }

    public int ServiceScheduleId { get; private set; }

    public ServiceSchedule ServiceSchedule { get; private set; } = null!;

    public DayOfWeek DayOfWeek { get; private set; }

    internal static ServiceScheduleWorkDay Create(DayOfWeek dayOfWeek)
    {
        if (!Enum.IsDefined(dayOfWeek))
        {
            throw new ArgumentOutOfRangeException(
                nameof(dayOfWeek),
                "Day of week must be valid.");
        }

        return new ServiceScheduleWorkDay
        {
            DayOfWeek = dayOfWeek
        };
    }
}
