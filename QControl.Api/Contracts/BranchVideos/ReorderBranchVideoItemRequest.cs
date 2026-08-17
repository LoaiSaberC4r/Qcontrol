namespace Qcontrol.Api.Contracts.BranchVideos;

public sealed class ReorderBranchVideoItemRequest
{
    public int VideoId { get; init; }
    public int DisplayOrder { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}
