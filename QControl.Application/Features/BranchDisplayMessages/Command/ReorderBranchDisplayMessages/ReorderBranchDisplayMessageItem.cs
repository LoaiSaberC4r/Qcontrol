namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.ReorderBranchDisplayMessages;

public sealed record ReorderBranchDisplayMessageItem
{
    public int MessageId { get; init; }
    public int DisplayOrder { get; init; }
    public string? RowVersion { get; init; }
}
