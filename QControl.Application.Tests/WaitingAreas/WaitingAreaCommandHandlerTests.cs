using Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.DeactivateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.PermanentDeleteWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.ReactivateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.WaitingAreas;

public sealed class WaitingAreaCommandHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

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
        Assert.True(result.Value.IsActive);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_returns_conflict_when_number_exists_in_branch_including_inactive()
    {
        var branch = EntityTestFactory.Branch(1);
        var existing = EntityTestFactory.WaitingArea(10, 1, 7, branch);
        existing.Deactivate(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var waitingAreas = new List<WaitingArea> { existing };
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
            error => error.Code == "WaitingAreas.Create.NumberAlreadyExistsInBranch");
        Assert.Single(waitingAreas);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_updates_fields_without_changing_branch()
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
                Number = 8,
                AudioDevice = "speaker 2",
                ControlDevice = "control 2",
                DescriptiveName = "secondary",
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(branch1.Id, waitingArea.BranchId);
        Assert.Equal(8, waitingArea.Number);
        Assert.Equal("speaker 2", waitingArea.AudioDevice);
        Assert.Equal("control 2", waitingArea.ControlDevice);
        Assert.Equal("secondary", waitingArea.DescriptiveName);
        Assert.Equal(EntityTestFactory.CurrentUserId, waitingArea.LastModifiedByApplicationUserId);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_returns_conflict_for_duplicate_number_in_same_branch()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingAreas = new List<WaitingArea>
        {
            EntityTestFactory.WaitingArea(10, 1, 5, branch),
            EntityTestFactory.WaitingArea(11, 1, 8, branch)
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
                Number = 8,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "WaitingAreas.Update.NumberAlreadyExistsInBranch");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Deactivate_and_reactivate_transition_waiting_area_state()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingArea = EntityTestFactory.WaitingArea(10, 1, 5, branch);
        var waitingAreas = new List<WaitingArea> { waitingArea };
        var branches = new List<Branch> { branch };
        var writeRepository =
            new InMemoryWriteRepository<WaitingArea>(waitingAreas);
        var unitOfWork = new TestUnitOfWork();
        var deactivateHandler = new DeactivateWaitingAreaCommandHandler(
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Branch>(branches),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);
        var reactivateHandler = new ReactivateWaitingAreaCommandHandler(
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Branch>(branches),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);

        var deactivateResult = await deactivateHandler.Handle(
            new DeactivateWaitingAreaCommand
            {
                Id = waitingArea.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var reactivateResult = await reactivateHandler.Handle(
            new ReactivateWaitingAreaCommand
            {
                Id = waitingArea.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(deactivateResult.IsSuccess);
        Assert.True(reactivateResult.IsSuccess);
        Assert.True(waitingArea.IsActive);
        Assert.NotNull(waitingArea.DeactivatedOnUtc);
        Assert.NotNull(waitingArea.ReactivatedOnUtc);
        Assert.Equal(2, writeRepository.UpdateCallCount);
        Assert.Equal(2, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Permanent_delete_requires_inactive_and_no_windows()
    {
        var branch = EntityTestFactory.Branch(1);
        var active = EntityTestFactory.WaitingArea(10, 1, 5, branch);
        var linkedInactive = EntityTestFactory.WaitingArea(11, 1, 6, branch);
        linkedInactive.Deactivate(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var inactive = EntityTestFactory.WaitingArea(12, 1, 7, branch);
        inactive.Deactivate(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var window = EntityTestFactory.Window(100, linkedInactive.Id, waitingArea: linkedInactive);
        var waitingAreas = new List<WaitingArea> { active, linkedInactive, inactive };
        var windows = new List<Window> { window };
        var writeRepository =
            new InMemoryWriteRepository<WaitingArea>(waitingAreas);
        var unitOfWork = new TestUnitOfWork();
        var handler = new PermanentDeleteWaitingAreaCommandHandler(
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Window>(windows),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);

        var activeResult = await handler.Handle(
            new PermanentDeleteWaitingAreaCommand
            {
                Id = active.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var linkedResult = await handler.Handle(
            new PermanentDeleteWaitingAreaCommand
            {
                Id = linkedInactive.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var inactiveResult = await handler.Handle(
            new PermanentDeleteWaitingAreaCommand
            {
                Id = inactive.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(activeResult.IsFailure);
        Assert.True(linkedResult.IsFailure);
        Assert.True(inactiveResult.IsSuccess);
        Assert.Contains(
            activeResult.Errors,
            error => error.Code == "WaitingAreas.PermanentDelete.MustBeInactive");
        Assert.Contains(
            linkedResult.Errors,
            error => error.Code == "WaitingAreas.PermanentDelete.HasWindows");
        Assert.DoesNotContain(waitingAreas, area => area.Id == inactive.Id);
        Assert.Equal(1, writeRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
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
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Branch>(branches),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);
}
