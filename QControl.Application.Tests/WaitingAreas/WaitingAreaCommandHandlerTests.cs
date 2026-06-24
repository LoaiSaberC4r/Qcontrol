using Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.DeleteWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.WaitingAreas;

public sealed class WaitingAreaCommandHandlerTests
{
    [Fact]
    public async Task Create_creates_waiting_area_successfully_and_commits_once()
    {
        var branch = EntityTestFactory.Branch(1);
        var branches = new List<Branch> { branch };
        var waitingAreas = new List<WaitingArea>();
        var unitOfWork = new TestUnitOfWork();
        var writeRepository = new InMemoryWriteRepository<WaitingArea>(waitingAreas);
        var handler = CreateHandler(
            branches,
            waitingAreas,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new CreateWaitingAreaCommand
            {
                BranchId = 1,
                Number = 5,
                AudioDevice = "  speaker  ",
                ControlDevice = " tablet ",
                DescriptiveName = " Main "
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(waitingAreas);
        Assert.Equal(1, result.Value.BranchId);
        Assert.Equal(5, result.Value.Number);
        Assert.Equal("speaker", result.Value.AudioDevice);
        Assert.Equal("tablet", result.Value.ControlDevice);
        Assert.Equal("Main", result.Value.DescriptiveName);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_returns_branch_not_found_when_branch_does_not_exist()
    {
        var waitingAreas = new List<WaitingArea>();
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Branch>(),
            waitingAreas,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new CreateWaitingAreaCommand
            {
                BranchId = 99,
                Number = 1
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "WaitingAreas.Create.BranchNotFound");
        Assert.Empty(waitingAreas);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_returns_conflict_when_number_exists_in_branch()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingAreas = new List<WaitingArea>
        {
            EntityTestFactory.WaitingArea(10, 1, 7, branch)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Branch> { branch },
            waitingAreas,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new CreateWaitingAreaCommand
            {
                BranchId = 1,
                Number = 7
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error =>
                error.Code ==
                "WaitingAreas.Create.NumberAlreadyExistsInBranch");
        Assert.Single(waitingAreas);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_allows_same_number_in_another_branch()
    {
        var branch1 = EntityTestFactory.Branch(1);
        var branch2 = EntityTestFactory.Branch(2);
        var waitingAreas = new List<WaitingArea>
        {
            EntityTestFactory.WaitingArea(10, 2, 7, branch2)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Branch> { branch1, branch2 },
            waitingAreas,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new CreateWaitingAreaCommand
            {
                BranchId = 1,
                Number = 7
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, waitingAreas.Count);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_updates_successfully_and_commits_once()
    {
        var branch1 = EntityTestFactory.Branch(1);
        var branch2 = EntityTestFactory.Branch(2);
        var waitingArea =
            EntityTestFactory.WaitingArea(10, 1, 5, branch1);
        var waitingAreas = new List<WaitingArea> { waitingArea };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<WaitingArea>(waitingAreas);
        var handler = UpdateHandler(
            new List<Branch> { branch1, branch2 },
            waitingAreas,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new UpdateWaitingAreaCommand
            {
                Id = 10,
                RequestId = 10,
                BranchId = 2,
                Number = 8,
                AudioDevice = "speaker 2",
                ControlDevice = "control 2",
                DescriptiveName = "secondary"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, waitingArea.BranchId);
        Assert.Equal(8, waitingArea.Number);
        Assert.Equal("speaker 2", waitingArea.AudioDevice);
        Assert.Equal("control 2", waitingArea.ControlDevice);
        Assert.Equal("secondary", waitingArea.DescriptiveName);
        Assert.Equal(EntityTestFactory.CurrentUserId, waitingArea.LastModifiedByApplicationUserId);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_returns_waiting_area_not_found()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingAreas = new List<WaitingArea>();
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            new List<Branch> { branch },
            waitingAreas,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new UpdateWaitingAreaCommand
            {
                Id = 10,
                RequestId = 10,
                BranchId = 1,
                Number = 5
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error =>
                error.Code ==
                "WaitingAreas.Update.WaitingAreaNotFound");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_returns_branch_not_found()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingArea =
            EntityTestFactory.WaitingArea(10, 1, 5, branch);
        var waitingAreas = new List<WaitingArea> { waitingArea };
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            new List<Branch>(),
            waitingAreas,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new UpdateWaitingAreaCommand
            {
                Id = 10,
                RequestId = 10,
                BranchId = 99,
                Number = 5
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "WaitingAreas.Update.BranchNotFound");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_returns_conflict_for_duplicate_branch_number()
    {
        var branch1 = EntityTestFactory.Branch(1);
        var branch2 = EntityTestFactory.Branch(2);
        var waitingAreas = new List<WaitingArea>
        {
            EntityTestFactory.WaitingArea(10, 1, 5, branch1),
            EntityTestFactory.WaitingArea(11, 2, 8, branch2)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            new List<Branch> { branch1, branch2 },
            waitingAreas,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new UpdateWaitingAreaCommand
            {
                Id = 10,
                RequestId = 10,
                BranchId = 2,
                Number = 8
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error =>
                error.Code ==
                "WaitingAreas.Update.NumberAlreadyExistsInBranch");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_allows_keeping_existing_number_for_same_waiting_area()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingAreas = new List<WaitingArea>
        {
            EntityTestFactory.WaitingArea(10, 1, 5, branch)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            new List<Branch> { branch },
            waitingAreas,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new UpdateWaitingAreaCommand
            {
                Id = 10,
                RequestId = 10,
                BranchId = 1,
                Number = 5
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Delete_deletes_successfully_when_no_windows_exist()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingArea =
            EntityTestFactory.WaitingArea(10, 1, 5, branch);
        var waitingAreas = new List<WaitingArea> { waitingArea };
        var windows = new List<Window>();
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<WaitingArea>(waitingAreas);
        var handler = DeleteHandler(
            waitingAreas,
            windows,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new DeleteWaitingAreaCommand { Id = 10 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(waitingAreas);
        Assert.Equal(1, writeRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Delete_returns_waiting_area_not_found()
    {
        var waitingAreas = new List<WaitingArea>();
        var windows = new List<Window>();
        var unitOfWork = new TestUnitOfWork();
        var handler = DeleteHandler(
            waitingAreas,
            windows,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new DeleteWaitingAreaCommand { Id = 10 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error =>
                error.Code ==
                "WaitingAreas.Delete.WaitingAreaNotFound");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Delete_returns_conflict_when_related_windows_exist()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingArea =
            EntityTestFactory.WaitingArea(10, 1, 5, branch);
        var waitingAreas = new List<WaitingArea> { waitingArea };
        var windows = new List<Window>
        {
            EntityTestFactory.Window(100, 10, waitingArea: waitingArea)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = DeleteHandler(
            waitingAreas,
            windows,
            new InMemoryWriteRepository<WaitingArea>(waitingAreas),
            unitOfWork);

        var result = await handler.Handle(
            new DeleteWaitingAreaCommand { Id = 10 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "WaitingAreas.Delete.HasWindows");
        Assert.Single(waitingAreas);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static CreateWaitingAreaCommandHandler CreateHandler(
        List<Branch> branches,
        List<WaitingArea> waitingAreas,
        InMemoryWriteRepository<WaitingArea> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static UpdateWaitingAreaCommandHandler UpdateHandler(
        List<Branch> branches,
        List<WaitingArea> waitingAreas,
        InMemoryWriteRepository<WaitingArea> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static DeleteWaitingAreaCommandHandler DeleteHandler(
        List<WaitingArea> waitingAreas,
        List<Window> windows,
        InMemoryWriteRepository<WaitingArea> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            writeRepository,
            new InMemoryWriteReadRepository<Window>(windows),
            new TestCurrentUser(),
            unitOfWork);
}
