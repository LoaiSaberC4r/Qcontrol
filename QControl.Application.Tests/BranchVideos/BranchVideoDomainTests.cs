using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.BranchVideos;

public sealed class BranchVideoDomainTests
{
    [Fact]
    public void Create_normalizes_media_and_starts_active_processing()
    {
        var video = BranchVideo.Create(
            7,
            " offer.mp4 ",
            "Branches\\7\\Videos\\key\\original\\source.mp4",
            3,
            EntityTestFactory.CurrentUserId);

        Assert.Equal(7, video.BranchId);
        Assert.Equal("offer.mp4", video.OriginalFileName);
        Assert.Equal("Branches/7/Videos/key/original/source.mp4", video.OriginalPath);
        Assert.Equal(3, video.DisplayOrder);
        Assert.True(video.IsActive);
        Assert.Equal(BranchVideoProcessingStatus.Processing, video.ProcessingStatus);
        Assert.Null(video.HlsManifestPath);
    }

    [Fact]
    public void Deactivate_and_reactivate_enforce_idempotency_rules()
    {
        var video = EntityTestFactory.BranchVideo(1, 7, 1);
        var now = new DateTime(2026, 8, 16, 10, 0, 0, DateTimeKind.Utc);

        Assert.True(video.Deactivate(now, EntityTestFactory.CurrentUserId));
        Assert.False(video.IsActive);
        Assert.False(video.Deactivate(now, EntityTestFactory.CurrentUserId));
        Assert.True(video.Reactivate(now.AddMinutes(1), EntityTestFactory.CurrentUserId));
        Assert.True(video.IsActive);
        Assert.False(video.Reactivate(now.AddMinutes(2), EntityTestFactory.CurrentUserId));
    }

    [Fact]
    public void Change_order_and_processing_transitions_update_expected_state()
    {
        var modifier = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var video = EntityTestFactory.BranchVideo(1, 7, 1);

        video.ChangeDisplayOrder(9, modifier);
        video.MarkReady("Branches\\7\\Videos\\key\\hls\\master.m3u8");

        Assert.Equal(9, video.DisplayOrder);
        Assert.Equal(modifier, video.LastModifiedByApplicationUserId);
        Assert.Equal(BranchVideoProcessingStatus.Ready, video.ProcessingStatus);
        Assert.Equal("Branches/7/Videos/key/hls/master.m3u8", video.HlsManifestPath);

        video.MarkFailed();
        Assert.Equal(BranchVideoProcessingStatus.Failed, video.ProcessingStatus);
        Assert.Null(video.HlsManifestPath);
    }
}
