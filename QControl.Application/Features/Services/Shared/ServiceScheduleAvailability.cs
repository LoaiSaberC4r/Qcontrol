namespace Qcontrol.Application.Features.Services.Shared;

internal static class ServiceScheduleAvailability
{
    public static TimeOnly CalculateAllowedUntil(TimeOnly currentTime, TimeSpan allowedTime)
    {
        var calculatedEnd = currentTime.ToTimeSpan() + allowedTime;

        return calculatedEnd >= TimeSpan.FromDays(1)
            ? TimeOnly.MaxValue
            : TimeOnly.FromTimeSpan(calculatedEnd);
    }
}
