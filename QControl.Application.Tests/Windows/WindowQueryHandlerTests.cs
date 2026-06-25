using Qcontrol.Application.Features.Windows.Query.GetDeletedWindows;
using Qcontrol.Application.Features.Windows.Query.GetWindowById;
using Qcontrol.Application.Features.Windows.Query.GetWindows;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Windows;

public sealed class WindowQueryHandlerTests
{
    [Fact]
    public async Task Active_list_returns_paginated_active_windows_with_projection()
    {
        var area1 = WaitingArea(id: 1, number: 1);
        var area2 = WaitingArea(id: 2, number: 2);
        var windows = new List<Window>
        {
            EntityTestFactory.Window(
                12,
                area1.Id,
                "2",
                area1,
                descriptiveName: "Second",
                ipAddress: "192.168.1.2",
                enableTicketBooking: true),
            EntityTestFactory.Window(
                10,
                area2.Id,
                "1",
                area2,
                descriptiveName: "Other Area"),
            EntityTestFactory.Window(
                11,
                area1.Id,
                "1",
                area1,
                descriptiveName: "Reception",
                ipAddress: "192.168.1.1",
                enableDirectCall: true),
            EntityTestFactory.Window(
                13,
                area1.Id,
                "3",
                area1,
                isDeleted: true)
        };
        var repository = new InMemoryWriteReadRepository<Window>(windows);
        var handler = new GetWindowsQueryHandler(
            repository,
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetWindowsQuery
            {
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItems);
        Assert.Equal(new[] { 11, 12, 10 }, result.Value.Data.Select(x => x.Id));
        var first = result.Value.Data[0];
        Assert.Equal(area1.Id, first.WaitingAreaId);
        Assert.Equal(area1.Number, first.WaitingAreaNumber);
        Assert.Equal(area1.DescriptiveName, first.WaitingAreaDescriptiveName);
        Assert.Equal("1", first.Number);
        Assert.Equal("Reception", first.DescriptiveName);
        Assert.Equal("192.168.1.1", first.IPAddress);
        Assert.True(first.EnableDirectCall);
        Assert.Equal(1, repository.ListWithCountCallCount);
    }

    [Fact]
    public async Task Active_list_filters_by_waiting_area_and_searches_window_fields()
    {
        var area1 = WaitingArea(id: 1);
        var area2 = WaitingArea(id: 2);
        var windows = new List<Window>
        {
            EntityTestFactory.Window(
                10,
                area1.Id,
                "A-100",
                area1,
                descriptiveName: "Reception",
                ipAddress: "192.168.1.10"),
            EntityTestFactory.Window(
                11,
                area1.Id,
                "B-200",
                area1,
                descriptiveName: "Billing",
                ipAddress: "192.168.1.20"),
            EntityTestFactory.Window(
                12,
                area2.Id,
                "A-100",
                area2,
                descriptiveName: "Reception",
                ipAddress: "10.0.0.5")
        };
        var handler = new GetWindowsQueryHandler(
            new InMemoryWriteReadRepository<Window>(windows),
            new TestCurrentUser());

        var byArea = await handler.Handle(
            new GetWindowsQuery
            {
                WaitingAreaId = area1.Id,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);
        var byNumber = await handler.Handle(
            new GetWindowsQuery
            {
                Search = "B-200",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);
        var byName = await handler.Handle(
            new GetWindowsQuery
            {
                Search = "Reception",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);
        var byIp = await handler.Handle(
            new GetWindowsQuery
            {
                Search = "10.0.0.5",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(byArea.IsSuccess);
        Assert.Equal(2, byArea.Value.TotalItems);
        Assert.All(byArea.Value.Data, item => Assert.Equal(area1.Id, item.WaitingAreaId));
        Assert.Single(byNumber.Value.Data);
        Assert.Equal(11, byNumber.Value.Data[0].Id);
        Assert.Equal(2, byName.Value.TotalItems);
        Assert.Single(byIp.Value.Data);
        Assert.Equal(12, byIp.Value.Data[0].Id);
    }

    [Fact]
    public async Task Details_returns_active_window_and_not_found_for_missing_or_deleted()
    {
        var area = WaitingArea();
        var active = EntityTestFactory.Window(
            10,
            area.Id,
            "1",
            area,
            descriptiveName: "Reception",
            ipAddress: "192.168.1.10");
        var deleted = EntityTestFactory.Window(
            11,
            area.Id,
            "2",
            area,
            isDeleted: true);
        var handler = new GetWindowByIdQueryHandler(
            new InMemoryWriteReadRepository<Window>(
                new List<Window> { active, deleted }),
            new TestCurrentUser());

        var activeResult = await handler.Handle(
            new GetWindowByIdQuery { Id = active.Id },
            CancellationToken.None);
        var missingResult = await handler.Handle(
            new GetWindowByIdQuery { Id = 99 },
            CancellationToken.None);
        var deletedResult = await handler.Handle(
            new GetWindowByIdQuery { Id = deleted.Id },
            CancellationToken.None);

        Assert.True(activeResult.IsSuccess);
        Assert.Equal(active.Id, activeResult.Value.Id);
        Assert.Equal(area.Number, activeResult.Value.WaitingAreaNumber);
        Assert.Equal("Reception", activeResult.Value.DescriptiveName);
        Assert.Equal("192.168.1.10", activeResult.Value.IPAddress);
        Assert.True(missingResult.IsFailure);
        Assert.True(deletedResult.IsFailure);
        Assert.Contains(
            deletedResult.Errors,
            error => error.Code == "Windows.Details.WindowNotFound");
    }

    [Fact]
    public async Task Deleted_list_returns_only_deleted_windows_with_filters_search_and_deleted_date()
    {
        var area1 = WaitingArea(id: 1);
        var area2 = WaitingArea(id: 2);
        var deletedOnUtc = new DateTime(2026, 06, 25, 8, 0, 0, DateTimeKind.Utc);
        var windows = new List<Window>
        {
            EntityTestFactory.Window(
                10,
                area1.Id,
                "A-1",
                area1,
                descriptiveName: "Old Reception",
                ipAddress: "192.168.1.10",
                isDeleted: true,
                deletedOnUtc: deletedOnUtc),
            EntityTestFactory.Window(
                11,
                area1.Id,
                "A-2",
                area1),
            EntityTestFactory.Window(
                12,
                area2.Id,
                "B-1",
                area2,
                descriptiveName: "Old Billing",
                isDeleted: true)
        };
        var handler = new GetDeletedWindowsQueryHandler(
            new InMemoryWriteReadRepository<Window>(windows),
            new TestCurrentUser());

        var allDeleted = await handler.Handle(
            new GetDeletedWindowsQuery
            {
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);
        var filtered = await handler.Handle(
            new GetDeletedWindowsQuery
            {
                WaitingAreaId = area1.Id,
                Search = "192.168.1.10",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(allDeleted.IsSuccess);
        Assert.Equal(2, allDeleted.Value.TotalItems);
        Assert.DoesNotContain(allDeleted.Value.Data, item => item.Id == 11);
        Assert.Single(filtered.Value.Data);
        Assert.Equal(10, filtered.Value.Data[0].Id);
        Assert.Equal(deletedOnUtc, filtered.Value.Data[0].DeletedOnUtc);
    }

    [Fact]
    public async Task Deleted_list_supports_pagination_with_deterministic_ordering()
    {
        var area1 = WaitingArea(id: 1);
        var area2 = WaitingArea(id: 2);
        var windows = new List<Window>
        {
            EntityTestFactory.Window(10, area2.Id, "1", area2, isDeleted: true),
            EntityTestFactory.Window(11, area1.Id, "2", area1, isDeleted: true),
            EntityTestFactory.Window(12, area1.Id, "1", area1, isDeleted: true)
        };
        var handler = new GetDeletedWindowsQueryHandler(
            new InMemoryWriteReadRepository<Window>(windows),
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetDeletedWindowsQuery
            {
                PageNumber = 2,
                PageSize = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItems);
        Assert.Single(result.Value.Data);
        Assert.Equal(11, result.Value.Data[0].Id);
    }

    private static WaitingArea WaitingArea(
        int id = 1,
        int number = 1)
    {
        var branch = EntityTestFactory.Branch(id);
        return EntityTestFactory.WaitingArea(
            id,
            branch.Id,
            number,
            branch,
            descriptiveName: $"Area {id}");
    }
}
