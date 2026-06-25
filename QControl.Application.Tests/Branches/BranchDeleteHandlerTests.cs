using Qcontrol.Application.Features.Branches.Command.DeleteBranch;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Branches;

public sealed class BranchDeleteHandlerTests
{
    [Fact]
    public async Task Delete_is_blocked_by_active_display()
    {
        var branch = EntityTestFactory.Branch(1);
        var display = EntityTestFactory.Display(
            10,
            branch.Id,
            branch: branch);
        var displays = new List<Display> { display };
        var unitOfWork = new TestUnitOfWork();
        var displayWriteRepository =
            new InMemoryWriteRepository<Display>(displays);
        var handler = CreateHandler(
            new List<Branch> { branch },
            new List<WaitingArea>(),
            displays,
            unitOfWork);

        var result = await handler.Handle(
            new DeleteBranchCommand { BranchId = branch.Id },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Branches.Delete.HasRelatedData");
        Assert.Contains(display, displays);
        Assert.Equal(0, displayWriteRepository.DeleteCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Delete_is_blocked_by_soft_deleted_display_using_ignore_filter_check()
    {
        var branch = EntityTestFactory.Branch(1);
        var display = EntityTestFactory.Display(
            10,
            branch.Id,
            branch: branch,
            isDeleted: true);
        var displays = new List<Display> { display };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Branch> { branch },
            new List<WaitingArea>(),
            displays,
            unitOfWork);

        var result = await handler.Handle(
            new DeleteBranchCommand { BranchId = branch.Id },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Branches.Delete.HasRelatedData");
        Assert.Contains(display, displays);
        Assert.True(display.IsDeleted);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static DeleteBranchCommandHandler CreateHandler(
        List<Branch> branches,
        List<WaitingArea> waitingAreas,
        List<Display> displays,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteRepository<Branch>(branches),
            new InMemoryWriteRepository<Location>(new List<Location>()),
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Display>(displays),
            new TestCurrentUser(),
            unitOfWork);
}
