using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.TicketConfigurations;

public sealed class TicketPrintConfigurationPersistenceTests
{
    [Fact]
    public void Model_enforces_branch_uniqueness_element_uniqueness_and_rowversion()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var root = model.FindEntityType(typeof(TicketPrintConfiguration))!;
        var element = model.FindEntityType(typeof(TicketPrintElement))!;

        Assert.Equal("TicketPrintConfiguration", root.GetTableName());
        Assert.Equal("decimal(9,2)", root.FindProperty(nameof(TicketPrintConfiguration.TicketWidthMm))!.GetColumnType());
        Assert.Equal("decimal(9,2)", root.FindProperty(nameof(TicketPrintConfiguration.TicketHeightMm))!.GetColumnType());
        var rowVersion = root.FindProperty(nameof(TicketPrintConfiguration.RowVersion))!;
        Assert.True(rowVersion.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);
        Assert.True(root.GetIndexes().Single(x =>
            x.GetDatabaseName() == "UX_TicketPrintConfiguration_BranchId").IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, root.GetForeignKeys().Single().DeleteBehavior);
        Assert.NotNull(root.GetQueryFilter());

        Assert.Equal("TicketPrintElement", element.GetTableName());
        Assert.Equal("decimal(9,2)", element.FindProperty(nameof(TicketPrintElement.XMm))!.GetColumnType());
        Assert.Equal("decimal(6,2)", element.FindProperty(nameof(TicketPrintElement.FontSizePt))!.GetColumnType());
        Assert.True(element.GetIndexes().Single(x =>
            x.GetDatabaseName() == "UX_TicketPrintElement_Configuration_ElementType").IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, element.GetForeignKeys().Single().DeleteBehavior);
        Assert.NotNull(element.GetQueryFilter());

        Assert.NotNull(model.FindEntityType(typeof(TicketCustomInputValue))!
            .FindProperty(nameof(TicketCustomInputValue.OrderSnapshot)));
        Assert.NotNull(model.FindEntityType(typeof(ReservationCustomInputValue))!
            .FindProperty(nameof(ReservationCustomInputValue.OrderSnapshot)));
    }

    private static PlatformWriteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QControlTicketPrintModelTests;Trusted_Connection=True;")
            .Options;
        return new PlatformWriteDbContext(options, new TestTenantContext(),
            new TestCurrentBranchContext());
    }
}
