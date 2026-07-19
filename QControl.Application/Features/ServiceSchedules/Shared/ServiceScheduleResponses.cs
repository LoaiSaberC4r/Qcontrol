using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceSchedules.Shared;

public sealed class ServiceScheduleResponse
{
    public int ScheduleId { get; init; }

    public int BranchId { get; init; }

    public int ServiceId { get; init; }

    public IReadOnlyCollection<ServiceScheduleDayResponse> Days { get; init; } =
        Array.Empty<ServiceScheduleDayResponse>();

    public bool IsSlotCodeRequired { get; init; }

    public string? SlotCode { get; init; }

    public bool IsCurrentlyOperational { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public Guid CreatedByApplicationUserId { get; init; }

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }

    public string? Message { get; init; }
}

public sealed class ServiceScheduleDayResponse
{
    public DayOfWeek DayOfWeek { get; init; }

    public IReadOnlyCollection<ServiceScheduleTimeSlotResponse> TimeSlots
        { get; init; } = Array.Empty<ServiceScheduleTimeSlotResponse>();
}

public sealed class ServiceScheduleTimeSlotResponse
{
    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}

internal sealed class ServiceScheduleProjection
{
    public int ScheduleId { get; init; }

    public int BranchId { get; init; }

    public int ServiceId { get; init; }

    public IReadOnlyCollection<ServiceScheduleTimeSlotProjection> TimeSlots
        { get; init; } = Array.Empty<ServiceScheduleTimeSlotProjection>();

    public bool IsSlotCodeRequired { get; init; }

    public string? SlotCode { get; init; }

    public byte[] RowVersion { get; init; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; init; }

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}

internal sealed class ServiceScheduleTimeSlotProjection
{
    public DayOfWeek DayOfWeek { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}

internal static class ServiceScheduleResponseFactory
{
    public static ServiceScheduleResponse ToResponse(
        ServiceScheduleProjection schedule,
        bool isCurrentlyOperational,
        string? message = null)
    {
        return new ServiceScheduleResponse
        {
            ScheduleId = schedule.ScheduleId,
            BranchId = schedule.BranchId,
            ServiceId = schedule.ServiceId,
            Days = schedule.TimeSlots
                .GroupBy(slot => slot.DayOfWeek)
                .OrderBy(group => group.Key)
                .Select(group => new ServiceScheduleDayResponse
                {
                    DayOfWeek = group.Key,
                    TimeSlots = group
                        .OrderBy(slot => slot.StartTime)
                        .ThenBy(slot => slot.EndTime)
                        .Select(slot => new ServiceScheduleTimeSlotResponse
                        {
                            StartTime = slot.StartTime,
                            EndTime = slot.EndTime
                        })
                        .ToArray()
                })
                .ToArray(),
            IsSlotCodeRequired = schedule.IsSlotCodeRequired,
            SlotCode = schedule.SlotCode,
            IsCurrentlyOperational = isCurrentlyOperational,
            RowVersion = RowVersionConverter.ToBase64(schedule.RowVersion),
            CreatedByApplicationUserId =
                schedule.CreatedByApplicationUserId,
            LastModifiedByApplicationUserId =
                schedule.LastModifiedByApplicationUserId,
            CreatedOnUtc = schedule.CreatedOnUtc,
            ModifiedOnUtc = schedule.ModifiedOnUtc,
            Message = message
        };
    }
}
