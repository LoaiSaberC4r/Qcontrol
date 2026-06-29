using Qcontrol.Application.Features.Displays.Query.GetDisplayById;
using Qcontrol.Application.Features.Displays.Query.GetDisplays;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Displays;

public sealed class DisplayQueryHandlerTests
{
    [Fact]
    public async Task List_includes_active_and_inactive_filters_search_orders_and_projects_branch_context()
    {
        var branch1 = EntityTestFactory.Branch(
            1,
            arabicName: "Branch Arabic 1",
            englishName: "Branch 1");
        var branch2 = EntityTestFactory.Branch(
            2,
            arabicName: "Branch Arabic 2",
            englishName: "Branch 2");
        var active2 = Display(
            11,
            branch1,
            number: "D-02",
            ipAddress: "192.168.1.31",
            serialNo: "DISPLAY-SN-002",
            type: "LCD Display");
        var active1 = Display(
            10,
            branch1,
            number: "D-01",
            ipAddress: "192.168.1.30",
            serialNo: "DISPLAY-SN-001",
            type: "LED Display");
        var otherBranch = Display(
            12,
            branch2,
            number: "D-01",
            ipAddress: "192.168.1.30",
            serialNo: "DISPLAY-SN-001",
            type: "LED Display");
        var inactive = Display(
            13,
            branch1,
            number: "SCREEN-A",
            ipAddress: "192.168.1.40",
            serialNo: "DISPLAY-SN-003",
            type: "Queue Display",
            isInactive: true);
        var displays = new List<Display>
        {
            active2,
            active1,
            otherBranch,
            inactive
        };
        var handler = new GetDisplaysQueryHandler(
            new InMemoryWriteReadRepository<Display>(displays),
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetDisplaysQuery
            {
                BranchId = branch1.Id,
                Search = "Display",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalItems);
        Assert.Equal(active1.Id, result.Value.Data[0].Id);
        Assert.Equal(active2.Id, result.Value.Data[1].Id);
        Assert.Equal(inactive.Id, result.Value.Data[2].Id);
        Assert.All(
            result.Value.Data,
            item => Assert.Equal(branch1.Id, item.BranchId));
        Assert.Equal(branch1.ArabicName, result.Value.Data[0].BranchArabicName);
        Assert.Equal(branch1.EnglishName, result.Value.Data[0].BranchEnglishName);
    }

    [Fact]
    public async Task List_can_filter_inactive_status()
    {
        var branch = EntityTestFactory.Branch(1);
        var active = Display(10, branch);
        var inactive = Display(11, branch, number: "D-02", isInactive: true);
        var handler = new GetDisplaysQueryHandler(
            new InMemoryWriteReadRepository<Display>(
                new List<Display> { active, inactive }),
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetDisplaysQuery
            {
                IsActive = false,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Data);
        Assert.Equal(inactive.Id, result.Value.Data[0].Id);
        Assert.False(result.Value.Data[0].IsActive);
    }

    [Fact]
    public async Task Details_returns_active_or_inactive_display_and_hides_missing()
    {
        var branch = EntityTestFactory.Branch(1);
        var active = Display(10, branch);
        var inactive = Display(11, branch, number: "D-02", isInactive: true);
        var displays = new List<Display> { active, inactive };
        var handler = new GetDisplayByIdQueryHandler(
            new InMemoryWriteReadRepository<Display>(displays),
            new TestCurrentUser());

        var activeResult = await handler.Handle(
            new GetDisplayByIdQuery { Id = active.Id },
            CancellationToken.None);
        var inactiveResult = await handler.Handle(
            new GetDisplayByIdQuery { Id = inactive.Id },
            CancellationToken.None);
        var missingResult = await handler.Handle(
            new GetDisplayByIdQuery { Id = 99 },
            CancellationToken.None);

        Assert.True(activeResult.IsSuccess);
        Assert.True(inactiveResult.IsSuccess);
        Assert.Equal(active.Id, activeResult.Value.Id);
        Assert.False(inactiveResult.Value.IsActive);
        Assert.True(missingResult.IsFailure);
    }

    private static Display Display(
        int id,
        Branch branch,
        string number = "D-01",
        string ipAddress = "192.168.1.30",
        string serialNo = "DISPLAY-SN-001",
        string type = "LED Display",
        bool isInactive = false)
        => EntityTestFactory.Display(
            id,
            branch.Id,
            number,
            ipAddress,
            serialNo,
            type,
            branch,
            isInactive);
}
