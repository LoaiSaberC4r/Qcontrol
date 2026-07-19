using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchServices.Command.UnassignBranchLeafService;

public sealed record UnassignBranchLeafServiceCommand
    : ICommand<UnassignBranchLeafServiceResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public IEnumerable<string> Tags =>
        new[]
        {
            OperationalCacheTags.Services,
            OperationalCacheTags.ServiceCentral,
            OperationalCacheTags.BranchServices,
            OperationalCacheTags.BranchServicesForBranch(BranchId),
            OperationalCacheTags.ServiceSchedules,
            OperationalCacheTags.ServiceSchedulesForBranch(BranchId),
            OperationalCacheTags.ServiceSchedule(BranchId, LeafServiceId)
        };
}
