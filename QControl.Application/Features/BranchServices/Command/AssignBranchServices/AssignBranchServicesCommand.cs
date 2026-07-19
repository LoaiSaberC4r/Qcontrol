using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchServices.Command.AssignBranchServices;

public sealed record AssignBranchServicesCommand
    : ICommand<AssignBranchServicesResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public IReadOnlyCollection<int> ServiceIds { get; init; } =
        Array.Empty<int>();

    public IEnumerable<string> Tags =>
        new[]
        {
            OperationalCacheTags.Services,
            OperationalCacheTags.ServiceCentral,
            OperationalCacheTags.BranchServices,
            OperationalCacheTags.BranchServicesForBranch(BranchId),
            OperationalCacheTags.ServiceSchedules,
            OperationalCacheTags.ServiceSchedulesForBranch(BranchId)
        }
        .Concat(ServiceIds.Select(serviceId =>
            OperationalCacheTags.ServiceSchedule(BranchId, serviceId)));
}
