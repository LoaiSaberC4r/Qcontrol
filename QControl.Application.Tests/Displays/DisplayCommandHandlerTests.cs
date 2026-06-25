using Qcontrol.Application.Features.Displays.Command.CreateDisplay;
using Qcontrol.Application.Features.Displays.Command.DeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.RestoreDisplay;
using Qcontrol.Application.Features.Displays.Command.UpdateDisplay;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Displays;

public sealed class DisplayCommandHandlerTests
{
    [Fact]
    public async Task Create_succeeds_trims_values_saves_once_and_creates_no_window_links()
    {
        var branch = Branch(1);
        var displays = new List<Display>();
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<Display>(displays);
        var handler = CreateHandler(
            new List<Branch> { branch },
            displays,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new CreateDisplayCommand
            {
                BranchId = branch.Id,
                Number = " D-01 ",
                IPAddress = " 192.168.1.30 ",
                SerialNo = " DISPLAY-SN-001 ",
                Type = " LED Display "
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(displays);
        Assert.Equal("D-01", result.Value.Number);
        Assert.Equal("192.168.1.30", result.Value.IPAddress);
        Assert.Equal("DISPLAY-SN-001", result.Value.SerialNo);
        Assert.Equal("LED Display", result.Value.Type);
        Assert.Empty(displays[0].DisplayWindows);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, unitOfWork.BeginTransactionCallCount);
    }

    [Fact]
    public async Task Create_scopes_number_ip_and_serial_to_branch_and_includes_deleted()
    {
        var branch1 = Branch(1);
        var branch2 = Branch(2);
        var deleted = Display(
            10,
            branch1,
            number: "D-01",
            ipAddress: "192.168.1.30",
            serialNo: "DISPLAY-SN-001",
            isDeleted: true);
        var displays = new List<Display> { deleted };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Branch> { branch1, branch2 },
            displays,
            new InMemoryWriteRepository<Display>(displays),
            unitOfWork);

        var duplicateNumber = await handler.Handle(
            ValidCreate(branch1.Id) with
            {
                Number = "D-01",
                IPAddress = "192.168.1.31",
                SerialNo = "DISPLAY-SN-002"
            },
            CancellationToken.None);
        var duplicateIp = await handler.Handle(
            ValidCreate(branch1.Id) with
            {
                Number = "D-02",
                IPAddress = "192.168.1.30",
                SerialNo = "DISPLAY-SN-003"
            },
            CancellationToken.None);
        var duplicateSerial = await handler.Handle(
            ValidCreate(branch1.Id) with
            {
                Number = "D-03",
                IPAddress = "192.168.1.32",
                SerialNo = "DISPLAY-SN-001"
            },
            CancellationToken.None);
        var otherBranch = await handler.Handle(
            ValidCreate(branch2.Id) with
            {
                Number = "D-01",
                IPAddress = "192.168.1.30",
                SerialNo = "DISPLAY-SN-001"
            },
            CancellationToken.None);

        Assert.True(duplicateNumber.IsFailure);
        Assert.True(duplicateIp.IsFailure);
        Assert.True(duplicateSerial.IsFailure);
        Assert.True(otherBranch.IsSuccess);
        Assert.Contains(
            duplicateNumber.Errors,
            error =>
                error.Code ==
                "Displays.Create.NumberAlreadyExistsInBranch");
        Assert.Contains(
            duplicateIp.Errors,
            error =>
                error.Code ==
                "Displays.Create.IPAddressAlreadyExistsInBranch");
        Assert.Contains(
            duplicateSerial.Errors,
            error =>
                error.Code ==
                "Displays.Create.SerialNoAlreadyExistsInBranch");
    }

    [Fact]
    public async Task Create_returns_not_found_when_branch_is_missing()
    {
        var displays = new List<Display>();
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Branch>(),
            displays,
            new InMemoryWriteRepository<Display>(displays),
            unitOfWork);

        var result = await handler.Handle(
            ValidCreate(branchId: 99),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Displays.Create.BranchNotFound");
        Assert.Empty(displays);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_succeeds_without_changing_branch_and_includes_deleted_duplicates()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var deletedDuplicate = Display(
            11,
            branch,
            number: "D-02",
            ipAddress: "192.168.1.31",
            serialNo: "DISPLAY-SN-002",
            isDeleted: true);
        var displays = new List<Display> { display, deletedDuplicate };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<Display>(displays);
        var handler = UpdateHandler(displays, writeRepository, unitOfWork);

        var currentValues = await handler.Handle(
            new UpdateDisplayCommand
            {
                Id = display.Id,
                RequestId = display.Id,
                Number = " D-01 ",
                IPAddress = " 192.168.1.30 ",
                SerialNo = " DISPLAY-SN-001 ",
                Type = " LCD Display "
            },
            CancellationToken.None);
        var duplicateNumber = await handler.Handle(
            new UpdateDisplayCommand
            {
                Id = display.Id,
                RequestId = display.Id,
                Number = "D-02",
                IPAddress = "192.168.1.30",
                SerialNo = "DISPLAY-SN-001",
                Type = "LCD Display"
            },
            CancellationToken.None);

        Assert.True(currentValues.IsSuccess);
        Assert.Equal(branch.Id, display.BranchId);
        Assert.Equal("LCD Display", display.Type);
        Assert.True(duplicateNumber.IsFailure);
        Assert.Contains(
            duplicateNumber.Errors,
            error =>
                error.Code ==
                "Displays.Update.NumberAlreadyExistsInBranch");
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_returns_not_found_for_soft_deleted_display()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isDeleted: true);
        var displays = new List<Display> { display };
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            displays,
            new InMemoryWriteRepository<Display>(displays),
            unitOfWork);

        var result = await handler.Handle(
            new UpdateDisplayCommand
            {
                Id = display.Id,
                RequestId = display.Id,
                Number = "D-02",
                IPAddress = "192.168.1.31",
                SerialNo = "DISPLAY-SN-002",
                Type = "LCD Display"
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Displays.Update.DisplayNotFound");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Delete_soft_deletes_display_and_preserves_window_links()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var window = Window(1, branch);
        var displayWindow = EntityTestFactory.DisplayWindow(
            30,
            display.Id,
            window.Id,
            display,
            window);
        var displays = new List<Display> { display };
        var displayWindows = new List<DisplayWindow> { displayWindow };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<Display>(displays);
        var handler = new DeleteDisplayCommandHandler(
            new InMemoryWriteReadRepository<Display>(displays),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

        var result = await handler.Handle(
            new DeleteDisplayCommand { Id = display.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(display.IsDeleted);
        Assert.NotNull(display.DeletedOnUtc);
        Assert.Single(displayWindows);
        Assert.Equal("D-01", display.Number);
        Assert.Equal("192.168.1.30", display.IPAddress);
        Assert.Equal("DISPLAY-SN-001", display.SerialNo);
        Assert.Equal(1, writeRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Restore_preserves_values_requires_branch_and_checks_conflicts()
    {
        var branch = Branch(1);
        var deleted = Display(10, branch, isDeleted: true);
        var conflict = Display(
            11,
            branch,
            number: "D-02",
            ipAddress: "192.168.1.31",
            serialNo: "DISPLAY-SN-002");
        var displays = new List<Display> { deleted, conflict };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<Display>(displays);
        var handler = RestoreHandler(
            new List<Branch> { branch },
            displays,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new RestoreDisplayCommand { Id = deleted.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(deleted.IsDeleted);
        Assert.Equal(branch.Id, deleted.BranchId);
        Assert.Equal("D-01", deleted.Number);
        Assert.Equal("192.168.1.30", deleted.IPAddress);
        Assert.Equal("DISPLAY-SN-001", deleted.SerialNo);
        Assert.Equal("LED Display", deleted.Type);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        deleted.IsDeleted = true;
        deleted.Update(
            "D-02",
            "192.168.1.30",
            "DISPLAY-SN-001",
            "LED Display",
            EntityTestFactory.CurrentUserId);

        var conflictResult = await handler.Handle(
            new RestoreDisplayCommand { Id = deleted.Id },
            CancellationToken.None);

        Assert.True(conflictResult.IsFailure);
        Assert.Contains(
            conflictResult.Errors,
            error => error.Code == "Displays.Restore.NumberConflict");
    }

    [Fact]
    public async Task Permanent_delete_requires_soft_delete_blocks_display_window_and_calls_repository_once()
    {
        var branch = Branch(1);
        var active = Display(10, branch);
        var linkedDeleted = Display(11, branch, number: "D-02", isDeleted: true);
        var deleted = Display(12, branch, number: "D-03", isDeleted: true);
        var window = Window(1, branch);
        var displayWindow = EntityTestFactory.DisplayWindow(
            30,
            linkedDeleted.Id,
            window.Id,
            linkedDeleted,
            window);
        var displays = new List<Display> { active, linkedDeleted, deleted };
        var displayWindows = new List<DisplayWindow> { displayWindow };
        var permanentRepository =
            new TestDisplayPermanentDeleteRepository();
        var handler = new PermanentDeleteDisplayCommandHandler(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            permanentRepository,
            new TestCurrentUser());

        var activeResult = await handler.Handle(
            new PermanentDeleteDisplayCommand { Id = active.Id },
            CancellationToken.None);
        var linkedResult = await handler.Handle(
            new PermanentDeleteDisplayCommand { Id = linkedDeleted.Id },
            CancellationToken.None);
        var deletedResult = await handler.Handle(
            new PermanentDeleteDisplayCommand { Id = deleted.Id },
            CancellationToken.None);

        Assert.True(activeResult.IsFailure);
        Assert.True(linkedResult.IsFailure);
        Assert.True(deletedResult.IsSuccess);
        Assert.Contains(
            activeResult.Errors,
            error =>
                error.Code ==
                "Displays.PermanentDelete.MustBeSoftDeleted");
        Assert.Contains(
            linkedResult.Errors,
            error =>
                error.Code ==
                "Displays.PermanentDelete.HasDisplayWindowLinks");
        Assert.Single(displayWindows);
        Assert.Equal(1, permanentRepository.CallCount);
        Assert.Equal(deleted.Id, permanentRepository.LastDisplayId);
    }

    private static CreateDisplayCommand ValidCreate(int branchId)
        => new()
        {
            BranchId = branchId,
            Number = "D-01",
            IPAddress = "192.168.1.30",
            SerialNo = "DISPLAY-SN-001",
            Type = "LED Display"
        };

    private static Branch Branch(int id)
        => EntityTestFactory.Branch(id);

    private static WaitingArea WaitingArea(int id, Branch branch)
        => EntityTestFactory.WaitingArea(
            id,
            branch.Id,
            id,
            branch,
            descriptiveName: $"Area {id}");

    private static Window Window(int id, Branch branch)
    {
        var waitingArea = WaitingArea(id, branch);

        return EntityTestFactory.Window(
            id,
            waitingArea.Id,
            id.ToString(),
            waitingArea);
    }

    private static Display Display(
        int id,
        Branch branch,
        string number = "D-01",
        string ipAddress = "192.168.1.30",
        string serialNo = "DISPLAY-SN-001",
        string type = "LED Display",
        bool isDeleted = false)
        => EntityTestFactory.Display(
            id,
            branch.Id,
            number,
            ipAddress,
            serialNo,
            type,
            branch,
            isDeleted);

    private static CreateDisplayCommandHandler CreateHandler(
        List<Branch> branches,
        List<Display> displays,
        InMemoryWriteRepository<Display> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<Display>(displays),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static UpdateDisplayCommandHandler UpdateHandler(
        List<Display> displays,
        InMemoryWriteRepository<Display> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Display>(displays),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static RestoreDisplayCommandHandler RestoreHandler(
        List<Branch> branches,
        List<Display> displays,
        InMemoryWriteRepository<Display> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<Display>(displays),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private sealed class TestDisplayPermanentDeleteRepository
        : IDisplayPermanentDeleteRepository
    {
        public int CallCount { get; private set; }

        public int? LastDisplayId { get; private set; }

        public Task<int> DeletePermanentlyAsync(
            int displayId,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastDisplayId = displayId;

            return Task.FromResult(1);
        }
    }
}
