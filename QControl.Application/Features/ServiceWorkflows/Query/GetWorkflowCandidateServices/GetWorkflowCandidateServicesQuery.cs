using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowCandidateServices;

public sealed class GetWorkflowCandidateServicesQuery
    : IQuery<Pagination<ServiceWorkflowCandidateServiceResponse>>
{
    public int BranchId { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchText { get; set; }

    public int? ParentServiceId { get; set; }

    public bool IncludeInactive { get; set; }

    public bool IncludeDeleted { get; set; }
}
