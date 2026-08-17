namespace Qcontrol.Application.Features.BranchVideos.Shared;

public sealed class BranchDisplayPlaylistResponse
{
    public int BranchId { get; init; }
    public IReadOnlyList<BranchDisplayPlaylistVideoResponse> Videos { get; init; } =
        Array.Empty<BranchDisplayPlaylistVideoResponse>();
}

public sealed class BranchDisplayPlaylistVideoResponse
{
    public int VideoId { get; init; }
    public int DisplayOrder { get; init; }
    public string StreamUrl { get; init; } = string.Empty;
}
