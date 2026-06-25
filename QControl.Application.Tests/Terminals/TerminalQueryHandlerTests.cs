using Qcontrol.Application.Features.Terminals.Query.GetDeletedTerminals;
using Qcontrol.Application.Features.Terminals.Query.GetTerminalById;
using Qcontrol.Application.Features.Terminals.Query.GetTerminals;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Terminals;

public sealed class TerminalQueryHandlerTests
{
    [Fact]
    public async Task Active_list_excludes_deleted_filters_search_and_projects_context()
    {
        var branch = EntityTestFactory.Branch(
            1,
            arabicName: "فرع 1",
            englishName: "Branch 1");
        var area = WaitingArea(1, branch);
        var window1 = Window(1, area, number: "2");
        var window2 = Window(2, area, number: "1");
        var active1 = Terminal(
            10,
            window1,
            number: "T-02",
            ipAddress: "192.168.1.22",
            serialNo: "ABC-200",
            type: "Kiosk Module");
        var active2 = Terminal(
            11,
            window2,
            number: "T-01",
            ipAddress: "192.168.1.20",
            serialNo: "ABC-100",
            type: "Operator Module");
        var deleted = Terminal(
            12,
            window1,
            number: "SELF-01",
            ipAddress: "192.168.1.30",
            serialNo: "ABC-300",
            type: "Self-Service Module",
            isDeleted: true);
        var terminals = new List<Terminal>
        {
            active1,
            active2,
            deleted
        };
        var handler = new GetTerminalsQueryHandler(
            new InMemoryWriteReadRepository<Terminal>(terminals),
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetTerminalsQuery
            {
                Search = "Module",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.DoesNotContain(
            result.Value.Data,
            item => item.Id == deleted.Id);
        Assert.Equal(active1.Id, result.Value.Data[0].Id);
        Assert.Equal(active2.Id, result.Value.Data[1].Id);
        Assert.Equal(branch.Id, result.Value.Data[0].BranchId);
        Assert.Equal(branch.ArabicName, result.Value.Data[0].BranchArabicName);
        Assert.Equal(area.Id, result.Value.Data[0].WaitingAreaId);
        Assert.Equal(window1.Number, result.Value.Data[0].WindowNumber);
    }

    [Fact]
    public async Task Active_list_supports_window_filter_and_pagination()
    {
        var area = WaitingArea(1);
        var window1 = Window(1, area);
        var window2 = Window(2, area);
        var terminals = new List<Terminal>
        {
            Terminal(10, window1, number: "T-01"),
            Terminal(11, window1, number: "T-02"),
            Terminal(12, window2, number: "T-03")
        };
        var handler = new GetTerminalsQueryHandler(
            new InMemoryWriteReadRepository<Terminal>(terminals),
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetTerminalsQuery
            {
                WindowId = window1.Id,
                PageNumber = 2,
                PageSize = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.Single(result.Value.Data);
        Assert.Equal(11, result.Value.Data[0].Id);
    }

    [Fact]
    public async Task Details_returns_active_terminal_and_hides_missing_or_deleted()
    {
        var area = WaitingArea(1);
        var window = Window(1, area);
        var active = Terminal(10, window);
        var deleted = Terminal(11, window, number: "T-02", isDeleted: true);
        var terminals = new List<Terminal> { active, deleted };
        var handler = new GetTerminalByIdQueryHandler(
            new InMemoryWriteReadRepository<Terminal>(terminals),
            new TestCurrentUser());

        var activeResult = await handler.Handle(
            new GetTerminalByIdQuery { Id = active.Id },
            CancellationToken.None);
        var deletedResult = await handler.Handle(
            new GetTerminalByIdQuery { Id = deleted.Id },
            CancellationToken.None);
        var missingResult = await handler.Handle(
            new GetTerminalByIdQuery { Id = 99 },
            CancellationToken.None);

        Assert.True(activeResult.IsSuccess);
        Assert.Equal(active.Id, activeResult.Value.Id);
        Assert.True(deletedResult.IsFailure);
        Assert.True(missingResult.IsFailure);
        Assert.Contains(
            deletedResult.Errors,
            error => error.Code == "Terminals.Details.TerminalNotFound");
    }

    [Fact]
    public async Task Deleted_list_returns_deleted_only_with_deleted_timestamp()
    {
        var area = WaitingArea(1);
        var window = Window(1, area);
        var active = Terminal(10, window);
        var deleted = Terminal(
            11,
            window,
            number: "T-02",
            type: "Self-Service Module",
            isDeleted: true);
        var terminals = new List<Terminal> { active, deleted };
        var handler = new GetDeletedTerminalsQueryHandler(
            new InMemoryWriteReadRepository<Terminal>(terminals),
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetDeletedTerminalsQuery
            {
                Search = "Self",
                WindowId = window.Id,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Data);
        Assert.Equal(deleted.Id, result.Value.Data[0].Id);
        Assert.NotNull(result.Value.Data[0].DeletedOnUtc);
    }

    private static WaitingArea WaitingArea(int id, Branch? branch = null)
    {
        branch ??= EntityTestFactory.Branch(id);

        return EntityTestFactory.WaitingArea(
            id,
            branch.Id,
            id,
            branch,
            descriptiveName: $"Area {id}");
    }

    private static Window Window(
        int id,
        WaitingArea waitingArea,
        string? number = null)
        => EntityTestFactory.Window(
            id,
            waitingArea.Id,
            number ?? id.ToString(),
            waitingArea);

    private static Terminal Terminal(
        int id,
        Window window,
        string number = "T-01",
        string ipAddress = "192.168.1.20",
        string serialNo = "ABC-100",
        string type = "Operator Module",
        bool isDeleted = false)
        => EntityTestFactory.Terminal(
            id,
            window.Id,
            number,
            ipAddress,
            serialNo,
            type,
            window,
            isDeleted);
}
