using Qcontrol.Application.Features.BranchVideos.Command.ReorderBranchVideos;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchVideos;

public sealed class ReorderBranchVideosTests
{
    private static readonly string RowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Validator_rejects_duplicate_video_ids()
    {
        var validator = new ReorderBranchVideosCommandValidator();
        var command = Command(
            new ReorderBranchVideoItem { VideoId = 11, DisplayOrder = 1, RowVersion = RowVersion },
            new ReorderBranchVideoItem { VideoId = 11, DisplayOrder = 2, RowVersion = RowVersion });

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validator_rejects_duplicate_display_orders()
    {
        var validator = new ReorderBranchVideosCommandValidator();
        var command = Command(
            new ReorderBranchVideoItem { VideoId = 11, DisplayOrder = 1, RowVersion = RowVersion },
            new ReorderBranchVideoItem { VideoId = 12, DisplayOrder = 1, RowVersion = RowVersion });

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Successful_swap_is_one_repository_business_operation()
    {
        var first = EntityTestFactory.BranchVideo(11, 1, 1);
        var second = EntityTestFactory.BranchVideo(12, 1, 2);
        first.ChangeDisplayOrder(2, EntityTestFactory.CurrentUserId);
        second.ChangeDisplayOrder(1, EntityTestFactory.CurrentUserId);
        var repository = new TestReorderRepository(
            new BranchVideoReorderResult(
                BranchVideoReorderStatus.Success,
                new[] { second, first }));
        var handler = Handler(repository);

        var result = await handler.Handle(Command(
            new ReorderBranchVideoItem { VideoId = 11, DisplayOrder = 2, RowVersion = RowVersion },
            new ReorderBranchVideoItem { VideoId = 12, DisplayOrder = 1, RowVersion = RowVersion }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 12, 11 }, result.Value.Select(x => x.Id));
        Assert.Equal(1, repository.CallCount);
        Assert.Equal(2, repository.LastItems.Count);
    }

    [Theory]
    [InlineData(BranchVideoReorderStatus.WrongBranch, "BranchVideos.DoesNotBelongToBranch")]
    [InlineData(BranchVideoReorderStatus.ConcurrencyConflict, "BranchVideos.ConcurrencyConflict")]
    [InlineData(BranchVideoReorderStatus.OrderConflict, "BranchVideos.OrderConflict")]
    public async Task Persistence_failures_are_mapped_cleanly(
        BranchVideoReorderStatus status,
        string expectedCode)
    {
        var handler = Handler(new TestReorderRepository(
            new BranchVideoReorderResult(status, Array.Empty<BranchVideo>())));

        var result = await handler.Handle(Command(
            new ReorderBranchVideoItem { VideoId = 11, DisplayOrder = 2, RowVersion = RowVersion }),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x => x.Code == expectedCode);
    }

    private static ReorderBranchVideosCommandHandler Handler(
        IBranchVideoReorderRepository repository) =>
        new(
            new InMemoryWriteReadRepository<Branch>(
                new() { EntityTestFactory.Branch(1) }),
            repository,
            new TestCurrentUser());

    private static ReorderBranchVideosCommand Command(
        params ReorderBranchVideoItem[] items) =>
        new() { BranchId = 1, Items = items.ToList() };

    private sealed class TestReorderRepository : IBranchVideoReorderRepository
    {
        private readonly BranchVideoReorderResult _result;
        public int CallCount { get; private set; }
        public IReadOnlyCollection<BranchVideoReorderItem> LastItems { get; private set; } =
            Array.Empty<BranchVideoReorderItem>();

        public TestReorderRepository(BranchVideoReorderResult result)
        {
            _result = result;
        }

        public Task<BranchVideoReorderResult> ReorderAsync(
            int branchId,
            Guid lastModifiedByApplicationUserId,
            IReadOnlyCollection<BranchVideoReorderItem> items,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastItems = items;
            return Task.FromResult(_result);
        }
    }
}
