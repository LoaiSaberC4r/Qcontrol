using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceSchedules.Shared;

internal sealed class GetServiceScheduleForMutationSpec
    : Specification<ServiceSchedule>
{
    public GetServiceScheduleForMutationSpec(
        int branchId,
        int serviceId)
    {
        IgnoreGlobalFilters();
        UseTracking();
        AddCriteria(x =>
            x.BranchId == branchId &&
            x.ServiceId == serviceId);
        Include(x => x.TimeSlots);
    }
}

internal sealed class GetServiceScheduleProjectionSpec
    : Specification<ServiceSchedule, ServiceScheduleProjection>
{
    public GetServiceScheduleProjectionSpec(
        int branchId,
        int serviceId,
        int? scheduleId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(x =>
            x.BranchId == branchId &&
            x.ServiceId == serviceId);

        if (scheduleId.HasValue)
        {
            var id = scheduleId.Value;
            AddCriteria(x => x.Id == id);
        }

        Select(x => new ServiceScheduleProjection
        {
            ScheduleId = x.Id,
            BranchId = x.BranchId,
            ServiceId = x.ServiceId,
            TimeSlots = x.TimeSlots
                .OrderBy(slot => slot.DayOfWeek)
                .ThenBy(slot => slot.StartTime)
                .ThenBy(slot => slot.EndTime)
                .Select(slot => new ServiceScheduleTimeSlotProjection
                {
                    DayOfWeek = slot.DayOfWeek,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime
                })
                .ToArray(),
            IsSlotCodeRequired = x.IsSlotCodeRequired,
            SlotCode = x.SlotCode,
            RowVersion = x.RowVersion,
            CreatedByApplicationUserId = x.CreatedByApplicationUserId,
            LastModifiedByApplicationUserId =
                x.LastModifiedByApplicationUserId,
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        });
    }
}

internal sealed class GetBranchScheduleStateSpec
    : Specification<Branch, BranchScheduleState>
{
    public GetBranchScheduleStateSpec(int branchId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(x => x.Id == branchId);
        Select(x => new BranchScheduleState
        {
            BranchId = x.Id,
            IsActive = x.IsActive
        });
    }
}

internal sealed class ServiceScheduleExistsSpec
    : Specification<ServiceSchedule, int>
{
    public ServiceScheduleExistsSpec(
        int branchId,
        int serviceId,
        int? excludedScheduleId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(x =>
            x.BranchId == branchId &&
            x.ServiceId == serviceId);

        if (excludedScheduleId.HasValue)
        {
            var id = excludedScheduleId.Value;
            AddCriteria(x => x.Id != id);
        }

        Select(x => x.Id);
    }
}

internal sealed class ServiceScheduleSlotCodeExistsSpec
    : Specification<ServiceSchedule, int>
{
    public ServiceScheduleSlotCodeExistsSpec(
        int branchId,
        string slotCode,
        int? excludedScheduleId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(x =>
            x.BranchId == branchId &&
            x.SlotCode == slotCode);

        if (excludedScheduleId.HasValue)
        {
            var id = excludedScheduleId.Value;
            AddCriteria(x => x.Id != id);
        }

        Select(x => x.Id);
    }
}

internal sealed class BranchScheduleState
{
    public int BranchId { get; init; }

    public bool IsActive { get; init; }
}
