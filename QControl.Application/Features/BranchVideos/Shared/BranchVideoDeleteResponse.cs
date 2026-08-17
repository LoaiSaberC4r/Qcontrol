namespace Qcontrol.Application.Features.BranchVideos.Shared;

public sealed class BranchVideoDeleteResponse
{
    public int VideoId { get; init; }
    public int BranchId { get; init; }
    public string Message { get; init; } = string.Empty;
}
