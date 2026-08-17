using BuildingBlock.Application.Abstraction.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.CompilerServices;
using Qcontrol.Application.Features.BranchVideos.Command.AddBranchVideo;
using QControl.Application.Abstraction.Services;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchVideos;

public sealed class AddBranchVideoCommandHandlerTests
{
    [Fact]
    public async Task Missing_branch_is_rejected_before_media_save()
    {
        var media = new TestMediaService();
        var handler = CreateHandler(new(), new(), media, new TestQueue(), new TestUnitOfWork());

        var result = await handler.Handle(Command(Video()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x => x.Code == "BranchVideos.BranchNotFound");
        Assert.Equal(0, media.SaveVideoCallCount);
    }

    [Fact]
    public async Task Empty_video_is_rejected()
    {
        var media = new TestMediaService();
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            new(),
            media,
            new TestQueue(),
            new TestUnitOfWork());

        var result = await handler.Handle(Command(null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x => x.Code == "BranchVideos.VideoRequired");
        Assert.Equal(0, media.SaveVideoCallCount);
    }

    [Fact]
    public async Task Existing_display_order_is_rejected()
    {
        var media = new TestMediaService();
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            new() { EntityTestFactory.BranchVideo(10, 1, 2) },
            media,
            new TestQueue(),
            new TestUnitOfWork());

        var result = await handler.Handle(Command(Video()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x => x.Code == "BranchVideos.OrderConflict");
        Assert.Equal(0, media.SaveVideoCallCount);
    }

    [Fact]
    public async Task Successful_upload_saves_original_once_and_queues_processing()
    {
        var media = new TestMediaService();
        var queue = new TestQueue();
        var unitOfWork = new TestUnitOfWork();
        var videos = new List<BranchVideo>();
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            videos,
            media,
            queue,
            unitOfWork);

        var result = await handler.Handle(Command(Video()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(videos);
        Assert.Equal(1, media.SaveVideoCallCount);
        Assert.Contains("Branches/1/Videos/", media.LastFolder);
        Assert.EndsWith("/original", media.LastFolder);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, queue.EnqueueCallCount);
    }

    [Fact]
    public async Task Persistence_failure_removes_saved_original_and_does_not_queue()
    {
        var media = new TestMediaService();
        var queue = new TestQueue();
        var handler = CreateHandler(
            new() { EntityTestFactory.Branch(1) },
            new(),
            media,
            queue,
            new TestUnitOfWork { SaveChangesException = new InvalidOperationException("db") });

        var result = await handler.Handle(Command(Video()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x => x.Code == "BranchVideos.PersistenceFailed");
        Assert.Equal(new[] { media.SavedPath }, media.RemovedPaths);
        Assert.Equal(0, queue.EnqueueCallCount);
    }

    private static AddBranchVideoCommandHandler CreateHandler(
        List<Branch> branches,
        List<BranchVideo> videos,
        TestMediaService media,
        TestQueue queue,
        TestUnitOfWork unitOfWork) =>
        new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<BranchVideo>(videos),
            new InMemoryWriteRepository<BranchVideo>(videos),
            new TestCurrentUser(),
            media,
            queue,
            unitOfWork,
            NullLogger<AddBranchVideoCommandHandler>.Instance);

    private static AddBranchVideoCommand Command(IFormFile? video) =>
        new() { BranchId = 1, Video = video, DisplayOrder = 2 };

    private static IFormFile Video()
    {
        var stream = new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
        return new FormFile(stream, 0, stream.Length, "Video", "offer.mp4")
        {
            Headers = new HeaderDictionary(),
            ContentType = "video/mp4"
        };
    }

    private sealed class TestQueue : IBranchVideoProcessingQueue
    {
        public int EnqueueCallCount { get; private set; }
        public bool TryEnqueue(int branchVideoId)
        {
            EnqueueCallCount++;
            return true;
        }
        public async IAsyncEnumerable<int> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class TestMediaService : IMediaService
    {
        public string SavedPath { get; } =
            "Branches/1/Videos/key/original/source.mp4";
        public int SaveVideoCallCount { get; private set; }
        public string LastFolder { get; private set; } = string.Empty;
        public List<string> RemovedPaths { get; } = new();

        public Task<string> SaveVideoAsync(IFormFile videoFile, string folderName)
        {
            SaveVideoCallCount++;
            LastFolder = folderName;
            return Task.FromResult(SavedPath);
        }
        public void Remove(string filePath) => RemovedPaths.Add(filePath);
        public void RemoveRange(IEnumerable<string> filePaths) => RemovedPaths.AddRange(filePaths);
        public Task<string> SaveAsync(IFormFile mediaFile, string folderName) => throw new NotSupportedException();
        public Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName) => throw new NotSupportedException();
        public Task<Stream> GetStream(IFormFile formFile) => throw new NotSupportedException();
    }
}
