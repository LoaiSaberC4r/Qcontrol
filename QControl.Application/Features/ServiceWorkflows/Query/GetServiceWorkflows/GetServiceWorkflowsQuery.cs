using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflows;

public sealed class GetServiceWorkflowsQuery
    : IQuery<Pagination<ServiceWorkflowListItemResponse>>
{
    public int BranchId { get; set; }

    public int LeafServiceId { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchText { get; set; }

    public bool? IsActive { get; set; }

    public OrderSort OrderSort { get; set; } = OrderSort.Newest;
}
