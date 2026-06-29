using Qcontrol.Application.Features.Windows.Query.GetWindowById;
using Qcontrol.Application.Features.Windows.Query.GetWindows;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Windows;

public sealed class WindowQueryHandlerTests
{
    [Fact]
    public async Task List_returns_paginated_active_and_inactive_windows_with_projection()
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
                isInactive: true)
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
        Assert.Equal(4, result.Value.TotalItems);
        Assert.Equal(new[] { 11, 12, 13, 10 }, result.Value.Data.Select(x => x.Id));
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
    public async Task List_filters_by_waiting_area_status_and_searches_window_fields()
    {
        var area1 = WaitingArea(id: 1);
        var area2 = WaitingArea(id: 2);
        var inactive = EntityTestFactory.Window(
            10,
            area1.Id,
            "A-100",
            area1,
            descriptiveName: "Reception",
            ipAddress: "192.168.1.10",
            isInactive: true);
        var active = EntityTestFactory.Window(
            11,
            area1.Id,
            "B-200",
            area1,
            descriptiveName: "Billing",
            ipAddress: "192.168.1.20");
        var otherArea = EntityTestFactory.Window(
            12,
            area2.Id,
            "A-100",
            area2,
            descriptiveName: "Reception",
            ipAddress: "10.0.0.5");
        var handler = new GetWindowsQueryHandler(
            new InMemoryWriteReadRepository<Window>(
                new List<Window> { inactive, active, otherArea }),
            new TestCurrentUser());

        var byAreaInactive = await handler.Handle(
            new GetWindowsQuery
            {
                WaitingAreaId = area1.Id,
                IsActive = false,
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
        var byIp = await handler.Handle(
            new GetWindowsQuery
            {
                Search = "10.0.0.5",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(byAreaInactive.IsSuccess);
        Assert.Single(byAreaInactive.Value.Data);
        Assert.Equal(inactive.Id, byAreaInactive.Value.Data[0].Id);
        Assert.False(byAreaInactive.Value.Data[0].IsActive);
        Assert.Single(byNumber.Value.Data);
        Assert.Equal(active.Id, byNumber.Value.Data[0].Id);
        Assert.Single(byIp.Value.Data);
        Assert.Equal(otherArea.Id, byIp.Value.Data[0].Id);
    }

    [Fact]
    public async Task Details_returns_active_or_inactive_window_and_not_found_for_missing()
    {
        var area = WaitingArea();
        var active = EntityTestFactory.Window(
            10,
            area.Id,
            "1",
            area,
            descriptiveName: "Reception",
            ipAddress: "192.168.1.10");
        var inactive = EntityTestFactory.Window(
            11,
            area.Id,
            "2",
            area,
            isInactive: true);
        var handler = new GetWindowByIdQueryHandler(
            new InMemoryWriteReadRepository<Window>(
                new List<Window> { active, inactive }),
            new TestCurrentUser());

        var activeResult = await handler.Handle(
            new GetWindowByIdQuery { Id = active.Id },
            CancellationToken.None);
        var inactiveResult = await handler.Handle(
            new GetWindowByIdQuery { Id = inactive.Id },
            CancellationToken.None);
        var missingResult = await handler.Handle(
            new GetWindowByIdQuery { Id = 99 },
            CancellationToken.None);

        Assert.True(activeResult.IsSuccess);
        Assert.True(inactiveResult.IsSuccess);
        Assert.Equal(active.Id, activeResult.Value.Id);
        Assert.Equal(area.Number, activeResult.Value.WaitingAreaNumber);
        Assert.Equal("Reception", activeResult.Value.DescriptiveName);
        Assert.Equal("192.168.1.10", activeResult.Value.IPAddress);
        Assert.False(inactiveResult.Value.IsActive);
        Assert.True(missingResult.IsFailure);
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
