using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Query.GetServices;

public sealed class GetServicesQuery
    : IQuery<Pagination<ServiceResponse>>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchText { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? IsTicketIssuable { get; set; }

    public int? ParentServiceId { get; set; }

    public ServiceScope? Scope { get; set; }

    public int? OwnerBranchId { get; set; }

    public bool? IsAssignedToCurrentBranch { get; set; }
}
