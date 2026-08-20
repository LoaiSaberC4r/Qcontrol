namespace Qcontrol.Api.Contracts.BranchDisplays;

public sealed class CreateBranchDisplayMessageRequest
{
    public string TextAr { get; init; } = string.Empty;
    public string TextEn { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

public sealed class UpdateBranchDisplayMessageRequest
{
    public string TextAr { get; init; } = string.Empty;
    public string TextEn { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public string? RowVersion { get; init; }
}

public sealed class ReorderBranchDisplayMessagesRequest
{
    public List<ReorderBranchDisplayMessageItemRequest> Items { get; init; } = new();
}

public sealed class ReorderBranchDisplayMessageItemRequest
{
    public int MessageId { get; init; }
    public int DisplayOrder { get; init; }
    public string? RowVersion { get; init; }
}
