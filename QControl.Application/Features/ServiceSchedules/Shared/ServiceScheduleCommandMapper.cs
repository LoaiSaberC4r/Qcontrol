using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceSchedules.Shared;

internal static class ServiceScheduleCommandMapper
{
    public static IReadOnlyCollection<ServiceScheduleTimeSlotDefinition>
        ToDefinitions(
            IReadOnlyCollection<ServiceScheduleDayCommandItem>? days)
    {
        if (days is null)
        {
            throw new ArgumentException(
                "Schedule days are required.",
                nameof(days));
        }

        var definitions = new List<ServiceScheduleTimeSlotDefinition>();

        foreach (var day in days)
        {
            if (day is null || day.TimeSlots is null)
            {
                throw new ArgumentException(
                    "Every schedule day must contain time slots.",
                    nameof(days));
            }

            foreach (var timeSlot in day.TimeSlots)
            {
                if (timeSlot?.StartTime is not TimeOnly startTime ||
                    timeSlot.EndTime is not TimeOnly endTime)
                {
                    throw new ArgumentException(
                        "Every time slot must contain a start and end time.",
                        nameof(days));
                }

                definitions.Add(new ServiceScheduleTimeSlotDefinition(
                    day.DayOfWeek,
                    startTime,
                    endTime));
            }
        }

        return definitions;
    }
}
