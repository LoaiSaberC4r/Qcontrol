using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using System.Text.Json;
using Qcontrol.Api.Controllers;
using QControl.Api.Contracts.Kiosk;
using QControl.Application.Abstraction.Services;
using QControl.Application.Features.TicketRuntime;
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

    [Theory]
    [InlineData("")]
    [InlineData(",\"segmentId\":null")]
    [InlineData(",\"segmentId\":1")]
    [InlineData(",\"segmentId\":3")]
    [InlineData(",\"segmentId\":999")]
    [InlineData(",\"segmentId\":-1")]
    public async Task Kiosk_endpoint_uses_assigned_system_default_regardless_of_client_segment(
        string segmentProperty)
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        await AddDefaultSegmentAsync(db);
        var repository = CreateRepository(db);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITicketRuntimeRepository>(repository);
        services.AddSingleton(new TicketRuntimeRequestGuard(
            new TestCurrentUser(), new TestBranchAccessValidator()));
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<CreateKioskTicketCommand>());
        using var provider = services.BuildServiceProvider();
        var controller = new KioskController(provider.GetRequiredService<ISender>());
        var request = JsonSerializer.Deserialize<CreateKioskTicketRequest>(
            "{\"serviceId\":10,\"field\":\"REF-42\",\"customInputs\":[]" + segmentProperty + "}",
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        var response = await controller.CreateTicket(1, request, CancellationToken.None);

        var details = Assert.IsType<TicketDetailsResponse>(
            Assert.IsType<OkObjectResult>(response).Value);
        Assert.Equal(7, details.SegmentId);
        db.ChangeTracker.Clear();
        var ticket = await db.Set<Ticket>().SingleAsync();
        Assert.Equal(7, ticket.SegmentId);
        Assert.Equal(TicketStatus.Waiting, ticket.Status);
        Assert.Equal(DateOnly.FromDateTime(Now), ticket.BusinessDate);
        Assert.Equal("A1", ticket.TicketNumber);
        var usage = await db.Set<BranchServiceSegmentDailyUsage>().SingleAsync();
        Assert.Equal(1007, usage.BranchServiceSegmentId);
        Assert.Equal(1, usage.ConsumedCount);
        Assert.Equal(TicketHistoryEventType.Created,
            (await db.Set<TicketHistory>().SingleAsync()).EventType);
        Assert.Empty(await db.Set<Reservation>().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Kiosk_without_default_assignment_does_not_fall_back_or_consume_quota(
        bool hasUnassignedDefault)
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        if (hasUnassignedDefault)
        {
            var segment = Segment.CreateSystemDefault("افتراضي", "Default", Performer);
            EntityTestFactory.AssignId(segment, 7);
            db.Add(segment);
            await db.SaveChangesAsync();
        }
        db.ChangeTracker.Clear();

        var result = await CreateRepository(db).CreateKioskTicketAsync(
            1, 10, "REF-42", Array.Empty<CustomInputSubmission>(),
            Performer, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tickets.Create.SegmentInvalid", result.Errors[0].Code);
        Assert.Equal(ErrorType.Domain, result.Errors[0].Type);
        Assert.Empty(await db.Set<Ticket>().ToListAsync());
        Assert.Empty(await db.Set<BranchServiceSegmentDailyUsage>().ToListAsync());
        Assert.Empty(await db.Set<TicketNumberSequence>().ToListAsync());
        Assert.Single(await db.Set<BranchServiceSegment>().ToListAsync());
    }

    [Theory]
    [InlineData(2, 10)]
    [InlineData(1, 20)]
    [InlineData(2, 20)]
    public async Task Kiosk_cannot_use_default_assignment_for_another_branch_or_service(
        int branchId, int serviceId)
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        if (branchId != 1) db.Add(EntityTestFactory.Branch(branchId));
        var service = serviceId == 10
            ? await db.Set<Service>().SingleAsync(x => x.Id == serviceId)
            : EntityTestFactory.Service(serviceId, isTicketIssuable: true);
        if (serviceId != 10) db.Add(service);
        var branchService = BranchService.Create(branchId, service, Performer);
        EntityTestFactory.AssignId(branchService, 200);
        db.Add(branchService);
        await db.SaveChangesAsync();
        await AddDefaultSegmentAsync(db, branchServiceId: 200);

        var result = await CreateRepository(db).CreateKioskTicketAsync(
            1, 10, "REF-42", Array.Empty<CustomInputSubmission>(),
            Performer, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tickets.Create.SegmentInvalid", result.Errors[0].Code);
        Assert.Empty(await db.Set<Ticket>().ToListAsync());
        Assert.Empty(await db.Set<BranchServiceSegmentDailyUsage>().ToListAsync());
        Assert.Empty(await db.Set<TicketNumberSequence>().ToListAsync());
        Assert.Equal(2, await db.Set<BranchServiceSegment>().CountAsync());
    }

    [Fact]
    public async Task Kiosk_rejects_ambiguous_default_assignments_without_consuming_quota()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        await AddDefaultSegmentAsync(db);
        await AddDefaultSegmentAsync(db, segmentId: 8);

        var result = await CreateRepository(db).CreateKioskTicketAsync(
            1, 10, "REF-42", Array.Empty<CustomInputSubmission>(),
            Performer, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tickets.Create.SegmentInvalid", result.Errors[0].Code);
        Assert.Empty(await db.Set<Ticket>().ToListAsync());
        Assert.Empty(await db.Set<BranchServiceSegmentDailyUsage>().ToListAsync());
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 2)]
    public async Task Kiosk_preserves_default_assignment_quota_validation(int quota, int consumed)
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        var relationship = await AddDefaultSegmentAsync(db, quota: quota);
        if (consumed > 0)
        {
            db.Add(BranchServiceSegmentDailyUsage.Create(
                relationship.Id, DateOnly.FromDateTime(Now), consumed, Now));
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }

        var result = await CreateRepository(db).CreateKioskTicketAsync(
            1, 10, "REF-42", Array.Empty<CustomInputSubmission>(),
            Performer, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Tickets.Create.QuotaExceeded", result.Errors[0].Code);
        Assert.Equal(ErrorType.Conflict, result.Errors[0].Type);
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<Ticket>().ToListAsync());
        Assert.Empty(await db.Set<TicketNumberSequence>().ToListAsync());
        Assert.Equal(consumed, await db.Set<BranchServiceSegmentDailyUsage>()
            .SumAsync(x => x.ConsumedCount));
    }

    [Fact]
    public async Task Reservation_conversion_preserves_non_default_segment_and_consumes_no_second_quota()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        await AddDefaultSegmentAsync(db);
        var repository = CreateRepository(db);
        var reservation = await repository.CreateReservationAsync(
            1, 10, 3, Now.AddHours(1), "REF-42",
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        Assert.True(reservation.IsSuccess, FormatErrors(reservation.Errors));
        db.ChangeTracker.Clear();

        var ticket = await repository.CreateTicketFromReservationAsync(
            1, reservation.Value.Id, Performer, CancellationToken.None);
        var duplicate = await repository.CreateTicketFromReservationAsync(
            1, reservation.Value.Id, Performer, CancellationToken.None);

        Assert.True(ticket.IsSuccess, FormatErrors(ticket.Errors));
        Assert.Equal(3, ticket.Value.SegmentId);
        Assert.Equal("Reservations.Convert.AlreadyConverted", duplicate.Errors[0].Code);
        db.ChangeTracker.Clear();
        Assert.Equal(3, (await db.Set<Ticket>().SingleAsync()).SegmentId);
        var storedReservation = await db.Set<Reservation>().SingleAsync();
        Assert.Equal(3, storedReservation.SegmentId);
        Assert.Equal(ReservationStatus.ConvertedToTicket, storedReservation.Status);
        var usage = await db.Set<BranchServiceSegmentDailyUsage>().SingleAsync();
        Assert.Equal(1000, usage.BranchServiceSegmentId);
        Assert.Equal(1, usage.ConsumedCount);
        Assert.Contains(await db.Set<ReservationHistory>().ToListAsync(),
            x => x.EventType == ReservationHistoryEventType.ConvertedToTicket);
    }

    [Fact]
    public async Task Administrative_ticket_creation_keeps_explicit_non_default_segment()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        await AddDefaultSegmentAsync(db);

        var result = await CreateRepository(db).CreateTicketAsync(
            1, 10, 3, "REF-42", Array.Empty<CustomInputSubmission>(),
            requireLookupValueWhenNoCustomInputs: true, Performer, CancellationToken.None);

        Assert.True(result.IsSuccess, FormatErrors(result.Errors));
        Assert.Equal(3, result.Value.SegmentId);
        Assert.Equal(3, (await db.Set<Ticket>().SingleAsync()).SegmentId);
        var usage = await db.Set<BranchServiceSegmentDailyUsage>().SingleAsync();
        Assert.Equal(1000, usage.BranchServiceSegmentId);
        Assert.Equal(1, usage.ConsumedCount);
    }

    [Theory]
    [InlineData(0, 1000)]
    [InlineData(7, 1000)]
    [InlineData(3, 0)]
    [InlineData(3, 1007)]
    public async Task Reservation_conversion_rejects_invalid_segment_relationship_without_default_fallback(
        int segmentId, int relationshipId)
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        await AddDefaultSegmentAsync(db);
        var reservation = await AddReservationAsync(db, 1, 10, segmentId,
            relationshipId, Now.AddHours(1), "REF-42");

        var result = await CreateRepository(db).CreateTicketFromReservationAsync(
            1, reservation.Id, Performer, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Reservations.Convert.SegmentInvalid", result.Errors[0].Code);
        db.ChangeTracker.Clear();
        var stored = await db.Set<Reservation>().SingleAsync();
        Assert.Equal(segmentId, stored.SegmentId);
        Assert.Equal(ReservationStatus.Active, stored.Status);
        Assert.Empty(await db.Set<Ticket>().ToListAsync());
        Assert.Empty(await db.Set<BranchServiceSegmentDailyUsage>().ToListAsync());
        Assert.Empty(await db.Set<TicketNumberSequence>().ToListAsync());
    }

    [Fact]
    public async Task Direct_kiosk_ticket_creates_ticket_without_reservation()
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, serviceId: 10);
        await AddDefaultSegmentAsync(db);
        var repository = CreateRepository(db);

        var missing = await repository.CreateKioskTicketAsync(
            1, 10, null, Array.Empty<CustomInputSubmission>(), Performer,
            CancellationToken.None);
        var result = await repository.CreateKioskTicketAsync(
            1, 10, "REF-42", Array.Empty<CustomInputSubmission>(), Performer,
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
        await AddDefaultSegmentAsync(db);
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

        var missing = await repository.CreateKioskTicketAsync(
            1, 10, null, Array.Empty<CustomInputSubmission>(),
            Performer, CancellationToken.None);
        var fieldNotAllowed = await repository.CreateKioskTicketAsync(
            1, 10, "REF-42", input,
            Performer, CancellationToken.None);
        var success = await repository.CreateKioskTicketAsync(
            1, 10, null, input,
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

    private static async Task<BranchServiceSegment> AddDefaultSegmentAsync(
        PlatformWriteDbContext db, int branchServiceId = 100, int segmentId = 7,
        int quota = 100)
    {
        var segment = Segment.CreateSystemDefault("افتراضي", "Default", Performer);
        EntityTestFactory.AssignId(segment, segmentId);
        var relationship = BranchServiceSegment.Create(
            branchServiceId, segmentId, quota, Performer);
        EntityTestFactory.AssignId(relationship, branchServiceId * 10 + segmentId);
        db.AddRange(segment, relationship);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return relationship;
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
