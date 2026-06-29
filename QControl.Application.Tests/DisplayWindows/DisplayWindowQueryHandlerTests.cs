using Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;
using Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.DisplayWindows;

public sealed class DisplayWindowQueryHandlerTests
{
    [Fact]
    public async Task Linked_returns_requested_display_links_including_inactive_and_legacy_cross_branch_windows()
    {
        var branch1 = Branch(1);
        var branch2 = Branch(2);
        var display = Display(10, branch1);
        var otherDisplay = Display(11, branch1, number: "D-02");
        var inactiveLinked = Window(
            10,
            branch1,
            number: "W-01",
            descriptiveName: "Alpha",
            ipAddress: "10.0.0.1",
            isInactive: true);
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
        var displayWindows = new List<DisplayWindow>
        {
            Link(1, display, activeLinked),
            Link(2, display, inactiveLinked),
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
            new[] { inactiveLinked.Id, activeLinked.Id, legacyCrossBranch.Id },
            result.Value.Data.Select(x => x.Id).ToArray());
        Assert.Contains(
            result.Value.Data,
            item => item.Id == inactiveLinked.Id && !item.IsActive);
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
            item => item.Id == otherDisplayWindow.Id);
        Assert.Equal(
            typeof(DisplayLinkedWindowResponse),
            result.Value.Data[0].GetType());
    }

    [Fact]
    public async Task Linked_works_when_display_is_inactive()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isInactive: true);
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

    [Theory]
    [InlineData("W-02")]
    [InlineData("Beta")]
    [InlineData("10.0.0.2")]
    [InlineData("inactive")]
    public async Task Linked_searches_window_fields_and_status(string search)
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var matching = Window(
            20,
            branch,
            number: "W-02",
            descriptiveName: "Beta",
            ipAddress: "10.0.0.2",
            isInactive: search == "inactive");
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
    }

    [Fact]
    public async Task Available_returns_same_branch_windows_and_excludes_only_current_display_links()
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
        var inactive = Window(
            40,
            branch1,
            number: "W-04",
            descriptiveName: "Inactive",
            ipAddress: "10.1.0.4",
            isInactive: true);
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
            inactive,
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
        Assert.Equal(3, result.Value.TotalItems);
        Assert.Equal(
            new[] { available.Id, linkedToOtherDisplay.Id, inactive.Id },
            result.Value.Data.Select(x => x.Id).ToArray());
        Assert.DoesNotContain(
            result.Value.Data,
            item =>
                item.Id == linkedToCurrent.Id ||
                item.Id == otherBranch.Id);
        Assert.True(result.Value.Data[0].EnableTicketBooking);
        Assert.True(result.Value.Data[1].EnableDirectCall);
        Assert.False(result.Value.Data[2].IsActive);
    }

    [Fact]
    public async Task Available_inactive_display_returns_available_windows()
    {
        var branch = Branch(1);
        var display = Display(10, branch, isInactive: true);
        var window = Window(20, branch);
        var handler = AvailableHandler(
            new List<Display> { display },
            new List<Window> { window });

        var result = await handler.Handle(
            new GetDisplayAvailableWindowsQuery
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

    [Theory]
    [InlineData("W-02")]
    [InlineData("Beta")]
    [InlineData("10.1.0.2")]
    [InlineData("inactive")]
    public async Task Available_searches_window_fields_and_status(string search)
    {
        var branch = Branch(1);
        var display = Display(10, branch);
        var matching = Window(
            20,
            branch,
            number: "W-02",
            descriptiveName: "Beta",
            ipAddress: "10.1.0.2",
            isInactive: search == "inactive");
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
        bool isInactive = false)
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
