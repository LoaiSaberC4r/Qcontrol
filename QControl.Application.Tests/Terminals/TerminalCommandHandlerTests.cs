using Qcontrol.Application.Features.Terminals.Command.CreateTerminal;
using Qcontrol.Application.Features.Terminals.Command.DeactivateTerminal;
using Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;
using Qcontrol.Application.Features.Terminals.Command.ReactivateTerminal;
using Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Terminals;

public sealed class TerminalCommandHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Create_succeeds_trims_values_and_saves_once()
    {
        var window = Window(id: 1);
        var terminals = new List<Terminal>();
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<Terminal>(terminals);
        var handler = CreateHandler(
            new List<Window> { window },
            terminals,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new CreateTerminalCommand
            {
                WindowId = window.Id,
                Number = " T-01 ",
                IPAddress = " 192.168.1.20 ",
                SerialNo = " ABC-100 ",
                Type = " Operator Module "
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(terminals);
        Assert.Equal(window.BranchId, result.Value.BranchId);
        Assert.Equal("T-01", result.Value.Number);
        Assert.Equal("192.168.1.20", result.Value.IPAddress);
        Assert.Equal("ABC-100", result.Value.SerialNo);
        Assert.Equal("Operator Module", result.Value.Type);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, unitOfWork.BeginTransactionCallCount);
    }

    [Fact]
    public async Task Create_allows_inactive_window_and_uses_effective_inactive()
    {
        var window = Window(id: 1, isInactive: true);
        var terminals = new List<Terminal>();
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Window> { window },
            terminals,
            new InMemoryWriteRepository<Terminal>(terminals),
            unitOfWork);

        var result = await handler.Handle(
            ValidCreate(window.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.EffectiveIsActive);
        Assert.Single(terminals);
    }

    [Fact]
    public async Task Create_rejects_duplicate_number_in_same_window_including_inactive()
    {
        var window = Window(id: 1);
        var inactiveTerminal = Terminal(
            id: 10,
            window,
            number: "T-01",
            ipAddress: "192.168.1.30",
            serialNo: "ABC-200",
            isInactive: true);
        var terminals = new List<Terminal> { inactiveTerminal };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Window> { window },
            terminals,
            new InMemoryWriteRepository<Terminal>(terminals),
            unitOfWork);

        var result = await handler.Handle(
            ValidCreate(window.Id) with { Number = "T-01" },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Terminals.Create.NumberAlreadyExistsInWindow");
        Assert.Single(terminals);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_checks_ip_and_serial_uniqueness_inside_branch()
    {
        var branch1 = EntityTestFactory.Branch(1);
        var branch2 = EntityTestFactory.Branch(2);
        var area1 = WaitingArea(1, branch1);
        var area2 = WaitingArea(2, branch1);
        var area3 = WaitingArea(3, branch2);
        var window1 = Window(1, area1);
        var window2 = Window(2, area2);
        var window3 = Window(3, area3);
        var existing = Terminal(
            10,
            window1,
            ipAddress: "192.168.1.20",
            serialNo: "ABC-100");
        var terminals = new List<Terminal> { existing };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<Window> { window1, window2, window3 },
            terminals,
            new InMemoryWriteRepository<Terminal>(terminals),
            unitOfWork);

        var duplicateIp = await handler.Handle(
            ValidCreate(window2.Id) with
            {
                Number = "T-02",
                IPAddress = "192.168.1.20",
                SerialNo = "ABC-200"
            },
            CancellationToken.None);
        var duplicateSerial = await handler.Handle(
            ValidCreate(window2.Id) with
            {
                Number = "T-03",
                IPAddress = "192.168.1.21",
                SerialNo = "ABC-100"
            },
            CancellationToken.None);
        var otherBranch = await handler.Handle(
            ValidCreate(window3.Id) with
            {
                Number = "T-01",
                IPAddress = "192.168.1.20",
                SerialNo = "ABC-100"
            },
            CancellationToken.None);

        Assert.True(duplicateIp.IsFailure);
        Assert.True(duplicateSerial.IsFailure);
        Assert.True(otherBranch.IsSuccess);
        Assert.Contains(
            duplicateIp.Errors,
            error => error.Code == "Terminals.Create.IPAddressAlreadyExistsInBranch");
        Assert.Contains(
            duplicateSerial.Errors,
            error => error.Code == "Terminals.Create.SerialNoAlreadyExistsInBranch");
    }

    [Fact]
    public async Task Update_succeeds_without_changing_window_and_saves_once()
    {
        var window = Window(id: 1);
        var terminal = Terminal(10, window);
        var terminals = new List<Terminal> { terminal };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository =
            new InMemoryWriteRepository<Terminal>(terminals);
        var handler = UpdateHandler(
            new List<Window> { window },
            terminals,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new UpdateTerminalCommand
            {
                Id = terminal.Id,
                Number = " SELF-01 ",
                IPAddress = " 10.0.0.25 ",
                SerialNo = " XYZ-100 ",
                Type = " Self-Service Module ",
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(window.Id, terminal.WindowId);
        Assert.Equal(window.BranchId, terminal.BranchId);
        Assert.Equal("SELF-01", terminal.Number);
        Assert.Equal("10.0.0.25", terminal.IPAddress);
        Assert.Equal("XYZ-100", terminal.SerialNo);
        Assert.Equal("Self-Service Module", terminal.Type);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, unitOfWork.BeginTransactionCallCount);
    }

    [Fact]
    public async Task Deactivate_and_reactivate_transition_terminal_state()
    {
        var window = Window(id: 1);
        var terminal = Terminal(10, window);
        var terminals = new List<Terminal> { terminal };
        var windows = new List<Window> { window };
        var writeRepository =
            new InMemoryWriteRepository<Terminal>(terminals);
        var unitOfWork = new TestUnitOfWork();
        var deactivateHandler = new DeactivateTerminalCommandHandler(
            new InMemoryWriteReadRepository<Terminal>(terminals),
            new InMemoryWriteReadRepository<Window>(windows),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);
        var reactivateHandler = new ReactivateTerminalCommandHandler(
            new InMemoryWriteReadRepository<Terminal>(terminals),
            new InMemoryWriteReadRepository<Window>(windows),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);

        var deactivateResult = await deactivateHandler.Handle(
            new DeactivateTerminalCommand
            {
                Id = terminal.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var reactivateResult = await reactivateHandler.Handle(
            new ReactivateTerminalCommand
            {
                Id = terminal.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(deactivateResult.IsSuccess);
        Assert.True(reactivateResult.IsSuccess);
        Assert.True(terminal.IsActive);
        Assert.NotNull(terminal.DeactivatedOnUtc);
        Assert.NotNull(terminal.ReactivatedOnUtc);
        Assert.Equal(2, writeRepository.UpdateCallCount);
        Assert.Equal(2, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Permanent_delete_requires_inactive_terminal_and_deletes_once()
    {
        var window = Window(id: 1);
        var active = Terminal(10, window);
        var inactive = Terminal(11, window, number: "T-02", isInactive: true);
        var terminals = new List<Terminal> { active, inactive };
        var writeRepository =
            new InMemoryWriteRepository<Terminal>(terminals);
        var unitOfWork = new TestUnitOfWork();
        var handler = new PermanentDeleteTerminalCommandHandler(
            new InMemoryWriteReadRepository<Terminal>(terminals),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);

        var activeResult = await handler.Handle(
            new PermanentDeleteTerminalCommand
            {
                Id = active.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var inactiveResult = await handler.Handle(
            new PermanentDeleteTerminalCommand
            {
                Id = inactive.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(activeResult.IsFailure);
        Assert.True(inactiveResult.IsSuccess);
        Assert.Contains(
            activeResult.Errors,
            error => error.Code == "Terminals.PermanentDelete.MustBeInactive");
        Assert.DoesNotContain(terminals, terminal => terminal.Id == inactive.Id);
        Assert.Equal(1, writeRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private static CreateTerminalCommand ValidCreate(int windowId)
        => new()
        {
            WindowId = windowId,
            Number = "T-01",
            IPAddress = "192.168.1.20",
            SerialNo = "ABC-100",
            Type = "Operator Module"
        };

    private static Branch Branch(int id)
        => EntityTestFactory.Branch(id);

    private static WaitingArea WaitingArea(int id, Branch? branch = null)
    {
        branch ??= Branch(id);

        return EntityTestFactory.WaitingArea(
            id,
            branch.Id,
            id,
            branch,
            descriptiveName: $"Area {id}");
    }

    private static Window Window(
        int id,
        WaitingArea? waitingArea = null,
        bool isInactive = false)
    {
        waitingArea ??= WaitingArea(id);

        return EntityTestFactory.Window(
            id,
            waitingArea.Id,
            number: id.ToString(),
            waitingArea,
            isInactive: isInactive);
    }

    private static Terminal Terminal(
        int id,
        Window window,
        string number = "T-01",
        string ipAddress = "192.168.1.20",
        string serialNo = "ABC-100",
        string type = "Operator Module",
        bool isInactive = false)
        => EntityTestFactory.Terminal(
            id,
            window.Id,
            number,
            ipAddress,
            serialNo,
            type,
            window,
            isInactive);

    private static CreateTerminalCommandHandler CreateHandler(
        List<Window> windows,
        List<Terminal> terminals,
        InMemoryWriteRepository<Terminal> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<Terminal>(terminals),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static UpdateTerminalCommandHandler UpdateHandler(
        List<Window> windows,
        List<Terminal> terminals,
        InMemoryWriteRepository<Terminal> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<Terminal>(terminals),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);
}
