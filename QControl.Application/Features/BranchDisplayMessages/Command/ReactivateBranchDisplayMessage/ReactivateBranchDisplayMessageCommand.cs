using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.ReactivateBranchDisplayMessage;

public sealed record ReactivateBranchDisplayMessageCommand
    : ICommand<BranchDisplayMessageStateResponse>, ICacheInvalidator
{
    public int BranchId { get; init; }
    public int MessageId { get; init; }
    public string? RowVersion { get; init; }
    public IEnumerable<string> Tags => BranchDisplayCacheTags.ForBranch(BranchId);
}
