namespace Qcontrol.Api.Contracts.ServiceSchedules;

public sealed class ServiceScheduleDayRequest
{
    public DayOfWeek DayOfWeek { get; init; }

    public IReadOnlyCollection<ServiceScheduleTimeSlotRequest> TimeSlots
        { get; init; } = Array.Empty<ServiceScheduleTimeSlotRequest>();
}
