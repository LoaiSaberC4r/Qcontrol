using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceSchedules.Shared;

namespace Qcontrol.Application.Features.ServiceSchedules.Query.GetServiceSchedule;

public sealed class GetServiceScheduleQuery
    : IQuery<ServiceScheduleResponse>
{
    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }
}
