using Microsoft.EntityFrameworkCore;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Tickets;

public sealed partial class KioskTicketRuntimeRepositoryTests
{
    [Theory]
    [InlineData("label")]
    [InlineData("name")]
    [InlineData("constraints")]
    [InlineData("deactivated")]
    [InlineData("replaced")]
    public async Task Historical_lookup_and_conversion_preserve_original_snapshots_after_input_edits(
        string change)
    {
        await using var db = CreateContext();
        var service = await SeedServiceGraphAsync(db, 10);
        var definition = EntityTestFactory.ServiceCustomInput(service, 101, "OriginalName",
            isRequired: true, minLength: 3, labelEn: "Original label", labelAr: "الاسم الأصلي");
        db.Add(definition);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);
        var original = await repository.CreateReservationAsync(1, 10, 3, Now.AddHours(1),
            null, new[] { new CustomInputSubmission(101, "old-value") },
            Performer, CancellationToken.None);
        Assert.True(original.IsSuccess, FormatErrors(original.Errors));
        db.ChangeTracker.Clear();
        service = await db.Set<Service>().Include(x => x.CustomInputs).SingleAsync();
        definition = service.CustomInputs.Single();
        if (change is "deactivated" or "replaced")
            service.DeactivateCustomInput(definition, Performer);
        else
            service.UpdateCustomInput(definition,
                change == "name" ? "RenamedInput" : definition.Name,
                change == "label" ? "New label" : definition.LabelEn,
                definition.LabelAr,
                change == "constraints" ? ServiceCustomInputType.Integer : definition.Type,
                true, null, null, null, null, null, 1, Performer);
        if (change == "replaced")
            db.Add(EntityTestFactory.ServiceCustomInput(service, 102, "Replacement",
                isRequired: true));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var search = await repository.SearchReservationsForKioskAsync(1, 10, null,
            new[] { new CustomInputSubmission(101, "old-value") }, CancellationToken.None);
        var wrongValue = await repository.SearchReservationsForKioskAsync(1, 10, null,
            new[] { new CustomInputSubmission(101, "wrong") }, CancellationToken.None);
        var conversion = await repository.CreateTicketFromReservationAsync(1,
            original.Value.Id, Performer, CancellationToken.None);

        Assert.True(search.IsSuccess, FormatErrors(search.Errors));
        Assert.Equal(original.Value.Id, Assert.Single(search.Value).ReservationId);
        Assert.True(wrongValue.IsSuccess);
        Assert.Empty(wrongValue.Value);
        Assert.True(conversion.IsSuccess, FormatErrors(conversion.Errors));
        var snapshot = Assert.Single(conversion.Value.CustomInputs);
        Assert.Equal(101, snapshot.ServiceCustomInputId);
        Assert.Equal("OriginalName", snapshot.Name);
        Assert.Equal("Original label", snapshot.LabelEn);
        Assert.Equal("الاسم الأصلي", snapshot.LabelAr);
        Assert.Equal(ServiceCustomInputType.String, snapshot.Type);
        Assert.Equal("old-value", snapshot.Value);
        Assert.Equal(1, snapshot.OrderSnapshot);
        Assert.Equal(3, conversion.Value.SegmentId);
        Assert.Null(conversion.Value.Field);
        db.ChangeTracker.Clear();
        var storedSnapshot = await db.Set<ReservationCustomInputValue>().SingleAsync();
        Assert.Equal(101, storedSnapshot.ServiceCustomInputId);
        Assert.Equal(101, storedSnapshot.OriginalServiceCustomInputId);
        Assert.Equal("OriginalName", storedSnapshot.NameSnapshot);
        Assert.Equal("old-value", storedSnapshot.Value);
        Assert.Equal(1, (await db.Set<BranchServiceSegmentDailyUsage>().SingleAsync()).ConsumedCount);

        if (change == "replaced")
        {
            var staleCreation = await repository.CreateReservationAsync(1, 10, 3,
                Now.AddHours(2), null, new[] { new CustomInputSubmission(101, "old-value") },
                Performer, CancellationToken.None);
            var newCreation = await repository.CreateReservationAsync(1, 10, 3,
                Now.AddHours(2), null, new[] { new CustomInputSubmission(102, "new-value") },
                Performer, CancellationToken.None);
            Assert.True(staleCreation.IsFailure);
            Assert.True(newCreation.IsSuccess, FormatErrors(newCreation.Errors));
            Assert.Equal("Replacement", Assert.Single(newCreation.Value.CustomInputs).Name);
            var currentSearch = await repository.SearchReservationsForKioskAsync(1, 10, null,
                new[] { new CustomInputSubmission(102, "new-value") }, CancellationToken.None);
            Assert.Equal(newCreation.Value.Id, Assert.Single(currentSearch.Value).ReservationId);
            var mismatchedIdentity = await repository.SearchReservationsForKioskAsync(1, 10, null,
                new[] { new CustomInputSubmission(102, "old-value") }, CancellationToken.None);
            Assert.Empty(mismatchedIdentity.Value);
        }
        if (change == "constraints")
        {
            var invalidNew = await repository.CreateReservationAsync(1, 10, 3,
                Now.AddHours(2), null, new[] { new CustomInputSubmission(101, "old-value") },
                Performer, CancellationToken.None);
            Assert.Equal("TicketRuntime.CustomInputs.Invalid", invalidNew.Errors[0].Code);
        }
    }

    [Fact]
    public async Task Generic_field_reservations_remain_searchable_after_custom_inputs_are_added()
    {
        await using var db = CreateContext();
        var service = await SeedServiceGraphAsync(db, 10);
        var oldReservation = await AddReservationAsync(db, 1, 10, 3, 1000,
            Now.AddHours(1), "REF-42");
        service = await db.Set<Service>().SingleAsync();
        db.Add(EntityTestFactory.ServiceCustomInput(service, 101, "Phone", isRequired: true));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);

        var search = await repository.SearchReservationsForKioskAsync(1, 10, " REF-42 ",
            Array.Empty<CustomInputSubmission>(), CancellationToken.None);
        var newBooking = await repository.CreateReservationAsync(1, 10, 3, Now.AddHours(2),
            "REF-42", Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);

        Assert.Equal(oldReservation.Id, Assert.Single(search.Value).ReservationId);
        Assert.Equal("Reservations.Create.FieldNotAllowed", newBooking.Errors[0].Code);
    }

    [Fact]
    public async Task Historical_search_keeps_exact_AND_matching_branch_service_scope_and_input_validation()
    {
        await using var db = CreateContext();
        var service = await SeedServiceGraphAsync(db, 10);
        var phone = EntityTestFactory.ServiceCustomInput(service, 101, "Phone", isActive: false);
        var reference = EntityTestFactory.ServiceCustomInput(service, 102, "Reference", isActive: false);
        var otherBranch = EntityTestFactory.ServiceCustomInput(service, 103, "Branch", isActive: false);
        var otherService = EntityTestFactory.ServiceCustomInput(service, 104, "Service", isActive: false);
        db.AddRange(phone, reference, otherBranch, otherService);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var target = await AddReservationAsync(db, 1, 10, 3, 1000, Now.AddHours(1),
            (phone, "01012345678"), (reference, "REF-42"));
        await AddReservationAsync(db, 1, 10, 3, 1000, Now.AddHours(2),
            (phone, "01012345678"), (reference, "other"));
        await AddReservationAsync(db, 2, 10, 3, 1000, Now.AddHours(3),
            (phone, "01012345678"), (reference, "REF-42"), (otherBranch, "branch"));
        await AddReservationAsync(db, 1, 20, 3, 1000, Now.AddHours(4),
            (phone, "01012345678"), (reference, "REF-42"), (otherService, "service"));
        await db.Set<ReservationCustomInputValue>().ForEachAsync(snapshot =>
            db.Entry(snapshot).Property(x => x.ServiceCustomInputId).CurrentValue = null);
        await db.SaveChangesAsync();
        var repository = CreateRepository(db);
        var inputs = new[] { new CustomInputSubmission(101, "01012345678"),
            new CustomInputSubmission(102, "REF-42") };

        var result = await repository.SearchReservationsForKioskAsync(1, 10, null, inputs, CancellationToken.None);
        var substring = await repository.SearchReservationsForKioskAsync(1, 10, null,
            new[] { new CustomInputSubmission(101, "0101234567") }, CancellationToken.None);
        var partial = await repository.SearchReservationsForKioskAsync(1, 10, null,
            new[] { inputs[0] }, CancellationToken.None);
        var empty = await repository.SearchReservationsForKioskAsync(1, 10, null,
            Array.Empty<CustomInputSubmission>(), CancellationToken.None);
        var blank = await repository.SearchReservationsForKioskAsync(1, 10, null,
            new[] { new CustomInputSubmission(101, " ") }, CancellationToken.None);
        var duplicate = await repository.SearchReservationsForKioskAsync(1, 10, null,
            new[] { inputs[0], inputs[0] }, CancellationToken.None);
        var foreign = await repository.SearchReservationsForKioskAsync(1, 10, null,
            new[] { new CustomInputSubmission(999, "value") }, CancellationToken.None);

        Assert.Equal(target.Id, Assert.Single(result.Value).ReservationId);
        Assert.Empty(substring.Value);
        Assert.Equal(2, partial.Value.Count);
        foreach (var foreignInput in new CustomInputSubmission[] { new(103, "branch"), new(104, "service") })
        {
            var outsideScope = await repository.SearchReservationsForKioskAsync(
                1, 10, null, [foreignInput], CancellationToken.None);
            Assert.Equal("TicketRuntime.CustomInputs.NotApplicable", outsideScope.Errors[0].Code);
        }
        Assert.Equal("Kiosk.Reservations.Search.FieldRequired", empty.Errors[0].Code);
        Assert.Equal("Kiosk.Reservations.Search.CustomInputsInvalid", blank.Errors[0].Code);
        Assert.Equal("TicketRuntime.CustomInputs.Duplicate", duplicate.Errors[0].Code);
        Assert.Equal("TicketRuntime.CustomInputs.NotApplicable", foreign.Errors[0].Code);
    }

    [Fact]
    public async Task Snapshot_without_retained_input_id_is_preserved_and_never_guessed_by_value()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, 10);
        var reservation = Reservation.Create(1, 10, 3, 1000, Now.AddHours(1),
            DateOnly.FromDateTime(Now), Now, Performer);
        var legacySnapshot = ReservationCustomInputValue.Create(101, 101,
            "Deleted input", "Original label", null, ServiceCustomInputType.String, "secret", Now);
        reservation.AddCustomInput(legacySnapshot);
        db.Add(reservation);
        db.Entry(legacySnapshot).Property(x => x.ServiceCustomInputId).CurrentValue = null;
        db.Entry(legacySnapshot).Property(x => x.OriginalServiceCustomInputId).CurrentValue = null;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);

        var byOldId = await repository.SearchReservationsForKioskAsync(1, 10, null,
            new[] { new CustomInputSubmission(101, "secret") }, CancellationToken.None);
        var byField = await repository.SearchReservationsForKioskAsync(1, 10, "secret",
            Array.Empty<CustomInputSubmission>(), CancellationToken.None);
        var converted = await repository.CreateTicketFromReservationAsync(1, reservation.Id,
            Performer, CancellationToken.None);

        Assert.Equal("TicketRuntime.CustomInputs.NotApplicable", byOldId.Errors[0].Code);
        Assert.Empty(byField.Value);
        Assert.True(converted.IsSuccess, FormatErrors(converted.Errors));
        var snapshot = Assert.Single(converted.Value.CustomInputs);
        Assert.Null(snapshot.ServiceCustomInputId);
        Assert.Equal("Deleted input", snapshot.Name);
        Assert.Equal("secret", snapshot.Value);
        var convertedSnapshot = await db.Set<TicketCustomInputValue>().AsNoTracking().SingleAsync();
        Assert.Null(convertedSnapshot.OriginalServiceCustomInputId);
        Assert.Equal("Original label", convertedSnapshot.LabelEnSnapshot);
    }
}
