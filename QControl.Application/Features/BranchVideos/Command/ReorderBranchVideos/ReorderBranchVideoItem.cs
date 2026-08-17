namespace Qcontrol.Application.Features.BranchVideos.Command.ReorderBranchVideos;

public sealed class ReorderBranchVideoItem
{
    public int VideoId { get; init; }
    public int DisplayOrder { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}
