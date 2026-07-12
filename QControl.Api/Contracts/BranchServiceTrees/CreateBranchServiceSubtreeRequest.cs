namespace Qcontrol.Api.Contracts.BranchServiceTrees;

public sealed class CreateBranchServiceSubtreeRequest
{
    public CreateBranchServiceTreeNodeRequest? Root { get; init; }
}
