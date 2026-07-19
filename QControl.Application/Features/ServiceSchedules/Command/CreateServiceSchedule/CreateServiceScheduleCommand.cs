using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceSchedules.Command.CreateServiceSchedule;

public sealed record CreateServiceScheduleCommand
    : ICommand<ServiceScheduleResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public IReadOnlyCollection<ServiceScheduleDayCommandItem> Days
        { get; init; } = Array.Empty<ServiceScheduleDayCommandItem>();

    public bool IsSlotCodeRequired { get; init; }

    public string? SlotCode { get; init; }

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
