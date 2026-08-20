using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Command.CreateBranchDisplayConfiguration;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Command.UpdateBranchDisplayConfiguration;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchDisplays;

public sealed class BranchDisplayConfigurationHandlerTests
{
    private static readonly byte[] RowVersion = [1, 2, 3, 4, 5, 6, 7, 8];

    [Fact]
    public async Task Create_rejects_a_second_configuration_for_the_branch()
    {
        var configurations = new List<BranchDisplayConfiguration>
        {
            EntityTestFactory.BranchDisplayConfiguration(10, 1)
        };
        var unitOfWork = new TestUnitOfWork();
        var handler = new CreateBranchDisplayConfigurationCommandHandler(
            new InMemoryWriteReadRepository<Branch>(new() { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<BranchDisplayConfiguration>(configurations),
            new InMemoryWriteRepository<BranchDisplayConfiguration>(configurations),
            new TestCurrentUser(),
            unitOfWork);

        var result = await handler.Handle(ValidCreate(), CancellationToken.None);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("BranchDisplayConfiguration.AlreadyExists", error.Code);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_maps_stale_row_version_to_conflict()
    {
        var configurations = new List<BranchDisplayConfiguration>
        {
            EntityTestFactory.BranchDisplayConfiguration(10, 1, RowVersion)
        };
        var tokenManager = new TestConcurrencyTokenManager();
        var handler = new UpdateBranchDisplayConfigurationCommandHandler(
            new InMemoryWriteReadRepository<Branch>(new() { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<BranchDisplayConfiguration>(configurations),
            new InMemoryWriteRepository<BranchDisplayConfiguration>(configurations),
            tokenManager,
            new TestCurrentUser(),
            new TestUnitOfWork { SaveChangesException = new DbUpdateConcurrencyException() });

        var result = await handler.Handle(ValidUpdate(), CancellationToken.None);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("BranchDisplayConfiguration.ConcurrencyConflict", error.Code);
        Assert.Equal(1, tokenManager.SetOriginalRowVersionCallCount);
    }

    private static CreateBranchDisplayConfigurationCommand ValidCreate() =>
        new()
        {
            BranchId = 1,
            DisplayBackgroundColor = "#FFFFFF",
            MainTitleAr = "العنوان",
            MainTitleEn = "Title",
            HeaderBackgroundColor = "#FFFFFF",
            MainTitleTextColor = "#158EA3",
            MainTitleFontSize = 28,
            TableHeaderBackgroundColor = "#158EA3",
            TableHeaderTextColor = "#FFFFFF",
            TableRowBackgroundColor = "#FFFFFF",
            TableRowTextColor = "#333333",
            TicketNumberBackgroundColor = "#168EA4",
            TicketNumberTextColor = "#FFFFFF",
            TicketColumnTitleAr = "التذكرة",
            TicketColumnTitleEn = "Ticket",
            ServiceColumnTitleAr = "الخدمة",
            ServiceColumnTitleEn = "Service",
            WindowColumnTitleAr = "الشباك",
            WindowColumnTitleEn = "Window",
            TickerBackgroundColor = "#FFFFFF",
            TickerTextColor = "#168EA4",
            TickerFontSize = 18,
            ShowClock = true,
            ClockBackgroundColor = "#147E92",
            ClockTextColor = "#FFFFFF"
        };

    private static UpdateBranchDisplayConfigurationCommand ValidUpdate() =>
        new()
        {
            BranchId = 1,
            DisplayBackgroundColor = "#FFFFFF",
            MainTitleAr = "العنوان",
            MainTitleEn = "Title",
            HeaderBackgroundColor = "#FFFFFF",
            MainTitleTextColor = "#158EA3",
            MainTitleFontSize = 28,
            TableHeaderBackgroundColor = "#158EA3",
            TableHeaderTextColor = "#FFFFFF",
            TableRowBackgroundColor = "#FFFFFF",
            TableRowTextColor = "#333333",
            TicketNumberBackgroundColor = "#168EA4",
            TicketNumberTextColor = "#FFFFFF",
            TicketColumnTitleAr = "التذكرة",
            TicketColumnTitleEn = "Ticket",
            ServiceColumnTitleAr = "الخدمة",
            ServiceColumnTitleEn = "Service",
            WindowColumnTitleAr = "الشباك",
            WindowColumnTitleEn = "Window",
            TickerBackgroundColor = "#FFFFFF",
            TickerTextColor = "#168EA4",
            TickerFontSize = 18,
            ShowClock = true,
            ClockBackgroundColor = "#147E92",
            ClockTextColor = "#FFFFFF",
            RowVersion = Convert.ToBase64String(RowVersion)
        };
}
