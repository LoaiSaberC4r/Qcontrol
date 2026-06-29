using Qcontrol.Application.Features.Windows.Command.CreateWindow;
using Qcontrol.Application.Features.Windows.Command.DeactivateWindow;
using Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;
using Qcontrol.Application.Features.Windows.Command.ReactivateWindow;
using Qcontrol.Application.Features.Windows.Command.UpdateWindow;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Windows;

public sealed class WindowCommandHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Create_succeeds_trims_values_allows_both_flags_and_commits_once()
    {
        var waitingArea = WaitingArea();
        var windows = new List<Window>();
        var unitOfWork = new TestUnitOfWork();
        var writeRepository = new InMemoryWriteRepository<Window>(windows);
        var handler = CreateHandler(
            new List<WaitingArea> { waitingArea },
            new List<Branch> { waitingArea.Branch },
            windows,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = waitingArea.Id,
                Number = " 5 ",
                DescriptiveName = " Main Window ",
                IPAddress = " 192.168.1.10 ",
                EnableTicketBooking = true,
                EnableDirectCall = true
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(windows);
        Assert.Equal(waitingArea.BranchId, result.Value.BranchId);
        Assert.Equal("5", result.Value.Number);
        Assert.Equal("Main Window", result.Value.DescriptiveName);
        Assert.Equal("192.168.1.10", result.Value.IPAddress);
        Assert.True(result.Value.EnableTicketBooking);
        Assert.True(result.Value.EnableDirectCall);
        Assert.True(result.Value.IsActive);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_rejects_duplicate_number_and_ip_in_same_waiting_area_including_inactive()
    {
        var waitingArea = WaitingArea();
        var windows = new List<Window>
        {
            EntityTestFactory.Window(
                10,
                waitingArea.Id,
                "5",
                waitingArea,
                ipAddress: "192.168.1.10",
                isInactive: true)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<WaitingArea> { waitingArea },
            new List<Branch> { waitingArea.Branch },
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var numberResult = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = waitingArea.Id,
                Number = "5"
            },
            CancellationToken.None);
        var ipResult = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = waitingArea.Id,
                Number = "6",
                IPAddress = "192.168.1.10"
            },
            CancellationToken.None);

        Assert.True(numberResult.IsFailure);
        Assert.True(ipResult.IsFailure);
        Assert.Contains(
            numberResult.Errors,
            error => error.Code == "Windows.Create.NumberAlreadyExistsInWaitingArea");
        Assert.Contains(
            ipResult.Errors,
            error => error.Code == "Windows.Create.IPAddressAlreadyExistsInWaitingArea");
        Assert.Single(windows);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_succeeds_without_changing_waiting_area_and_commits_once()
    {
        var waitingArea = WaitingArea();
        var window =
            EntityTestFactory.Window(10, waitingArea.Id, "1", waitingArea);
        var windows = new List<Window> { window };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository = new InMemoryWriteRepository<Window>(windows);
        var handler = UpdateHandler(
            windows,
            new List<WaitingArea> { waitingArea },
            new List<Branch> { waitingArea.Branch },
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new UpdateWindowCommand
            {
                Id = window.Id,
                Number = " 8 ",
                DescriptiveName = " Updated ",
                IPAddress = " 192.168.1.20 ",
                EnableTicketBooking = true,
                EnableDirectCall = true,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(waitingArea.Id, window.WaitingAreaId);
        Assert.Equal(waitingArea.BranchId, window.BranchId);
        Assert.Equal("8", window.Number);
        Assert.Equal("Updated", window.DescriptiveName);
        Assert.Equal("192.168.1.20", window.IPAddress);
        Assert.True(window.EnableTicketBooking);
        Assert.True(window.EnableDirectCall);
        Assert.Equal(EntityTestFactory.CurrentUserId, window.LastModifiedByApplicationUserId);
        Assert.Equal(1, writeRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Deactivate_and_reactivate_transition_window_state()
    {
        var waitingArea = WaitingArea();
        var window = EntityTestFactory.Window(10, waitingArea.Id, "1", waitingArea);
        var windows = new List<Window> { window };
        var waitingAreas = new List<WaitingArea> { waitingArea };
        var branches = new List<Branch> { waitingArea.Branch };
        var writeRepository = new InMemoryWriteRepository<Window>(windows);
        var unitOfWork = new TestUnitOfWork();
        var deactivateHandler = new DeactivateWindowCommandHandler(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Branch>(branches),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);
        var reactivateHandler = new ReactivateWindowCommandHandler(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Branch>(branches),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestDateTimeProvider(),
            unitOfWork);

        var deactivateResult = await deactivateHandler.Handle(
            new DeactivateWindowCommand
            {
                Id = window.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var reactivateResult = await reactivateHandler.Handle(
            new ReactivateWindowCommand
            {
                Id = window.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(deactivateResult.IsSuccess);
        Assert.True(reactivateResult.IsSuccess);
        Assert.True(window.IsActive);
        Assert.NotNull(window.DeactivatedOnUtc);
        Assert.NotNull(window.ReactivatedOnUtc);
        Assert.Equal(2, writeRepository.UpdateCallCount);
        Assert.Equal(2, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Permanent_delete_requires_inactive_and_blocks_related_terminal_or_display_window()
    {
        var waitingArea = WaitingArea();
        var activeWindow = EntityTestFactory.Window(10, waitingArea.Id, "1", waitingArea);
        var terminalWindow = EntityTestFactory.Window(
            11,
            waitingArea.Id,
            "2",
            waitingArea,
            isInactive: true);
        var displayWindowOwner = EntityTestFactory.Window(
            12,
            waitingArea.Id,
            "3",
            waitingArea,
            isInactive: true);
        var inactiveWindow = EntityTestFactory.Window(
            13,
            waitingArea.Id,
            "4",
            waitingArea,
            isInactive: true);
        var terminal = EntityTestFactory.Terminal(20, terminalWindow.Id, window: terminalWindow);
        var displayWindow =
            EntityTestFactory.DisplayWindow(30, 1, displayWindowOwner.Id, window: displayWindowOwner);
        var windows = new List<Window>
        {
            activeWindow,
            terminalWindow,
            displayWindowOwner,
            inactiveWindow
        };
        var terminals = new List<Terminal> { terminal };
        var displayWindows = new List<DisplayWindow> { displayWindow };
        var writeRepository = new InMemoryWriteRepository<Window>(windows);
        var unitOfWork = new TestUnitOfWork();
        var handler = new PermanentDeleteWindowCommandHandler(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<Terminal>(terminals),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);

        var activeResult = await handler.Handle(
            new PermanentDeleteWindowCommand
            {
                Id = activeWindow.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var terminalResult = await handler.Handle(
            new PermanentDeleteWindowCommand
            {
                Id = terminalWindow.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var displayResult = await handler.Handle(
            new PermanentDeleteWindowCommand
            {
                Id = displayWindowOwner.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var inactiveResult = await handler.Handle(
            new PermanentDeleteWindowCommand
            {
                Id = inactiveWindow.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(activeResult.IsFailure);
        Assert.True(terminalResult.IsFailure);
        Assert.True(displayResult.IsFailure);
        Assert.True(inactiveResult.IsSuccess);
        Assert.Contains(
            activeResult.Errors,
            error => error.Code == "Windows.PermanentDelete.MustBeInactive");
        Assert.Contains(
            terminalResult.Errors,
            error => error.Code == "Windows.PermanentDelete.HasRelatedRecords");
        Assert.Contains(
            displayResult.Errors,
            error => error.Code == "Windows.PermanentDelete.HasRelatedRecords");
        Assert.DoesNotContain(windows, window => window.Id == inactiveWindow.Id);
        Assert.Equal(1, writeRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private static WaitingArea WaitingArea(int id = 1)
    {
        var branch = EntityTestFactory.Branch(id);
        return EntityTestFactory.WaitingArea(
            id,
            branch.Id,
            id,
            branch,
            descriptiveName: $"Area {id}");
    }

    private static CreateWindowCommandHandler CreateHandler(
        List<WaitingArea> waitingAreas,
        List<Branch> branches,
        List<Window> windows,
        InMemoryWriteRepository<Window> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Branch>(branches),
            new InMemoryWriteReadRepository<Window>(windows),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static UpdateWindowCommandHandler UpdateHandler(
        List<Window> windows,
        List<WaitingArea> waitingAreas,
        List<Branch> branches,
        InMemoryWriteRepository<Window> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Branch>(branches),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);
}
