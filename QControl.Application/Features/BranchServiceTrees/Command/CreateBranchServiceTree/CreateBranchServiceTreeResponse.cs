using QControl.Domain.Enums;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;

public sealed class CreateBranchServiceTreeResponse
{
    public int BranchId { get; init; }

    public ServiceScope Scope { get; init; }

    public int OwnerBranchId { get; init; }

    public int CreatedServicesCount { get; init; }

    public int CreatedAssignmentsCount { get; init; }

    public CreatedBranchServiceTreeNodeResponse Root { get; init; } = null!;

    public ServiceGlobalizationRequestSummaryResponse GlobalizationRequest
    {
        get;
        init;
    } = null!;

    public string Message { get; init; } = string.Empty;
}

public sealed class CreatedBranchServiceTreeNodeResponse
{
    public int Id { get; init; }

    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ServiceCode { get; init; }

    public bool IsServiceCodeRequired { get; init; }

    public ServiceScope Scope { get; init; }

    public int OwnerBranchId { get; init; }

    public bool IsTicketIssuable { get; init; }

    public IReadOnlyList<CreatedBranchServiceTreeNodeResponse> Children { get; init; } =
        Array.Empty<CreatedBranchServiceTreeNodeResponse>();
}
