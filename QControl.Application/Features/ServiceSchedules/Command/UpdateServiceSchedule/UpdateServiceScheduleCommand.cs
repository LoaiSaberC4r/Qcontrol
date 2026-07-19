using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceSchedules.Command.UpdateServiceSchedule;

public sealed record UpdateServiceScheduleCommand
    : ICommand<ServiceScheduleResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    public IReadOnlyCollection<DayOfWeek> WorkDays { get; init; } =
        Array.Empty<DayOfWeek>();

    public bool IsSlotCodeRequired { get; init; }

    public string? SlotCode { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.ServiceSchedules,
        OperationalCacheTags.ServiceSchedulesForBranch(BranchId),
        OperationalCacheTags.ServiceSchedule(BranchId, LeafServiceId),
        OperationalCacheTags.BranchServices,
        OperationalCacheTags.BranchServicesForBranch(BranchId),
        OperationalCacheTags.Services
    };
}
