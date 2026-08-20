using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.UpdateBranchDisplayMessage;

public sealed record UpdateBranchDisplayMessageCommand
    : ICommand<BranchDisplayMessageResponse>, ICacheInvalidator
{
    public int BranchId { get; init; }
    public int MessageId { get; init; }
    public string TextAr { get; init; } = string.Empty;
    public string TextEn { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public string? RowVersion { get; init; }
    public IEnumerable<string> Tags => BranchDisplayCacheTags.ForBranch(BranchId);
}
