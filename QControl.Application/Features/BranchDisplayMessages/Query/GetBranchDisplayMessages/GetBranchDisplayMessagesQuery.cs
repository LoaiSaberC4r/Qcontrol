using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Query.GetBranchDisplayMessages;

public sealed record GetBranchDisplayMessagesQuery
    : IQuery<IReadOnlyList<BranchDisplayMessageResponse>>
{
    public int BranchId { get; init; }
}
