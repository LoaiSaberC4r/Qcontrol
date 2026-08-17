using BuildingBlock.Application.Abstraction.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Qcontrol.Application.Features.BranchVideos.Command.PermanentDeleteBranchVideo;
using QControl.Application.Abstraction.Services;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchVideos;

public sealed class PermanentDeleteBranchVideoCommandHandlerTests
{
    private static readonly string RowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Active_video_is_rejected_without_database_or_media_changes()
    {
        var unitOfWork = new TestUnitOfWork();
        var media = new TestMediaService(unitOfWork);
        var transcoder = new TestTranscoder(unitOfWork);
        var videos = new List<BranchVideo> { EntityTestFactory.BranchVideo(10, 1, 1) };
        var handler = Handler(videos, unitOfWork, media, transcoder);

        var result = await handler.Handle(Command(1, 10), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x => x.Code == "BranchVideos.PermanentDelete.MustBeInactive");
        Assert.Single(videos);
        Assert.Empty(media.RemovedPaths);
        Assert.Equal(0, transcoder.RemoveCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Inactive_video_is_deleted_then_original_and_hls_are_cleaned()
    {
        var unitOfWork = new TestUnitOfWork();
        var media = new TestMediaService(unitOfWork);
        var transcoder = new TestTranscoder(unitOfWork);
        var video = EntityTestFactory.BranchVideo(10, 1, 1, isActive: false);
        var videos = new List<BranchVideo> { video };
        var handler = Handler(videos, unitOfWork, media, transcoder);

        var result = await handler.Handle(Command(1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(videos);
        Assert.Equal(new[] { video.OriginalPath }, media.RemovedPaths);
        Assert.Equal(1, transcoder.RemoveCallCount);
        Assert.True(media.DatabaseWasCommittedBeforeRemove);
        Assert.True(transcoder.DatabaseWasCommittedBeforeRemove);
    }

    [Fact]
    public async Task Video_from_another_branch_is_rejected()
    {
        var unitOfWork = new TestUnitOfWork();
        var handler = Handler(
            new() { EntityTestFactory.BranchVideo(10, 2, 1, isActive: false) },
            unitOfWork,
            new TestMediaService(unitOfWork),
            new TestTranscoder(unitOfWork),
            includeSecondBranch: true);

        var result = await handler.Handle(Command(1, 10), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x => x.Code == "BranchVideos.DoesNotBelongToBranch");
    }

    [Fact]
    public async Task Stale_row_version_is_reported_as_concurrency_conflict()
    {
        var unitOfWork = new TestUnitOfWork
        {
            SaveChangesException = new DbUpdateConcurrencyException()
        };
        var handler = Handler(
            new() { EntityTestFactory.BranchVideo(10, 1, 1, isActive: false) },
            unitOfWork,
            new TestMediaService(unitOfWork),
            new TestTranscoder(unitOfWork));

        var result = await handler.Handle(Command(1, 10), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x => x.Code == "BranchVideos.ConcurrencyConflict");
    }

    private static PermanentDeleteBranchVideoCommandHandler Handler(
        List<BranchVideo> videos,
        TestUnitOfWork unitOfWork,
        TestMediaService media,
        TestTranscoder transcoder,
        bool includeSecondBranch = false)
    {
        var branches = new List<Branch> { EntityTestFactory.Branch(1) };
        if (includeSecondBranch)
        {
            branches.Add(EntityTestFactory.Branch(2));
        }

        return new PermanentDeleteBranchVideoCommandHandler(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<BranchVideo>(videos),
            new InMemoryWriteRepository<BranchVideo>(videos),
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestCoordinator(),
            transcoder,
            media,
            unitOfWork,
            NullLogger<PermanentDeleteBranchVideoCommandHandler>.Instance);
    }

    private static PermanentDeleteBranchVideoCommand Command(int branchId, int videoId) =>
        new() { BranchId = branchId, VideoId = videoId, RowVersion = RowVersion };

    private sealed class TestCoordinator : IBranchVideoProcessingCoordinator
    {
        public ValueTask<IAsyncDisposable> AcquireAsync(int branchVideoId, CancellationToken cancellationToken) =>
            ValueTask.FromResult<IAsyncDisposable>(new Lease());
        private sealed class Lease : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    private sealed class TestTranscoder : IBranchVideoTranscoder
    {
        private readonly TestUnitOfWork _unitOfWork;
        public int RemoveCallCount { get; private set; }
        public bool DatabaseWasCommittedBeforeRemove { get; private set; }
        public TestTranscoder(TestUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public Task<string> TranscodeToHlsAsync(string originalPath, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public void RemoveHlsOutput(string originalPath, string? hlsManifestPath)
        {
            RemoveCallCount++;
            DatabaseWasCommittedBeforeRemove = _unitOfWork.SaveChangesCallCount == 1;
        }
    }

    private sealed class TestMediaService : IMediaService
    {
        private readonly TestUnitOfWork _unitOfWork;
        public List<string> RemovedPaths { get; } = new();
        public bool DatabaseWasCommittedBeforeRemove { get; private set; }
        public TestMediaService(TestUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public void Remove(string filePath)
        {
            DatabaseWasCommittedBeforeRemove = _unitOfWork.SaveChangesCallCount == 1;
            RemovedPaths.Add(filePath);
        }
        public void RemoveRange(IEnumerable<string> filePaths) => RemovedPaths.AddRange(filePaths);
        public Task<string> SaveVideoAsync(IFormFile videoFile, string folderName) => throw new NotSupportedException();
        public Task<string> SaveAsync(IFormFile mediaFile, string folderName) => throw new NotSupportedException();
        public Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName) => throw new NotSupportedException();
        public Task<Stream> GetStream(IFormFile formFile) => throw new NotSupportedException();
    }
}
