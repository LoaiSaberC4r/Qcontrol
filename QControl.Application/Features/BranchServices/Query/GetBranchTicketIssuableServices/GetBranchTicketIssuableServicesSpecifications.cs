using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

internal sealed record BranchTicketIssuanceState
{
    public int BranchId { get; init; }

    public bool IsActive { get; init; }

    public string? LogoPath { get; init; }

    public string? MainColor { get; init; }

    public string? SecondaryColor { get; init; }

    public string? BackgroundColor { get; init; }
}

internal sealed class GetBranchTicketIssuanceStateSpec
    : Specification<Branch, BranchTicketIssuanceState>
{
    public GetBranchTicketIssuanceStateSpec(int branchId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(branch => branch.Id == branchId);
        Select(branch => new BranchTicketIssuanceState
        {
            BranchId = branch.Id,
            IsActive = branch.IsActive,
            LogoPath = branch.Branding == null
                ? null
                : branch.Branding.LogoPath,
            MainColor = branch.Branding == null
                ? null
                : branch.Branding.MainColor,
            SecondaryColor = branch.Branding == null
                ? null
                : branch.Branding.SecondaryColor,
            BackgroundColor = branch.Branding == null
                ? null
                : branch.Branding.BackgroundColor
        });
    }
}

internal sealed class GetAssignedBranchServiceIdsSpec
    : Specification<BranchService, int>
{
    public GetAssignedBranchServiceIdsSpec(int branchId)
    {
        UseNoTracking();
        EnableDistinct();
        AddCriteria(assignment => assignment.BranchId == branchId);
        Select(assignment => assignment.ServiceId);
    }
}

internal sealed class GetCurrentlyAvailableScheduledServiceIdsSpec
    : Specification<ServiceSchedule, int>
{
    public GetCurrentlyAvailableScheduledServiceIdsSpec(
        int branchId,
        DayOfWeek currentDay,
        TimeOnly currentTime)
    {
        UseNoTracking();
        EnableDistinct();
        AddCriteria(schedule => schedule.BranchId == branchId);
        AddCriteria(schedule => schedule.TimeSlots.Any(slot =>
            slot.DayOfWeek == currentDay &&
            slot.StartTime <= currentTime &&
            currentTime < slot.EndTime));
        Select(schedule => schedule.ServiceId);
    }
}
