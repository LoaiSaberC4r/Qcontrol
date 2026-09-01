using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using QControl.Application.Abstraction.Services;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;
using QControl.infrastructure.Repositories;

namespace QControl.Application.Tests.Tickets;

public sealed class KioskTicketRuntimeRepositoryTests
{
    private static readonly Guid Performer = EntityTestFactory.CurrentUserId;
    private static readonly DateTime Now =
        new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Reservations_without_custom_inputs_require_field_and_allow_duplicates()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);

        var missing = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(1), null,
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        var customInputsNotAllowed = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(1), "customer-42",
            new[] { new CustomInputSubmission(999, "value") },
            Performer, CancellationToken.None);
        var first = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(1), " customer-42 ",
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        var second = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(2), "customer-42",
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);

        Assert.True(missing.IsFailure);
        Assert.Equal("Reservations.Create.FieldRequired", missing.Errors[0].Code);
        Assert.Equal("Reservations.Create.CustomInputsNotAllowed",
            customInputsNotAllowed.Errors[0].Code);
        Assert.True(first.IsSuccess, FormatErrors(first.Errors));
        Assert.True(second.IsSuccess, FormatErrors(second.Errors));
        Assert.NotEqual(first.Value.Id, second.Value.Id);
        Assert.Equal(2, await db.Set<Reservation>()
            .CountAsync(x => x.LookupValue == "customer-42"));
    }

    [Fact]
    public async Task Reservations_with_custom_inputs_keep_full_creation_validation_and_allow_duplicates()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        db.ChangeTracker.Clear();
        var service = await db.Set<Service>().SingleAsync(x => x.Id == 10);
        var phone = EntityTestFactory.ServiceCustomInput(
            service, 101, "Phone", isRequired: true,
            minLength: 11, maxLength: 11, startWith: "010");
        db.Add(phone);
        db.Entry(service).State = EntityState.Unchanged;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);
        var input = new[] { new CustomInputSubmission(phone.Id, "01012345678") };

        var missing = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(1), null,
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        var fieldAndInput = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(1), "not-allowed", input,
            Performer, CancellationToken.None);
        var first = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(1), null, input,
            Performer, CancellationToken.None);
        var second = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(2), null, input,
            Performer, CancellationToken.None);

        Assert.True(missing.IsFailure);
        Assert.True(fieldAndInput.IsFailure);
        Assert.Equal("Reservations.Create.FieldNotAllowed", fieldAndInput.Errors[0].Code);
        Assert.True(first.IsSuccess, FormatErrors(first.Errors));
        Assert.True(second.IsSuccess, FormatErrors(second.Errors));
        Assert.Equal(2, await db.Set<ReservationCustomInputValue>()
            .CountAsync(x => x.ServiceCustomInputId == phone.Id &&
                x.Value == "01012345678"));
    }

    [Fact]
    public async Task Search_with_one_or_multiple_custom_inputs_uses_branch_service_and_and_semantics()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        db.ChangeTracker.Clear();
        var service = await db.Set<Service>().SingleAsync(x => x.Id == 10);
        var phone = EntityTestFactory.ServiceCustomInput(service, 101, "Phone");
        var nationalId = EntityTestFactory.ServiceCustomInput(
            service, 102, "NationalId", order: 2);
        db.AddRange(phone, nationalId);
        db.Entry(service).State = EntityState.Unchanged;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await AddReservationAsync(db, 1, 10, 3, 1000, Now.AddHours(1),
            (phone, "01012345678"), (nationalId, "29801011234567"));
        await AddReservationAsync(db, 1, 10, 3, 1000, Now.AddHours(2),
            (phone, "01012345678"), (nationalId, "29902021234567"));
        await AddReservationAsync(db, 2, 10, 3, 1000, Now.AddHours(3),
            (phone, "01012345678"), (nationalId, "29801011234567"));
        await AddReservationAsync(db, 1, 20, 3, 1000, Now.AddHours(4),
            (phone, "01012345678"), (nationalId, "29801011234567"));
        var repository = CreateRepository(db);

        var byPhone = await repository.SearchReservationsForKioskAsync(
            1, 10, null,
            new[] { new CustomInputSubmission(phone.Id, "01012345678") },
            CancellationToken.None);
        var byBoth = await repository.SearchReservationsForKioskAsync(
            1, 10, null,
            new[]
            {
                new CustomInputSubmission(phone.Id, "01012345678"),
                new CustomInputSubmission(nationalId.Id, "29801011234567")
            }, CancellationToken.None);

        Assert.True(byPhone.IsSuccess);
        Assert.Equal(2, byPhone.Value.Count);
        Assert.True(byBoth.IsSuccess);
        Assert.Single(byBoth.Value);
        Assert.Equal(Now.AddHours(1), byBoth.Value[0].ScheduledOnUtc);
    }

    [Fact]
    public async Task Custom_input_search_rejects_empty_duplicate_and_foreign_inputs()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        db.ChangeTracker.Clear();
        var service = await db.Set<Service>().SingleAsync(x => x.Id == 10);
        var phone = EntityTestFactory.ServiceCustomInput(service, 101, "Phone");
        db.Add(phone);
        db.Entry(service).State = EntityState.Unchanged;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);

        var empty = await repository.SearchReservationsForKioskAsync(
            1, 10, null, Array.Empty<CustomInputSubmission>(), CancellationToken.None);
        var duplicate = await repository.SearchReservationsForKioskAsync(
            1, 10, null,
            new[]
            {
                new CustomInputSubmission(phone.Id, "one"),
                new CustomInputSubmission(phone.Id, "two")
            }, CancellationToken.None);
        var foreign = await repository.SearchReservationsForKioskAsync(
            1, 10, null,
            new[] { new CustomInputSubmission(999, "value") },
            CancellationToken.None);

        Assert.Equal("Kiosk.Reservations.Search.CustomInputsRequired", empty.Errors[0].Code);
        Assert.Equal("TicketRuntime.CustomInputs.Duplicate", duplicate.Errors[0].Code);
        Assert.Equal("TicketRuntime.CustomInputs.NotApplicable", foreign.Errors[0].Code);
    }

    [Fact]
    public async Task Field_search_returns_all_exact_matches_only_in_requested_branch_and_service()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        db.ChangeTracker.Clear();
        await AddReservationAsync(db, 1, 10, 3, 1000, Now.AddHours(1), "REF-42");
        await AddReservationAsync(db, 1, 10, 3, 1000, Now.AddHours(2), "REF-42");
        await AddReservationAsync(db, 2, 10, 3, 1000, Now.AddHours(3), "REF-42");
        await AddReservationAsync(db, 1, 20, 3, 1000, Now.AddHours(4), "REF-42");
        await AddReservationAsync(db, 1, 10, 3, 1000, Now.AddHours(5), "REF-420");
        var repository = CreateRepository(db);

        var result = await repository.SearchReservationsForKioskAsync(
            1, 10, "REF-42", Array.Empty<CustomInputSubmission>(),
            CancellationToken.None);
        var missing = await repository.SearchReservationsForKioskAsync(
            1, 10, null, Array.Empty<CustomInputSubmission>(),
            CancellationToken.None);
        var customInputsNotAllowed = await repository.SearchReservationsForKioskAsync(
            1, 10, "REF-42",
            new[] { new CustomInputSubmission(999, "value") },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Kiosk.Reservations.Search.FieldRequired", missing.Errors[0].Code);
        Assert.Equal("Kiosk.Reservations.Search.CustomInputsNotAllowed",
            customInputsNotAllowed.Errors[0].Code);
    }

    [Fact]
    public async Task Reservation_conversion_copies_lookup_value_to_ticket()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);
        var reservation = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(1), "REF-42",
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);

        var ticket = await repository.CreateTicketFromReservationAsync(
            1, reservation.Value.Id, Performer, CancellationToken.None);

        Assert.True(ticket.IsSuccess, FormatErrors(ticket.Errors));
        Assert.Equal("REF-42", ticket.Value.Field);
        Assert.Equal("REF-42", (await db.Set<Ticket>().SingleAsync()).LookupValue);
    }

    [Fact]
    public async Task Direct_kiosk_ticket_creates_ticket_without_reservation()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);

        var missing = await repository.CreateTicketAsync(
            1, 10, 3, null, Array.Empty<CustomInputSubmission>(),
            requireLookupValueWhenNoCustomInputs: true, Performer,
            CancellationToken.None);
        var result = await repository.CreateTicketAsync(
            1, 10, 3, "REF-42", Array.Empty<CustomInputSubmission>(),
            requireLookupValueWhenNoCustomInputs: true, Performer,
            CancellationToken.None);

        Assert.Equal("Tickets.Create.FieldRequired", missing.Errors[0].Code);
        Assert.True(result.IsSuccess, FormatErrors(result.Errors));
        Assert.Equal("REF-42", result.Value.Field);
        Assert.Equal(1, await db.Set<Ticket>().CountAsync());
        Assert.Equal(0, await db.Set<Reservation>().CountAsync());
        Assert.Equal(0, await db.Set<ReservationHistory>().CountAsync());
        Assert.Equal(0, await db.Set<ReservationCustomInputValue>().CountAsync());
    }

    [Fact]
    public async Task Direct_kiosk_ticket_with_custom_inputs_applies_full_validation()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        db.ChangeTracker.Clear();
        var service = await db.Set<Service>().SingleAsync(x => x.Id == 10);
        var phone = EntityTestFactory.ServiceCustomInput(
            service, 101, "Phone", isRequired: true, minLength: 11,
            maxLength: 11, startWith: "010");
        db.Add(phone);
        db.Entry(service).State = EntityState.Unchanged;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);
        var input = new[] { new CustomInputSubmission(phone.Id, "01012345678") };

        var missing = await repository.CreateTicketAsync(
            1, 10, 3, null, Array.Empty<CustomInputSubmission>(), true,
            Performer, CancellationToken.None);
        var fieldNotAllowed = await repository.CreateTicketAsync(
            1, 10, 3, "REF-42", input, true,
            Performer, CancellationToken.None);
        var success = await repository.CreateTicketAsync(
            1, 10, 3, null, input, true,
            Performer, CancellationToken.None);

        Assert.True(missing.IsFailure);
        Assert.Equal("Tickets.Create.FieldNotAllowed", fieldNotAllowed.Errors[0].Code);
        Assert.True(success.IsSuccess, FormatErrors(success.Errors));
        Assert.Null(success.Value.Field);
        Assert.Single(success.Value.CustomInputs);
        Assert.Equal(0, await db.Set<Reservation>().CountAsync());
    }

    [Fact]
    public void Lookup_value_is_copied_to_ticket_and_reservation_archives()
    {
        var ticket = Ticket.Create(
            1, 10, 3, null, "A1", DateOnly.FromDateTime(Now), Now,
            Performer, "REF-42");
        var reservation = Reservation.Create(
            1, 10, 3, 1000, Now.AddHours(1), DateOnly.FromDateTime(Now),
            Now, Performer, "REF-84");

        var archiveMethods = typeof(TicketRuntimeRepository)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(x => x.Name == "CreateArchive")
            .ToArray();
        var ticketArchive = (TicketArchive)archiveMethods.Single(x =>
            x.GetParameters()[0].ParameterType == typeof(Ticket))
            .Invoke(null, new object[] { ticket, Now.AddDays(1) })!;
        var reservationArchive = (ReservationArchive)archiveMethods.Single(x =>
            x.GetParameters()[0].ParameterType == typeof(Reservation))
            .Invoke(null, new object[] { reservation, Now.AddDays(1) })!;

        Assert.Equal("REF-42", ticketArchive.LookupValue);
        Assert.Equal("REF-84", reservationArchive.LookupValue);
    }

    private static TicketRuntimeRepository CreateRepository(PlatformWriteDbContext db) =>
        new(db, new AlwaysAvailableService(),
            new TestDateTimeProvider { UtcNow = Now },
            NullLogger<TicketRuntimeRepository>.Instance);

    private static PlatformWriteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseInMemoryDatabase($"ticket-runtime-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new PlatformWriteDbContext(
            options, new TestTenantContext(), new TestCurrentBranchContext());
    }

    private static async Task<Service> SeedServiceGraphAsync(
        PlatformWriteDbContext db, int serviceId)
    {
        var service = EntityTestFactory.Service(
            serviceId, isTicketIssuable: true);
        var branch = EntityTestFactory.Branch(1);
        var segment = Segment.CreateGlobal(
            "شريحة", "Segment", 1, Performer);
        EntityTestFactory.AssignId(segment, 3);
        var branchService = BranchService.Create(1, service, Performer);
        EntityTestFactory.AssignId(branchService, serviceId * 10);
        var relationship = BranchServiceSegment.Create(
            branchService, segment, 100, Performer);
        EntityTestFactory.AssignId(relationship, serviceId * 100);
        db.AddRange(branch, service, segment, branchService, relationship);
        await db.SaveChangesAsync();
        return service;
    }

    private static async Task<Reservation> AddReservationAsync(
        PlatformWriteDbContext db,
        int branchId,
        int serviceId,
        int segmentId,
        int relationshipId,
        DateTime scheduledOnUtc,
        params (ServiceCustomInput Definition, string Value)[] inputs)
    {
        var reservation = Reservation.Create(
            branchId, serviceId, segmentId, relationshipId, scheduledOnUtc,
            DateOnly.FromDateTime(scheduledOnUtc), Now, Performer);
        foreach (var (definition, value) in inputs)
        {
            reservation.AddCustomInput(ReservationCustomInputValue.Create(
                definition.Id, definition.Name, definition.LabelEn,
                definition.LabelAr, definition.Type, value, Now));
        }
        db.Add(reservation);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return reservation;
    }

    private static async Task<Reservation> AddReservationAsync(
        PlatformWriteDbContext db,
        int branchId,
        int serviceId,
        int segmentId,
        int relationshipId,
        DateTime scheduledOnUtc,
        string lookupValue)
    {
        var reservation = Reservation.Create(
            branchId, serviceId, segmentId, relationshipId, scheduledOnUtc,
            DateOnly.FromDateTime(scheduledOnUtc), Now, Performer, lookupValue);
        db.Add(reservation);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return reservation;
    }

    private static string FormatErrors(IEnumerable<BuildingBlock.Domain.Results.Error> errors) =>
        string.Join(" | ", errors.Select(x => $"{x.Code}: {x.Message}"));

    private sealed class AlwaysAvailableService : IBranchServiceAvailabilityChecker
    {
        public Task<BranchServiceAvailability> CheckAsync(
            int branchId, int serviceId, DateTime atUtc,
            CancellationToken cancellationToken) =>
            Task.FromResult(new BranchServiceAvailability(
                ServiceExists: true,
                IsActive: true,
                IsAssigned: true,
                IsScheduleAvailable: true,
                IsTicketIssuable: true,
                HasReservation: true,
                RangePrefix: "A",
                RangeStartNumber: 1,
                RangeEndNumber: 999));
    }
}
