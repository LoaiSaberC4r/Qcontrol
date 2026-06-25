using Qcontrol.Application.Features.Windows.Command.CreateWindow;
using Qcontrol.Application.Features.Windows.Command.DeleteWindow;
using Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;
using Qcontrol.Application.Features.Windows.Command.UpdateWindow;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Windows;

public sealed class WindowCommandHandlerTests
{
    [Fact]
    public async Task Create_succeeds_trims_values_allows_both_flags_and_commits_once()
    {
        var waitingArea = WaitingArea();
        var windows = new List<Window>();
        var unitOfWork = new TestUnitOfWork();
        var writeRepository = new InMemoryWriteRepository<Window>(windows);
        var handler = CreateHandler(
            new List<WaitingArea> { waitingArea },
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
        Assert.Equal("5", result.Value.Number);
        Assert.Equal("Main Window", result.Value.DescriptiveName);
        Assert.Equal("192.168.1.10", result.Value.IPAddress);
        Assert.True(result.Value.EnableTicketBooking);
        Assert.True(result.Value.EnableDirectCall);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_returns_waiting_area_not_found()
    {
        var windows = new List<Window>();
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<WaitingArea>(),
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var result = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = 99,
                Number = "1"
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "Windows.Create.WaitingAreaNotFound");
        Assert.Empty(windows);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_rejects_duplicate_number_in_same_waiting_area_including_deleted()
    {
        var waitingArea = WaitingArea();
        var windows = new List<Window>
        {
            EntityTestFactory.Window(
                10,
                waitingArea.Id,
                "5",
                waitingArea,
                isDeleted: true)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<WaitingArea> { waitingArea },
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var result = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = waitingArea.Id,
                Number = "5"
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error =>
                error.Code ==
                "Windows.Create.NumberAlreadyExistsInWaitingArea");
        Assert.Single(windows);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_allows_same_number_in_another_waiting_area()
    {
        var area1 = WaitingArea(id: 1);
        var area2 = WaitingArea(id: 2);
        var windows = new List<Window>
        {
            EntityTestFactory.Window(10, area2.Id, "5", area2)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<WaitingArea> { area1, area2 },
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var result = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = area1.Id,
                Number = "5"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, windows.Count);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_rejects_duplicate_ip_in_same_waiting_area_including_deleted()
    {
        var waitingArea = WaitingArea();
        var windows = new List<Window>
        {
            EntityTestFactory.Window(
                10,
                waitingArea.Id,
                "1",
                waitingArea,
                ipAddress: "192.168.1.10",
                isDeleted: true)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<WaitingArea> { waitingArea },
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var result = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = waitingArea.Id,
                Number = "2",
                IPAddress = "192.168.1.10"
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error =>
                error.Code ==
                "Windows.Create.IPAddressAlreadyExistsInWaitingArea");
        Assert.Single(windows);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_allows_same_ip_in_another_waiting_area_and_null_ip()
    {
        var area1 = WaitingArea(id: 1);
        var area2 = WaitingArea(id: 2);
        var windows = new List<Window>
        {
            EntityTestFactory.Window(
                10,
                area2.Id,
                "1",
                area2,
                ipAddress: "192.168.1.10")
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(
            new List<WaitingArea> { area1, area2 },
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var sameIpResult = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = area1.Id,
                Number = "2",
                IPAddress = "192.168.1.10"
            },
            CancellationToken.None);

        var nullIpResult = await handler.Handle(
            new CreateWindowCommand
            {
                WaitingAreaId = area1.Id,
                Number = "3",
                IPAddress = null
            },
            CancellationToken.None);

        Assert.True(sameIpResult.IsSuccess);
        Assert.True(nullIpResult.IsSuccess);
        Assert.Equal(3, windows.Count);
        Assert.Equal(2, unitOfWork.SaveChangesCallCount);
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
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new UpdateWindowCommand
            {
                Id = window.Id,
                RequestId = window.Id,
                Number = " 8 ",
                DescriptiveName = " Updated ",
                IPAddress = " 192.168.1.20 ",
                EnableTicketBooking = true,
                EnableDirectCall = true
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(waitingArea.Id, window.WaitingAreaId);
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
    public async Task Update_allows_existing_number_for_same_window()
    {
        var waitingArea = WaitingArea();
        var window =
            EntityTestFactory.Window(10, waitingArea.Id, "5", waitingArea);
        var windows = new List<Window> { window };
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var result = await handler.Handle(
            new UpdateWindowCommand
            {
                Id = window.Id,
                RequestId = window.Id,
                Number = "5"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_returns_not_found_for_missing_or_deleted_window()
    {
        var waitingArea = WaitingArea();
        var deletedWindow = EntityTestFactory.Window(
            10,
            waitingArea.Id,
            "1",
            waitingArea,
            isDeleted: true);
        var windows = new List<Window> { deletedWindow };
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var missingResult = await handler.Handle(
            new UpdateWindowCommand
            {
                Id = 99,
                RequestId = 99,
                Number = "1"
            },
            CancellationToken.None);

        var deletedResult = await handler.Handle(
            new UpdateWindowCommand
            {
                Id = deletedWindow.Id,
                RequestId = deletedWindow.Id,
                Number = "2"
            },
            CancellationToken.None);

        Assert.True(missingResult.IsFailure);
        Assert.True(deletedResult.IsFailure);
        Assert.Contains(
            deletedResult.Errors,
            error => error.Code == "Windows.Update.WindowNotFound");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_rejects_duplicate_number_and_ip_including_deleted_windows()
    {
        var waitingArea = WaitingArea();
        var window =
            EntityTestFactory.Window(10, waitingArea.Id, "1", waitingArea);
        var duplicate = EntityTestFactory.Window(
            11,
            waitingArea.Id,
            "8",
            waitingArea,
            ipAddress: "192.168.1.10",
            isDeleted: true);
        var windows = new List<Window> { window, duplicate };
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(
            windows,
            new InMemoryWriteRepository<Window>(windows),
            unitOfWork);

        var numberResult = await handler.Handle(
            new UpdateWindowCommand
            {
                Id = window.Id,
                RequestId = window.Id,
                Number = "8"
            },
            CancellationToken.None);

        var ipResult = await handler.Handle(
            new UpdateWindowCommand
            {
                Id = window.Id,
                RequestId = window.Id,
                Number = "2",
                IPAddress = "192.168.1.10"
            },
            CancellationToken.None);

        Assert.True(numberResult.IsFailure);
        Assert.True(ipResult.IsFailure);
        Assert.Contains(
            numberResult.Errors,
            error =>
                error.Code ==
                "Windows.Update.NumberAlreadyExistsInWaitingArea");
        Assert.Contains(
            ipResult.Errors,
            error =>
                error.Code ==
                "Windows.Update.IPAddressAlreadyExistsInWaitingArea");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Delete_soft_deletes_active_window_and_preserves_relationships()
    {
        var waitingArea = WaitingArea();
        var window =
            EntityTestFactory.Window(10, waitingArea.Id, "1", waitingArea);
        var terminal =
            EntityTestFactory.Terminal(20, window.Id, window: window);
        var displayWindow =
            EntityTestFactory.DisplayWindow(30, 1, window.Id, window: window);
        var windows = new List<Window> { window };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository = new InMemoryWriteRepository<Window>(windows);
        var handler = DeleteHandler(
            windows,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new DeleteWindowCommand { Id = window.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(windows);
        Assert.True(window.IsDeleted);
        Assert.NotNull(window.DeletedOnUtc);
        Assert.Contains(terminal, window.Terminals);
        Assert.Contains(displayWindow, window.DisplayWindows);
        Assert.Equal(1, writeRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Delete_returns_not_found_for_missing_or_already_deleted_window()
    {
        var waitingArea = WaitingArea();
        var deletedWindow = EntityTestFactory.Window(
            10,
            waitingArea.Id,
            "1",
            waitingArea,
            isDeleted: true);
        var windows = new List<Window> { deletedWindow };
        var unitOfWork = new TestUnitOfWork();
        var writeRepository = new InMemoryWriteRepository<Window>(windows);
        var handler = DeleteHandler(windows, writeRepository, unitOfWork);

        var missingResult = await handler.Handle(
            new DeleteWindowCommand { Id = 99 },
            CancellationToken.None);
        var deletedResult = await handler.Handle(
            new DeleteWindowCommand { Id = deletedWindow.Id },
            CancellationToken.None);

        Assert.True(missingResult.IsFailure);
        Assert.True(deletedResult.IsFailure);
        Assert.Equal(0, writeRepository.DeleteCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Permanent_delete_blocks_active_or_missing_window()
    {
        var waitingArea = WaitingArea();
        var activeWindow =
            EntityTestFactory.Window(10, waitingArea.Id, "1", waitingArea);
        var permanentRepository =
            new TestWindowPermanentDeleteRepository();
        var handler = PermanentDeleteHandler(
            new List<Window> { activeWindow },
            new List<Terminal>(),
            new List<DisplayWindow>(),
            permanentRepository);

        var activeResult = await handler.Handle(
            new PermanentDeleteWindowCommand { Id = activeWindow.Id },
            CancellationToken.None);
        var missingResult = await handler.Handle(
            new PermanentDeleteWindowCommand { Id = 99 },
            CancellationToken.None);

        Assert.True(activeResult.IsFailure);
        Assert.True(missingResult.IsFailure);
        Assert.Contains(
            activeResult.Errors,
            error =>
                error.Code ==
                "Windows.PermanentDelete.MustBeSoftDeleted");
        Assert.Contains(
            missingResult.Errors,
            error => error.Code == "Windows.PermanentDelete.WindowNotFound");
        Assert.Equal(0, permanentRepository.CallCount);
    }

    [Fact]
    public async Task Permanent_delete_blocks_related_terminal_or_display_window()
    {
        var waitingArea = WaitingArea();
        var terminalWindow = EntityTestFactory.Window(
            10,
            waitingArea.Id,
            "1",
            waitingArea,
            isDeleted: true);
        var displayWindowOwner = EntityTestFactory.Window(
            11,
            waitingArea.Id,
            "2",
            waitingArea,
            isDeleted: true);
        var terminals = new List<Terminal>
        {
            EntityTestFactory.Terminal(20, terminalWindow.Id)
        };
        var displayWindows = new List<DisplayWindow>
        {
            EntityTestFactory.DisplayWindow(30, 1, displayWindowOwner.Id)
        };
        var permanentRepository =
            new TestWindowPermanentDeleteRepository();
        var handler = PermanentDeleteHandler(
            new List<Window> { terminalWindow, displayWindowOwner },
            terminals,
            displayWindows,
            permanentRepository);

        var terminalResult = await handler.Handle(
            new PermanentDeleteWindowCommand { Id = terminalWindow.Id },
            CancellationToken.None);
        var displayResult = await handler.Handle(
            new PermanentDeleteWindowCommand { Id = displayWindowOwner.Id },
            CancellationToken.None);

        Assert.True(terminalResult.IsFailure);
        Assert.True(displayResult.IsFailure);
        Assert.Contains(
            terminalResult.Errors,
            error =>
                error.Code ==
                "Windows.PermanentDelete.HasRelatedRecords");
        Assert.Equal(0, permanentRepository.CallCount);
    }

    [Fact]
    public async Task Permanent_delete_calls_repository_once_for_soft_deleted_window()
    {
        var waitingArea = WaitingArea();
        var window = EntityTestFactory.Window(
            10,
            waitingArea.Id,
            "1",
            waitingArea,
            isDeleted: true);
        var permanentRepository =
            new TestWindowPermanentDeleteRepository();
        var handler = PermanentDeleteHandler(
            new List<Window> { window },
            new List<Terminal>(),
            new List<DisplayWindow>(),
            permanentRepository);

        var result = await handler.Handle(
            new PermanentDeleteWindowCommand { Id = window.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, permanentRepository.CallCount);
        Assert.Equal(window.Id, permanentRepository.LastWindowId);
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
        List<Window> windows,
        InMemoryWriteRepository<Window> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<WaitingArea>(waitingAreas),
            new InMemoryWriteReadRepository<Window>(windows),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static UpdateWindowCommandHandler UpdateHandler(
        List<Window> windows,
        InMemoryWriteRepository<Window> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Window>(windows),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static DeleteWindowCommandHandler DeleteHandler(
        List<Window> windows,
        InMemoryWriteRepository<Window> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Window>(windows),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static PermanentDeleteWindowCommandHandler PermanentDeleteHandler(
        List<Window> windows,
        List<Terminal> terminals,
        List<DisplayWindow> displayWindows,
        TestWindowPermanentDeleteRepository permanentRepository)
        => new(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<Terminal>(terminals),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            permanentRepository,
            new TestCurrentUser());

    private sealed class TestWindowPermanentDeleteRepository
        : IWindowPermanentDeleteRepository
    {
        public int CallCount { get; private set; }

        public int? LastWindowId { get; private set; }

        public int DeletedRows { get; init; } = 1;

        public Task<int> DeletePermanentlyAsync(
            int windowId,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastWindowId = windowId;

            return Task.FromResult(DeletedRows);
        }
    }
}
