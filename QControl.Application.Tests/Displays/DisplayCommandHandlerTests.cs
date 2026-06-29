using Qcontrol.Application.Features.Displays.Command.CreateDisplay;
using Qcontrol.Application.Features.Displays.Command.DeactivateDisplay;
using Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.ReactivateDisplay;
using Qcontrol.Application.Features.Displays.Command.UpdateDisplay;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Displays;

public sealed class DisplayCommandHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

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
        Assert.True(result.Value.IsActive);
        Assert.Empty(displays[0].DisplayWindows);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, unitOfWork.BeginTransactionCallCount);
    }

    [Fact]
    public async Task Create_scopes_number_ip_and_serial_to_branch_and_includes_inactive()
    {
        var branch1 = Branch(1);
        var branch2 = Branch(2);
        var inactive = Display(
            10,
            branch1,
            number: "D-01",
            ipAddress: "192.168.1.30",
            serialNo: "DISPLAY-SN-001",
            isInactive: true);
        var displays = new List<Display> { inactive };
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
            error => error.Code == "Displays.Create.NumberAlreadyExistsInBranch");
        Assert.Contains(
            duplicateIp.Errors,
            error => error.Code == "Displays.Create.IPAddressAlreadyExistsInBranch");
        Assert.Contains(
            duplicateSerial.Errors,
            error => error.Code == "Displays.Create.SerialNoAlreadyExistsInBranch");
    }

    [Fact]
    public async Task Update_succeeds_without_changing_branch_and_includes_inactive_duplicates()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var inactiveDuplicate = Display(
            11,
            branch,
            number: "D-02",
            ipAddress: "192.168.1.31",
            serialNo: "DISPLAY-SN-002",
            isInactive: true);
        var displays = new List<Display> { display, inactiveDuplicate };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<Display>(displays);
        var handler = UpdateHandler(
            new List<Branch> { branch },
            displays,
            writeRepository,
            unitOfWork);

        var currentValues = await handler.Handle(
            new UpdateDisplayCommand
            {
                Id = display.Id,
                Number = " D-01 ",
                IPAddress = " 192.168.1.30 ",
                SerialNo = " DISPLAY-SN-001 ",
                Type = " LCD Display ",
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var duplicateNumber = await handler.Handle(
            new UpdateDisplayCommand
            {
                Id = display.Id,
                Number = "D-02",
                IPAddress = "192.168.1.30",
                SerialNo = "DISPLAY-SN-001",
                Type = "LCD Display",
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(currentValues.IsSuccess);
        Assert.Equal(branch.Id, display.BranchId);
        Assert.Equal("LCD Display", display.Type);
        Assert.True(duplicateNumber.IsFailure);
        Assert.Contains(
            duplicateNumber.Errors,
            error => error.Code == "Displays.Update.NumberAlreadyExistsInBranch");
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Deactivate_and_reactivate_transition_display_state()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var displays = new List<Display> { display };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<Display>(displays);
        var deactivateHandler = new DeactivateDisplayCommandHandler(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<Branch>(new List<Branch> { branch }),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);
        var reactivateHandler = new ReactivateDisplayCommandHandler(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<Branch>(new List<Branch> { branch }),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);

        var deactivateResult = await deactivateHandler.Handle(
            new DeactivateDisplayCommand
            {
                Id = display.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var reactivateResult = await reactivateHandler.Handle(
            new ReactivateDisplayCommand
            {
                Id = display.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(deactivateResult.IsSuccess);
        Assert.True(reactivateResult.IsSuccess);
        Assert.True(display.IsActive);
        Assert.NotNull(display.DeactivatedOnUtc);
        Assert.NotNull(display.ReactivatedOnUtc);
        Assert.Equal(2, writeRepository.UpdateCallCount);
        Assert.Equal(2, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Permanent_delete_requires_inactive_display_and_blocks_display_window_links()
    {
        var branch = Branch(1);
        var active = Display(10, branch);
        var linkedInactive = Display(11, branch, number: "D-02", isInactive: true);
        var inactive = Display(12, branch, number: "D-03", isInactive: true);
        var window = Window(1, branch);
        var displayWindow = EntityTestFactory.DisplayWindow(
            30,
            linkedInactive.Id,
            window.Id,
            linkedInactive,
            window);
        var displays = new List<Display> { active, linkedInactive, inactive };
        var displayWindows = new List<DisplayWindow> { displayWindow };
        var writeRepository =
            new InMemoryWriteRepository<Display>(displays);
        var unitOfWork = new TestUnitOfWork();
        var handler = new PermanentDeleteDisplayCommandHandler(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);

        var activeResult = await handler.Handle(
            new PermanentDeleteDisplayCommand
            {
                Id = active.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var linkedResult = await handler.Handle(
            new PermanentDeleteDisplayCommand
            {
                Id = linkedInactive.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var inactiveResult = await handler.Handle(
            new PermanentDeleteDisplayCommand
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
            error => error.Code == "Displays.PermanentDelete.MustBeInactive");
        Assert.Contains(
            linkedResult.Errors,
            error => error.Code == "Displays.PermanentDelete.HasDisplayWindowLinks");
        Assert.Single(displayWindows);
        Assert.DoesNotContain(displays, display => display.Id == inactive.Id);
        Assert.Equal(1, writeRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
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
        bool isInactive = false)
        => EntityTestFactory.Display(
            id,
            branch.Id,
            number,
            ipAddress,
            serialNo,
            type,
            branch,
            isInactive);

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
        List<Branch> branches,
        List<Display> displays,
        InMemoryWriteRepository<Display> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<Display>(displays),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);
}
