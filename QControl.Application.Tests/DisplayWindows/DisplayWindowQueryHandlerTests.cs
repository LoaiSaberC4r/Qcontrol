using Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;
using Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.DisplayWindows;

public sealed class DisplayWindowQueryHandlerTests
{
    [Fact]
    public async Task Linked_returns_only_requested_display_links_including_deleted_and_legacy_cross_branch_windows()
    {
        var branch1 = Branch(1);
        var branch2 = Branch(2);
        var display = Display(10, branch1);
        var otherDisplay = Display(11, branch1, number: "D-02");
        var deletedLinked = Window(
            10,
            branch1,
            number: "W-01",
            descriptiveName: "Alpha",
            ipAddress: "10.0.0.1",
            isDeleted: true);
        var activeLinked = Window(
            20,
            branch1,
            number: "W-02",
            descriptiveName: "Beta",
            ipAddress: "10.0.0.2",
            enableTicketBooking: true,
            enableDirectCall: true);
        var legacyCrossBranch = Window(
            30,
            branch2,
            number: "W-03",
            descriptiveName: "Legacy",
            ipAddress: "10.0.0.3");
        var otherDisplayWindow = Window(
            40,
            branch1,
            number: "W-04",
            descriptiveName: "Other",
            ipAddress: "10.0.0.4");
        var unlinked = Window(
            50,
            branch1,
            number: "W-05",
            descriptiveName: "Unlinked",
            ipAddress: "10.0.0.5");
        var displayWindows = new List<DisplayWindow>
        {
            Link(1, display, activeLinked),
            Link(2, display, deletedLinked),
            Link(3, display, legacyCrossBranch),
            Link(4, otherDisplay, otherDisplayWindow)
        };
        var handler = LinkedHandler(
            new List<Display> { display, otherDisplay },
            displayWindows);

        var result = await handler.Handle(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = display.Id,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItems);
        Assert.Equal(
            new[] { deletedLinked.Id, activeLinked.Id, legacyCrossBranch.Id },
            result.Value.Data.Select(x => x.Id).ToArray());
        Assert.Contains(
            result.Value.Data,
            item => item.Id == deletedLinked.Id && item.IsDeleted);
        Assert.Contains(
            result.Value.Data,
            item =>
                item.Id == activeLinked.Id &&
                item.EnableTicketBooking &&
                item.EnableDirectCall);
        Assert.Contains(
            result.Value.Data,
            item => item.Id == legacyCrossBranch.Id);
        Assert.DoesNotContain(
            result.Value.Data,
            item => item.Id == otherDisplayWindow.Id || item.Id == unlinked.Id);
        Assert.Equal(
            deletedLinked.WaitingArea.Number,
            result.Value.Data[0].WaitingAreaNumber);
        Assert.Equal(
            deletedLinked.WaitingArea.DescriptiveName,
            result.Value.Data[0].WaitingAreaDescriptiveName);
        Assert.Equal(
            typeof(DisplayLinkedWindowResponse),
            result.Value.Data[0].GetType());
    }

    [Fact]
    public async Task Linked_works_when_display_is_soft_deleted()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isDeleted: true);
        var window = Window(20, branch);
        var displayWindows = new List<DisplayWindow>
        {
            Link(1, display, window)
        };
        var handler = LinkedHandler(
            new List<Display> { display },
            displayWindows);

        var result = await handler.Handle(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = display.Id,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Data);
        Assert.Equal(window.Id, result.Value.Data[0].Id);
    }

    [Fact]
    public async Task Linked_missing_display_returns_not_found()
    {
        var handler = LinkedHandler(
            new List<Display>(),
            new List<DisplayWindow>());

        var result = await handler.Handle(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = 99,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Linked.DisplayNotFound");
    }

    [Theory]
    [InlineData("W-02")]
    [InlineData("Beta")]
    [InlineData("10.0.0.2")]
    public async Task Linked_searches_number_descriptive_name_and_ip_address(
        string search)
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var matching = Window(
            20,
            branch,
            number: "W-02",
            descriptiveName: "Beta",
            ipAddress: "10.0.0.2");
        var nonMatching = Window(
            30,
            branch,
            number: "W-03",
            descriptiveName: "Gamma",
            ipAddress: "10.0.0.3");
        var displayWindows = new List<DisplayWindow>
        {
            Link(1, display, matching),
            Link(2, display, nonMatching)
        };
        var handler = LinkedHandler(
            new List<Display> { display },
            displayWindows);

        var result = await handler.Handle(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = display.Id,
                Search = $" {search} ",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Data);
        Assert.Equal(matching.Id, result.Value.Data[0].Id);
        Assert.Equal(1, result.Value.TotalItems);
    }

    [Fact]
    public async Task Linked_paginates_and_reports_total_count()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var first = Window(10, branch, number: "W-01");
        var second = Window(20, branch, number: "W-02");
        var third = Window(30, branch, number: "W-03");
        var displayWindows = new List<DisplayWindow>
        {
            Link(1, display, first),
            Link(2, display, second),
            Link(3, display, third)
        };
        var handler = LinkedHandler(
            new List<Display> { display },
            displayWindows);

        var result = await handler.Handle(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = display.Id,
                PageNumber = 2,
                PageSize = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItems);
        Assert.Single(result.Value.Data);
        Assert.Equal(second.Id, result.Value.Data[0].Id);
    }

    [Fact]
    public async Task Linked_authentication_failure_returns_security_error()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var handler = LinkedHandler(
            new List<Display> { display },
            new List<DisplayWindow>(),
            new TestCurrentUser
            {
                IsAuthenticated = false,
                UserId = null
            });

        var result = await handler.Handle(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = display.Id,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Linked.Unauthenticated");
    }

    [Fact]
    public async Task Available_returns_active_same_branch_windows_and_excludes_only_current_display_links()
    {
        var branch1 = Branch(1);
        var branch2 = Branch(2);
        var display = Display(10, branch1);
        var otherDisplay = Display(11, branch1, number: "D-02");
        var available = Window(
            10,
            branch1,
            number: "W-01",
            descriptiveName: "Available",
            ipAddress: "10.1.0.1",
            enableTicketBooking: true);
        var linkedToCurrent = Window(
            20,
            branch1,
            number: "W-02",
            descriptiveName: "Current",
            ipAddress: "10.1.0.2");
        var linkedToOtherDisplay = Window(
            30,
            branch1,
            number: "W-03",
            descriptiveName: "Shared",
            ipAddress: "10.1.0.3",
            enableDirectCall: true);
        var deleted = Window(
            40,
            branch1,
            number: "W-04",
            descriptiveName: "Deleted",
            ipAddress: "10.1.0.4",
            isDeleted: true);
        var otherBranch = Window(
            50,
            branch2,
            number: "W-05",
            descriptiveName: "Other Branch",
            ipAddress: "10.2.0.1");
        var displayWindows = new List<DisplayWindow>
        {
            Link(1, display, linkedToCurrent),
            Link(2, otherDisplay, linkedToOtherDisplay)
        };
        var windows = new List<Window>
        {
            available,
            linkedToCurrent,
            linkedToOtherDisplay,
            deleted,
            otherBranch
        };
        var handler = AvailableHandler(
            new List<Display> { display, otherDisplay },
            windows);

        var result = await handler.Handle(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = display.Id,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.Equal(
            new[] { available.Id, linkedToOtherDisplay.Id },
            result.Value.Data.Select(x => x.Id).ToArray());
        Assert.DoesNotContain(
            result.Value.Data,
            item =>
                item.Id == linkedToCurrent.Id ||
                item.Id == deleted.Id ||
                item.Id == otherBranch.Id);
        Assert.True(result.Value.Data[0].EnableTicketBooking);
        Assert.True(result.Value.Data[1].EnableDirectCall);
        Assert.Equal(
            available.WaitingArea.Number,
            result.Value.Data[0].WaitingAreaNumber);
        Assert.Equal(
            typeof(AvailableWindowResponse),
            result.Value.Data[0].GetType());
        Assert.Equal(2, displayWindows.Count);
    }

    [Fact]
    public async Task Available_missing_display_returns_not_found()
    {
        var handler = AvailableHandler(
            new List<Display>(),
            new List<Window>());

        var result = await handler.Handle(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = 99,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Available.DisplayNotFound");
    }

    [Fact]
    public async Task Available_soft_deleted_display_returns_conflict()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isDeleted: true);
        var handler = AvailableHandler(
            new List<Display> { display },
            new List<Window>());

        var result = await handler.Handle(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = display.Id,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Available.DisplayDeleted");
    }

    [Theory]
    [InlineData("W-02")]
    [InlineData("Beta")]
    [InlineData("10.1.0.2")]
    public async Task Available_searches_number_descriptive_name_and_ip_address(
        string search)
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var matching = Window(
            20,
            branch,
            number: "W-02",
            descriptiveName: "Beta",
            ipAddress: "10.1.0.2");
        var nonMatching = Window(
            30,
            branch,
            number: "W-03",
            descriptiveName: "Gamma",
            ipAddress: "10.1.0.3");
        var handler = AvailableHandler(
            new List<Display> { display },
            new List<Window> { matching, nonMatching });

        var result = await handler.Handle(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = display.Id,
                Search = $" {search} ",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Data);
        Assert.Equal(matching.Id, result.Value.Data[0].Id);
        Assert.Equal(1, result.Value.TotalItems);
    }

    [Fact]
    public async Task Available_paginates_and_reports_total_count()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var first = Window(10, branch, number: "W-01");
        var second = Window(20, branch, number: "W-02");
        var third = Window(30, branch, number: "W-03");
        var handler = AvailableHandler(
            new List<Display> { display },
            new List<Window> { first, second, third });

        var result = await handler.Handle(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = display.Id,
                PageNumber = 2,
                PageSize = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItems);
        Assert.Single(result.Value.Data);
        Assert.Equal(second.Id, result.Value.Data[0].Id);
    }

    [Fact]
    public async Task Available_authentication_failure_returns_security_error()
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var handler = AvailableHandler(
            new List<Display> { display },
            new List<Window>(),
            new TestCurrentUser
            {
                IsAuthenticated = false,
                UserId = null
            });

        var result = await handler.Handle(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = display.Id,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DisplayWindows.Available.Unauthenticated");
    }

    private static GetDisplayLinkedWindowsQueryHandler LinkedHandler(
        List<Display> displays,
        List<DisplayWindow> displayWindows,
        TestCurrentUser? currentUser = null)
        => new(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<DisplayWindow>(displayWindows),
            currentUser ?? new TestCurrentUser());

    private static GetDisplayAvailableWindowsQueryHandler AvailableHandler(
        List<Display> displays,
        List<Window> windows,
        TestCurrentUser? currentUser = null)
        => new(
            new InMemoryWriteReadRepository<Display>(displays),
            new InMemoryWriteReadRepository<Window>(windows),
            currentUser ?? new TestCurrentUser());

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
        string? descriptiveName = null,
        string? ipAddress = null,
        bool enableTicketBooking = false,
        bool enableDirectCall = false,
        bool isDeleted = false)
    {
        var waitingArea = WaitingArea(id, branch);

        return EntityTestFactory.Window(
            id,
            waitingArea.Id,
            number,
            waitingArea,
            descriptiveName,
            ipAddress,
            enableTicketBooking,
            enableDirectCall,
            isDeleted);
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
}
