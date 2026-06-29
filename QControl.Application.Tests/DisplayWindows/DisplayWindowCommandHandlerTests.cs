using BuildingBlock.Domain.Primitive;
using Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;
using Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;
using Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;
using Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.DisplayWindows;

public sealed class DisplayWindowCommandHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Assign_succeeds_for_same_branch_pair_creates_one_link_and_saves_once()
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
        Assert.Equal(branch.Id, displayWindows[0].BranchId);
        Assert.Equal(display.Id, displayWindows[0].DisplayId);
        Assert.Equal(window.Id, displayWindows[0].WindowId);
        Assert.Equal(
            EntityTestFactory.CurrentUserId,
            displayWindows[0].CreatedByApplicationUserId);
        Assert.Equal(1, writeRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, unitOfWork.BeginTransactionCallCount);
    }

    [Fact]
    public async Task Assign_allows_inactive_display_and_window()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isInactive: true);
        var window = Window(20, branch, isInactive: true);
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

        Assert.True(result.IsSuccess);
        Assert.Single(displayWindows);
        Assert.False(display.IsActive);
        Assert.False(window.IsActive);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_returns_not_found_for_missing_display_or_window()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var window = Window(20, branch);
        var displayWindows = new List<DisplayWindow>();
        var unitOfWork = new TestUnitOfWork();

        var missingDisplayResult = await AssignHandler(
                new List<Display>(),
                new List<Window> { window },
                displayWindows,
                new InMemoryWriteRepository<DisplayWindow>(displayWindows),
                unitOfWork)
            .Handle(
                new AssignWindowToDisplayCommand
                {
                    DisplayId = 99,
                    WindowId = window.Id
                },
                CancellationToken.None);
        var missingWindowResult = await AssignHandler(
                new List<Display> { display },
                new List<Window>(),
                displayWindows,
                new InMemoryWriteRepository<DisplayWindow>(displayWindows),
                unitOfWork)
            .Handle(
                new AssignWindowToDisplayCommand
                {
                    DisplayId = display.Id,
                    WindowId = 99
                },
                CancellationToken.None);

        Assert.True(missingDisplayResult.IsFailure);
        Assert.True(missingWindowResult.IsFailure);
        Assert.Contains(
            missingDisplayResult.Errors,
            error => error.Code == "DisplayWindows.Assign.DisplayNotFound");
        Assert.Contains(
            missingWindowResult.Errors,
            error => error.Code == "DisplayWindows.Assign.WindowNotFound");
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
    public async Task Unassign_does_not_modify_display_or_window_state()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isInactive: true);
        var window = Window(20, branch, isInactive: true);
        var originalDisplayNumber = display.Number;
        var originalWindowNumber = window.Number;
        var originalDisplayLastModifiedBy =
            display.LastModifiedByApplicationUserId;
        var originalWindowLastModifiedBy =
            window.LastModifiedByApplicationUserId;
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
        Assert.False(display.IsActive);
        Assert.False(window.IsActive);
        Assert.Equal(originalDisplayNumber, display.Number);
        Assert.Equal(originalWindowNumber, window.Number);
        Assert.Equal(
            originalDisplayLastModifiedBy,
            display.LastModifiedByApplicationUserId);
        Assert.Equal(
            originalWindowLastModifiedBy,
            window.LastModifiedByApplicationUserId);
    }

    [Fact]
    public async Task Unassign_removes_final_link_so_permanent_delete_can_proceed()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isInactive: true);
        var window = Window(20, branch, isInactive: true);
        var link = Link(30, display, window);
        var displays = new List<Display> { display };
        var windows = new List<Window> { window };
        var displayWindows = new List<DisplayWindow> { link };
        var displayWriteRepository =
            new InMemoryWriteRepository<Display>(displays);
        var windowWriteRepository =
            new InMemoryWriteRepository<Window>(windows);
        var displayPermanentHandler = PermanentDisplayHandler(
            displays,
            displayWindows,
            displayWriteRepository,
            new TestUnitOfWork());
        var windowPermanentHandler = PermanentWindowHandler(
            windows,
            displayWindows,
            windowWriteRepository,
            new TestUnitOfWork());

        var blockedDisplay = await displayPermanentHandler.Handle(
            new PermanentDeleteDisplayCommand
            {
                Id = display.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var blockedWindow = await windowPermanentHandler.Handle(
            new PermanentDeleteWindowCommand
            {
                Id = window.Id,
                RowVersion = ValidRowVersion
            },
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
            new PermanentDeleteDisplayCommand
            {
                Id = display.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);
        var windowDelete = await windowPermanentHandler.Handle(
            new PermanentDeleteWindowCommand
            {
                Id = window.Id,
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(unassign.IsSuccess);
        Assert.True(displayDelete.IsSuccess);
        Assert.True(windowDelete.IsSuccess);
        Assert.Empty(displayWindows);
        Assert.Empty(displays);
        Assert.Empty(windows);
        Assert.Equal(1, displayWriteRepository.DeleteCallCount);
        Assert.Equal(1, windowWriteRepository.DeleteCallCount);
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
        InMemoryWriteRepository<Display> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);

    private static PermanentDeleteWindowCommandHandler PermanentWindowHandler(
        List<Window> windows,
        List<DisplayWindow> displayWindows,
        InMemoryWriteRepository<Window> writeRepository,
        TestUnitOfWork unitOfWork)
        => new(
            new InMemoryWriteReadRepository<Window>(windows),
            new InMemoryWriteReadRepository<Terminal>(new List<Terminal>()),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            writeRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            unitOfWork);

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
        bool isInactive = false)
    {
        var waitingArea = WaitingArea(id, branch);

        return EntityTestFactory.Window(
            id,
            waitingArea.Id,
            number,
            waitingArea,
            isInactive: isInactive);
    }

    private static Display Display(
        int id,
        Branch branch,
        string number = "D-01",
        bool isInactive = false)
        => EntityTestFactory.Display(
            id,
            branch.Id,
            number,
            ipAddress: $"192.168.1.{id}",
            serialNo: $"DISPLAY-SN-{id}",
            type: "LED Display",
            branch,
            isInactive);

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
}
