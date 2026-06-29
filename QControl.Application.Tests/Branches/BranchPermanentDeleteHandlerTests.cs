using Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Branches;

public sealed class BranchPermanentDeleteHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Permanent_delete_requires_inactive_branch()
    {
        var branch = EntityTestFactory.Branch(1);
        var branches = new List<Branch> { branch };
        var unitOfWork = new TestUnitOfWork();
        var branchWriteRepository =
            new InMemoryWriteRepository<Branch>(branches);
        var handler = CreateHandler(
            branches,
            new List<WaitingArea>(),
            new List<Display>(),
            branchWriteRepository,
            unitOfWork);

        var result = await handler.Handle(
            new PermanentDeleteBranchCommand
            {
                BranchId = branch.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Branches.PermanentDelete.MustBeInactive");
        Assert.Contains(branch, branches);
        Assert.Equal(0, branchWriteRepository.DeleteCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Permanent_delete_is_blocked_by_active_or_inactive_display()
    {
        var branch = EntityTestFactory.Branch(1);
        branch.Deactivate(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var display = EntityTestFactory.Display(
            10,
            branch.Id,
            branch: branch,
            isInactive: true);
        var displays = new List<Display> { display };
        var unitOfWork = new TestUnitOfWork();
        var branchWriteRepository =
            new InMemoryWriteRepository<Branch>(new List<Branch> { branch });
        var handler = CreateHandler(
            new List<Branch> { branch },
            new List<WaitingArea>(),
            displays,
            branchWriteRepository,
            unitOfWork);

        var result = await handler.Handle(
            new PermanentDeleteBranchCommand
            {
                BranchId = branch.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Branches.PermanentDelete.HasRelatedData");
        Assert.Contains(display, displays);
        Assert.False(display.IsActive);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Permanent_delete_removes_inactive_branch_and_location_when_no_related_data_exists()
    {
        var branch = EntityTestFactory.Branch(1);
        branch.Deactivate(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var branches = new List<Branch> { branch };
        var locations = new List<Location> { branch.Location! };
        var unitOfWork = new TestUnitOfWork();
        var branchWriteRepository =
            new InMemoryWriteRepository<Branch>(branches);
        var locationWriteRepository =
            new InMemoryWriteRepository<Location>(locations);
        var handler = new PermanentDeleteBranchCommandHandler(
            new InMemoryWriteReadRepository<Branch>(branches),
            branchWriteRepository,
            locationWriteRepository,
            new InMemoryWriteReadRepository<WaitingArea>(new List<WaitingArea>()),
            new InMemoryWriteReadRepository<Display>(new List<Display>()),
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);

        var result = await handler.Handle(
            new PermanentDeleteBranchCommand
            {
                BranchId = branch.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(branches);
        Assert.Empty(locations);
        Assert.Equal(1, branchWriteRepository.DeleteCallCount);
        Assert.Equal(1, locationWriteRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private static PermanentDeleteBranchCommandHandler CreateHandler(
        List<Branch> branches,
        List<WaitingArea> waitingAreas,
        List<Display> displays,
        InMemoryWriteRepository<Branch> branchWriteRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            branchWriteRepository,
            new InMemoryWriteRepository<Location>(
                branches
                    .Where(branch => branch.Location is not null)
                    .Select(branch => branch.Location!)
                    .ToList()),
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Display>(displays),
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);
}
