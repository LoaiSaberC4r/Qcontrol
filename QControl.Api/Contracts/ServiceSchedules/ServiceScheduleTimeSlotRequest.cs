namespace Qcontrol.Api.Contracts.ServiceSchedules;

public sealed class ServiceScheduleTimeSlotRequest
{
    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }
}
