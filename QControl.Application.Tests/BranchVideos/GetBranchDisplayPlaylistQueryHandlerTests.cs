using System.Text.Json;
using Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayPlaylist;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.BranchVideos;

public sealed class GetBranchDisplayPlaylistQueryHandlerTests
{
    [Fact]
    public async Task Returns_only_active_ready_videos_in_display_order_without_original_paths()
    {
        var videos = new List<BranchVideo>
        {
            EntityTestFactory.BranchVideo(21, 1, 3,
                processingStatus: BranchVideoProcessingStatus.Ready),
            EntityTestFactory.BranchVideo(22, 1, 2,
                isActive: false,
                processingStatus: BranchVideoProcessingStatus.Ready),
            EntityTestFactory.BranchVideo(23, 1, 4,
                processingStatus: BranchVideoProcessingStatus.Processing),
            EntityTestFactory.BranchVideo(24, 1, 5,
                processingStatus: BranchVideoProcessingStatus.Failed),
            EntityTestFactory.BranchVideo(25, 1, 1,
                processingStatus: BranchVideoProcessingStatus.Ready),
            EntityTestFactory.BranchVideo(30, 2, 1,
                processingStatus: BranchVideoProcessingStatus.Ready)
        };
        var cache = new TestCacheService();
        var handler = new GetBranchDisplayPlaylistQueryHandler(
            new InMemoryWriteReadRepository<Branch>(
                new() { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<BranchVideo>(videos),
            new TestCurrentUser(),
            cache);

        var result = await handler.Handle(
            new GetBranchDisplayPlaylistQuery { BranchId = 1 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 25, 21 }, result.Value.Videos.Select(x => x.VideoId));
        Assert.Equal(new[] { 1, 3 }, result.Value.Videos.Select(x => x.DisplayOrder));
        Assert.All(result.Value.Videos, x =>
        {
            Assert.StartsWith("/Media/Branches/1/Videos/", x.StreamUrl);
            Assert.EndsWith("/hls/master.m3u8", x.StreamUrl);
            Assert.DoesNotContain("original", x.StreamUrl, StringComparison.OrdinalIgnoreCase);
        });
        Assert.DoesNotContain(
            "OriginalPath",
            JsonSerializer.Serialize(result.Value),
            StringComparison.Ordinal);
        Assert.Equal(1, cache.SetCallCount);
    }
}
