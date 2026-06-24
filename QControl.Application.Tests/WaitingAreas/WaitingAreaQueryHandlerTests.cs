using Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreaById;
using Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.WaitingAreas;

public sealed class WaitingAreaQueryHandlerTests
{
    [Fact]
    public async Task GetById_returns_projected_data()
    {
        var branch = EntityTestFactory.Branch(
            1,
            arabicName: "Cairo Arabic Branch",
            englishName: "Cairo Branch");
        var waitingArea = EntityTestFactory.WaitingArea(
            id: 10,
            branchId: 1,
            number: 3,
            branch: branch,
            audioDevice: "speaker",
            controlDevice: "tablet",
            descriptiveName: "Main Area");
        var repository =
            new InMemoryWriteReadRepository<WaitingArea>(
                new List<WaitingArea> { waitingArea });
        var handler = new GetWaitingAreaByIdQueryHandler(
            repository,
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetWaitingAreaByIdQuery { Id = 10 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Id);
        Assert.Equal(1, result.Value.BranchId);
        Assert.Equal("Cairo Arabic Branch", result.Value.BranchArabicName);
        Assert.Equal("Cairo Branch", result.Value.BranchEnglishName);
        Assert.Equal(3, result.Value.Number);
        Assert.Equal("speaker", result.Value.AudioDevice);
        Assert.Equal("tablet", result.Value.ControlDevice);
        Assert.Equal("Main Area", result.Value.DescriptiveName);
    }

    [Fact]
    public async Task GetById_returns_not_found()
    {
        var repository =
            new InMemoryWriteReadRepository<WaitingArea>(
                new List<WaitingArea>());
        var handler = new GetWaitingAreaByIdQueryHandler(
            repository,
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetWaitingAreaByIdQuery { Id = 10 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error =>
                error.Code ==
                "WaitingAreas.Details.WaitingAreaNotFound");
    }

    [Fact]
    public async Task List_filters_by_branch_id()
    {
        var branch1 = EntityTestFactory.Branch(1);
        var branch2 = EntityTestFactory.Branch(2);
        var repository = RepositoryWith(
            EntityTestFactory.WaitingArea(10, 1, 1, branch1),
            EntityTestFactory.WaitingArea(11, 2, 1, branch2),
            EntityTestFactory.WaitingArea(12, 1, 2, branch1));
        var handler = new GetWaitingAreasQueryHandler(
            repository,
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetWaitingAreasQuery
            {
                BranchId = 1,
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.All(
            result.Value.Data,
            item => Assert.Equal(1, item.BranchId));
    }

    [Fact]
    public async Task List_applies_search_to_text_fields_and_number()
    {
        var branch = EntityTestFactory.Branch(1);
        var repository = RepositoryWith(
            EntityTestFactory.WaitingArea(
                10,
                1,
                1,
                branch,
                descriptiveName: "Reception"),
            EntityTestFactory.WaitingArea(
                11,
                1,
                27,
                branch,
                audioDevice: "Hall Speaker"),
            EntityTestFactory.WaitingArea(
                12,
                1,
                3,
                branch,
                controlDevice: "Desk Tablet"));
        var handler = new GetWaitingAreasQueryHandler(
            repository,
            new TestCurrentUser());

        var textResult = await handler.Handle(
            new GetWaitingAreasQuery
            {
                Search = "Speaker",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        var numberResult = await handler.Handle(
            new GetWaitingAreasQuery
            {
                Search = "27",
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(textResult.IsSuccess);
        Assert.Single(textResult.Value.Data);
        Assert.Equal(11, textResult.Value.Data[0].Id);

        Assert.True(numberResult.IsSuccess);
        Assert.Single(numberResult.Value.Data);
        Assert.Equal(27, numberResult.Value.Data[0].Number);
    }

    [Fact]
    public async Task List_applies_pagination_with_deterministic_ordering()
    {
        var branch1 = EntityTestFactory.Branch(1);
        var branch2 = EntityTestFactory.Branch(2);
        var repository = RepositoryWith(
            EntityTestFactory.WaitingArea(10, 2, 1, branch2),
            EntityTestFactory.WaitingArea(11, 1, 2, branch1),
            EntityTestFactory.WaitingArea(12, 1, 1, branch1));
        var handler = new GetWaitingAreasQueryHandler(
            repository,
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetWaitingAreasQuery
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

    [Fact]
    public async Task List_returns_correct_windows_count()
    {
        var branch = EntityTestFactory.Branch(1);
        var waitingArea =
            EntityTestFactory.WaitingArea(10, 1, 1, branch);
        EntityTestFactory.Window(100, 10, "1", waitingArea);
        EntityTestFactory.Window(101, 10, "2", waitingArea);

        var repository = RepositoryWith(waitingArea);
        var handler = new GetWaitingAreasQueryHandler(
            repository,
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetWaitingAreasQuery
            {
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Data);
        Assert.Equal(2, result.Value.Data[0].WindowsCount);
    }

    [Fact]
    public async Task List_uses_single_repository_query_for_page_and_count()
    {
        var branch = EntityTestFactory.Branch(1);
        var repository = RepositoryWith(
            EntityTestFactory.WaitingArea(10, 1, 1, branch),
            EntityTestFactory.WaitingArea(11, 1, 2, branch),
            EntityTestFactory.WaitingArea(12, 1, 3, branch));
        var handler = new GetWaitingAreasQueryHandler(
            repository,
            new TestCurrentUser());

        var result = await handler.Handle(
            new GetWaitingAreasQuery
            {
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, repository.ListWithCountCallCount);
        Assert.Equal(0, repository.AnyCallCount);
    }

    private static InMemoryWriteReadRepository<WaitingArea> RepositoryWith(
        params WaitingArea[] waitingAreas)
        => new(waitingAreas.ToList());
}
