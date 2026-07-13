using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetServiceGlobalizationRequests;

public sealed class GetServiceGlobalizationRequestsQuery
    : IQuery<Pagination<ServiceGlobalizationRequestListItemResponse>>
{
    public ServiceGlobalizationRequestStatus? Status { get; set; }

    public ServiceGlobalizationRequestType? RequestType { get; set; }

    public int? BranchId { get; set; }

    public string? SearchText { get; set; }

    public OrderSort OrderSort { get; set; } = OrderSort.Newest;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
