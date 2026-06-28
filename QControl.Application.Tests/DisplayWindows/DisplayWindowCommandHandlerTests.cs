using BuildingBlock.Domain.Primitive;
using Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;
using Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;
using Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;
using Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.DisplayWindows;

public sealed class DisplayWindowCommandHandlerTests
{
    [Fact]
    public async Task Assign_succeeds_for_active_same_branch_pair_creates_one_link_and_saves_once()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var window = Window(20, branch);
        var displays = new List<Display> { display };
        var windows = new List<Window> { window };
        var displayWindows = new List<DisplayWindow>();
        var writeRepository =
            new InMemoryWriteRepository<DisplayWindow>(displayWindows);
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            displays,
            windows,
            displayWindows,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AssignWindowToDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(displayWindows);
        Assert.Equal(display.Id, displayWindows[0].DisplayId);
        Assert.Equal(window.Id, displayWindows[0].WindowId);
        Assert.Equal(
            EntityTestFactory.CurrentUserId,
            displayWindows[0].CreatedByApplicationUserId);
        Assert.Equal(display.Id, result.Value.DisplayId);
        Assert.Equal(window.Id, result.Value.WindowId);
        Assert.Single(displays);
        Assert.Single(windows);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, unitOfWork.BeginTransactionCallCount);
    }

    [Fact]
    public async Task Assign_returns_not_found_when_display_is_missing()
    {
        var branch = Branch(1);
        var window = Window(20, branch);
        var displayWindows = new List<DisplayWindow>();
        var writeRepository =
            new InMemoryWriteRepository<DisplayWindow>(displayWindows);
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Display>(),
            new List<Window> { window },
            displayWindows,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AssignWindowToDisplayCommand
            {
                DisplayId = 99,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Assign.DisplayNotFound");
        Assert.Empty(displayWindows);
        Assert.Equal(0, writeRepository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_returns_conflict_when_display_is_soft_deleted()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isDeleted: true);
        var window = Window(20, branch);
        var displayWindows = new List<DisplayWindow>();
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Display> { display },
            new List<Window> { window },
            displayWindows,
            new InMemoryWriteRepository<DisplayWindow>(displayWindows),
            unitOfWork);

        var result = await handler.Handle(
            new AssignWindowToDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Assign.DisplayDeleted");
        Assert.Empty(displayWindows);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_returns_not_found_when_window_is_missing()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var displayWindows = new List<DisplayWindow>();
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Display> { display },
            new List<Window>(),
            displayWindows,
            new InMemoryWriteRepository<DisplayWindow>(displayWindows),
            unitOfWork);

        var result = await handler.Handle(
            new AssignWindowToDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = 99
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Assign.WindowNotFound");
        Assert.Empty(displayWindows);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_returns_conflict_when_window_is_soft_deleted()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var window = Window(20, branch, isDeleted: true);
        var displayWindows = new List<DisplayWindow>();
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Display> { display },
            new List<Window> { window },
            displayWindows,
            new InMemoryWriteRepository<DisplayWindow>(displayWindows),
            unitOfWork);

        var result = await handler.Handle(
            new AssignWindowToDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Assign.WindowDeleted");
        Assert.Empty(displayWindows);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_returns_conflict_when_display_and_window_are_in_different_branches()
    {
        var displayBranch = Branch(1);
        var windowBranch = Branch(2);
        var display = Display(10, displayBranch);
        var window = Window(20, windowBranch);
        var displayWindows = new List<DisplayWindow>();
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Display> { display },
            new List<Window> { window },
            displayWindows,
            new InMemoryWriteRepository<DisplayWindow>(displayWindows),
            unitOfWork);

        var result = await handler.Handle(
            new AssignWindowToDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Assign.DifferentBranch");
        Assert.Empty(displayWindows);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_returns_conflict_for_existing_pair_and_does_not_save()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var window = Window(20, branch);
        var existingLink = Link(30, display, window);
        var displayWindows = new List<DisplayWindow> { existingLink };
        var writeRepository =
            new InMemoryWriteRepository<DisplayWindow>(displayWindows);
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Display> { display },
            new List<Window> { window },
            displayWindows,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new AssignWindowToDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Assign.AlreadyLinked");
        Assert.Single(displayWindows);
        Assert.Equal(0, writeRepository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_allows_same_window_to_another_display_in_same_branch()
    {
        var branch = Branch(1);
        var firstDisplay = Display(10, branch, number: "D-01");
        var secondDisplay = Display(11, branch, number: "D-02");
        var window = Window(20, branch);
        var existingLink = Link(30, firstDisplay, window);
        var displayWindows = new List<DisplayWindow> { existingLink };
        var unitOfWork = new TestUnitOfWork();
        var handler = AssignHandler(
            new List<Display> { firstDisplay, secondDisplay },
            new List<Window> { window },
            displayWindows,
            new InMemoryWriteRepository<DisplayWindow>(displayWindows),
            unitOfWork);

        var result = await handler.Handle(
            new AssignWindowToDisplayCommand
            {
                DisplayId = secondDisplay.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, displayWindows.Count);
        Assert.Contains(
            displayWindows,
            link =>
                link.DisplayId == firstDisplay.Id &&
                link.WindowId == window.Id);
        Assert.Contains(
            displayWindows,
            link =>
                link.DisplayId == secondDisplay.Id &&
                link.WindowId == window.Id);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Unassign_existing_link_physically_removes_it_and_saves_once()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var window = Window(20, branch);
        var link = Link(30, display, window);
        var displayWindows = new List<DisplayWindow> { link };
        var writeRepository =
            new InMemoryWriteRepository<DisplayWindow>(displayWindows);
        var unitOfWork = new TestUnitOfWork();
        var handler = UnassignHandler(
            displayWindows,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new UnassignWindowFromDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(displayWindows);
        Assert.False(
            typeof(ISoftDeleteEntity).IsAssignableFrom(link.GetType()));
        Assert.Equal(1, writeRepository.DeleteCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Unassign_missing_link_returns_not_found_and_does_not_save()
    {
        var displayWindows = new List<DisplayWindow>();
        var writeRepository =
            new InMemoryWriteRepository<DisplayWindow>(displayWindows);
        var unitOfWork = new TestUnitOfWork();
        var handler = UnassignHandler(
            displayWindows,
            writeRepository,
            unitOfWork);

        var result = await handler.Handle(
            new UnassignWindowFromDisplayCommand
            {
                DisplayId = 10,
                WindowId = 20
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Unassign.LinkNotFound");
        Assert.Equal(0, writeRepository.DeleteCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Unassign_allows_active_or_deleted_parents(
        bool displayDeleted,
        bool windowDeleted)
    {
        var branch = Branch(1);
        var display = Display(10, branch, isDeleted: displayDeleted);
        var window = Window(20, branch, isDeleted: windowDeleted);
        var link = Link(30, display, window);
        var displayWindows = new List<DisplayWindow> { link };
        var unitOfWork = new TestUnitOfWork();
        var handler = UnassignHandler(
            displayWindows,
            new InMemoryWriteRepository<DisplayWindow>(displayWindows),
            unitOfWork);

        var result = await handler.Handle(
            new UnassignWindowFromDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(displayWindows);
        Assert.Equal(displayDeleted, display.IsDeleted);
        Assert.Equal(windowDeleted, window.IsDeleted);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Unassign_does_not_modify_display_or_window()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isDeleted: true);
        var window = Window(20, branch, isDeleted: true);
        var originalDisplayNumber = display.Number;
        var originalWindowNumber = window.Number;
        var link = Link(30, display, window);
        var displayWindows = new List<DisplayWindow> { link };
        var handler = UnassignHandler(
            displayWindows,
            new InMemoryWriteRepository<DisplayWindow>(displayWindows),
            new TestUnitOfWork());

        var result = await handler.Handle(
            new UnassignWindowFromDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(display.IsDeleted);
        Assert.True(window.IsDeleted);
        Assert.Equal(originalDisplayNumber, display.Number);
        Assert.Equal(originalWindowNumber, window.Number);
        Assert.Null(display.LastModifiedByApplicationUserId);
        Assert.Null(window.LastModifiedByApplicationUserId);
    }

    [Fact]
    public async Task Unassign_removes_final_link_so_permanent_delete_can_proceed()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isDeleted: true);
        var window = Window(20, branch, isDeleted: true);
        var link = Link(30, display, window);
        var displays = new List<Display> { display };
        var windows = new List<Window> { window };
        var displayWindows = new List<DisplayWindow> { link };
        var displayPermanentRepository =
            new TestDisplayPermanentDeleteRepository();
        var windowPermanentRepository =
            new TestWindowPermanentDeleteRepository();
        var displayPermanentHandler = PermanentDisplayHandler(
            displays,
            displayWindows,
            displayPermanentRepository);
        var windowPermanentHandler = PermanentWindowHandler(
            windows,
            displayWindows,
            windowPermanentRepository);

        var blockedDisplay = await displayPermanentHandler.Handle(
            new PermanentDeleteDisplayCommand { Id = display.Id },
            CancellationToken.None);
        var blockedWindow = await windowPermanentHandler.Handle(
            new PermanentDeleteWindowCommand { Id = window.Id },
            CancellationToken.None);

        Assert.True(blockedDisplay.IsFailure);
        Assert.True(blockedWindow.IsFailure);

        var unassignHandler = UnassignHandler(
            displayWindows,
            new InMemoryWriteRepository<DisplayWindow>(displayWindows),
            new TestUnitOfWork());

        var unassign = await unassignHandler.Handle(
            new UnassignWindowFromDisplayCommand
            {
                DisplayId = display.Id,
                WindowId = window.Id
            },
            CancellationToken.None);

        var displayDelete = await displayPermanentHandler.Handle(
            new PermanentDeleteDisplayCommand { Id = display.Id },
            CancellationToken.None);
        var windowDelete = await windowPermanentHandler.Handle(
            new PermanentDeleteWindowCommand { Id = window.Id },
            CancellationToken.None);

        Assert.True(unassign.IsSuccess);
        Assert.True(displayDelete.IsSuccess);
        Assert.True(windowDelete.IsSuccess);
        Assert.Empty(displayWindows);
        Assert.Equal(1, displayPermanentRepository.CallCount);
        Assert.Equal(1, windowPermanentRepository.CallCount);
    }

    private static AssignWindowToDisplayCommandHandler AssignHandler(
        List<Display> displays,
        List<Window> windows,
        List<DisplayWindow> displayWindows,
        InMemoryWriteRepository<DisplayWindow> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static UnassignWindowFromDisplayCommandHandler UnassignHandler(
        List<DisplayWindow> displayWindows,
        InMemoryWriteRepository<DisplayWindow> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            writeRepository,
            new TestCurrentUser(),
            unitOfWork);

    private static PermanentDeleteDisplayCommandHandler PermanentDisplayHandler(
        List<Display> displays,
        List<DisplayWindow> displayWindows,
        IDisplayPermanentDeleteRepository permanentRepository)
        => new(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            permanentRepository,
            new TestCurrentUser());

    private static PermanentDeleteWindowCommandHandler PermanentWindowHandler(
        List<Window> windows,
        List<DisplayWindow> displayWindows,
        IWindowPermanentDeleteRepository permanentRepository)
        => new(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<Terminal>(new List<Terminal>()),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            permanentRepository,
            new TestCurrentUser());

    private static Branch Branch(int id)
        => EntityTestFactory.Branch(id);

    private static WaitingArea WaitingArea(int id, Branch branch)
        => EntityTestFactory.WaitingArea(
            id,
            branch.Id,
            id,
            branch,
            descriptiveName: $"Area {id}");

    private static Window Window(
        int id,
        Branch branch,
        string number = "W-01",
        bool isDeleted = false)
    {
        var waitingArea = WaitingArea(id, branch);

        return EntityTestFactory.Window(
            id,
            waitingArea.Id,
            number,
            waitingArea,
            isDeleted: isDeleted);
    }

    private static Display Display(
        int id,
        Branch branch,
        string number = "D-01",
        bool isDeleted = false)
        => EntityTestFactory.Display(
            id,
            branch.Id,
            number,
            ipAddress: $"192.168.1.{id}",
            serialNo: $"DISPLAY-SN-{id}",
            type: "LED Display",
            branch,
            isDeleted);

    private static DisplayWindow Link(
        int id,
        Display display,
        Window window)
        => EntityTestFactory.DisplayWindow(
            id,
            display.Id,
            window.Id,
            display,
            window);

    private sealed class TestDisplayPermanentDeleteRepository
        : IDisplayPermanentDeleteRepository
    {
        public int CallCount { get; private set; }

        public Task<int> DeletePermanentlyAsync(
            int displayId,
            CancellationToken cancellationToken)
        {
            CallCount++;

            return Task.FromResult(1);
        }
    }

    private sealed class TestWindowPermanentDeleteRepository
        : IWindowPermanentDeleteRepository
    {
        public int CallCount { get; private set; }

        public Task<int> DeletePermanentlyAsync(
            int windowId,
            CancellationToken cancellationToken)
        {
            CallCount++;

            return Task.FromResult(1);
        }
    }
}
