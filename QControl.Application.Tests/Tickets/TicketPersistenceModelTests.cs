using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.Tickets;

public sealed class TicketPersistenceModelTests
{
    [Fact]
    public void Runtime_aggregates_have_required_concurrency_and_uniqueness_constraints()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        AssertRowVersion(model.FindEntityType(typeof(Ticket))!, nameof(Ticket.RowVersion));
        AssertRowVersion(model.FindEntityType(typeof(Reservation))!, nameof(Reservation.RowVersion));
        AssertRowVersion(model.FindEntityType(typeof(TicketNumberSequence))!, nameof(TicketNumberSequence.RowVersion));
        AssertRowVersion(model.FindEntityType(typeof(BranchServiceSegmentDailyUsage))!, nameof(BranchServiceSegmentDailyUsage.RowVersion));

        var ticket = model.FindEntityType(typeof(Ticket))!;
        var reservationIndex = ticket.GetIndexes().Single(x => x.GetDatabaseName() == "UX_Ticket_ReservationId_NotNull");
        Assert.True(reservationIndex.IsUnique);
        Assert.Equal("[ReservationId] IS NOT NULL", reservationIndex.GetFilter());
        Assert.Contains(ticket.GetIndexes(), x => x.GetDatabaseName() == "UX_Ticket_Branch_BusinessDate_IssuingService_Number" && x.IsUnique);

        var attempts = model.FindEntityType(typeof(TicketCallAttempt))!;
        Assert.Contains(attempts.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name)
            .SequenceEqual(new[] { "TicketId", "CallCycleNumber", "AttemptNumber" }));
        var steps = model.FindEntityType(typeof(TicketWorkflowStepSnapshot))!;
        Assert.Contains(steps.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name)
            .SequenceEqual(new[] { "TicketId", "StepOrder" }));

        AssertLookupValue(model.FindEntityType(typeof(Ticket))!);
        AssertLookupValue(model.FindEntityType(typeof(Reservation))!);
        AssertLookupValue(model.GetEntityTypes().Single(x => x.ClrType.Name == "TicketArchive"));
        AssertLookupValue(model.GetEntityTypes().Single(x => x.ClrType.Name == "ReservationArchive"));

        var reservationInputs = model.FindEntityType(typeof(ReservationCustomInputValue))!;
        Assert.Contains(reservationInputs.GetIndexes(), x =>
            x.GetDatabaseName() == "IX_ReservationCustomInputValue_Reservation_Input" &&
            x.Properties.Select(p => p.Name).SequenceEqual(
                new[] { "ReservationId", "ServiceCustomInputId" }));
    }

    private static void AssertRowVersion(IEntityType entity, string propertyName)
    {
        var property = entity.FindProperty(propertyName)!;
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        Assert.Equal("rowversion", property.GetColumnType());
    }

    private static void AssertLookupValue(IEntityType entity)
    {
        var property = entity.FindProperty("LookupValue");
        Assert.NotNull(property);
        Assert.True(property!.IsNullable);
        Assert.Equal(3000, property.GetMaxLength());
        Assert.DoesNotContain(entity.GetIndexes(), index =>
            index.Properties.Any(x => x.Name == "LookupValue"));
    }

    private static PlatformWriteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QControlTicketModelTests;Trusted_Connection=True;").Options;
        return new PlatformWriteDbContext(options, new TestTenantContext(), new TestCurrentBranchContext());
    }
}
