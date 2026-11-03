using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Qcontrol.Application.Features.BranchServices.Query.GetBranchTicketIssuableServices;
using QControl.Application.Abstraction.Services;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;
using QControl.infrastructure.Repositories;
using QControl.infrastructure.Services;

namespace QControl.Application.Tests.Tickets;

public sealed partial class KioskTicketRuntimeRepositoryTests
{
    [Theory]
    [InlineData("valid-global", true)]
    [InlineData("valid-scoped", true)]
    [InlineData("inactive-service", false)]
    [InlineData("deleted-service", false)]
    [InlineData("inactive-parent", false)]
    [InlineData("deleted-parent", false)]
    [InlineData("inactive-grandparent", false)]
    [InlineData("parent", false)]
    [InlineData("deleted-child", false)]
    [InlineData("wrong-scope", false)]
    [InlineData("unassigned", false)]
    [InlineData("inactive-branch", false)]
    [InlineData("missing-branch", false)]
    [InlineData("missing-service", false)]
    [InlineData("not-ticket-issuable", false)]
    public async Task Runtime_eligibility_is_enforced_for_ticket_reservation_conversion_and_search(
        string scenario, bool expected)
    {
        await using var db = CreateContext();
        var service = await SeedServiceGraphAsync(db, 10);
        SetReservationSupport(service, true);
        db.Add(EntityTestFactory.ServiceSchedule(1, 1, 10, timeSlots: new[]
        {
            new ServiceScheduleTimeSlotDefinition(Now.DayOfWeek,
                new TimeOnly(8, 0), new TimeOnly(16, 0))
        }));

        if (scenario is "valid-scoped" or "wrong-scope")
        {
            typeof(Service).GetProperty(nameof(Service.Scope))!.SetValue(service, ServiceScope.BranchScoped);
            typeof(Service).GetProperty(nameof(Service.OwnerBranchId))!.SetValue(service,
                scenario == "valid-scoped" ? 1 : 2);
        }
        if (scenario == "inactive-service") EntityTestFactory.SetServiceActive(service, false);
        if (scenario == "deleted-service") service.IsDeleted = true;
        if (scenario is "inactive-parent" or "deleted-parent" or "inactive-grandparent")
        {
            var parent = EntityTestFactory.Service(20);
            service.ChangeParent(parent.Id, Performer);
            if (scenario == "inactive-parent") EntityTestFactory.SetServiceActive(parent, false);
            if (scenario == "deleted-parent") parent.IsDeleted = true;
            if (scenario == "inactive-grandparent")
            {
                var grandparent = EntityTestFactory.Service(30);
                EntityTestFactory.SetServiceActive(grandparent, false);
                parent.ChangeParent(grandparent.Id, Performer);
                db.Add(grandparent);
            }
            db.Add(parent);
        }
        if (scenario is "parent" or "deleted-child")
        {
            var child = EntityTestFactory.Service(20, parentServiceId: 10);
            child.IsDeleted = scenario == "deleted-child";
            db.Add(child);
        }
        if (scenario == "not-ticket-issuable")
            typeof(Service).GetProperty(nameof(Service.IsTicketIssuable))!.SetValue(service, false);
        if (scenario == "unassigned")
        {
            db.Add(EntityTestFactory.Branch(2));
            var assignment = await db.Set<BranchService>().SingleAsync();
            typeof(BranchService).GetProperty(nameof(BranchService.BranchId))!.SetValue(assignment, 2);
        }
        if (scenario == "inactive-branch")
            (await db.Set<Branch>().SingleAsync()).Deactivate(Now, Performer);
        await db.SaveChangesAsync();
        await AddDefaultSegmentAsync(db);
        var branchId = scenario == "missing-branch" ? 999 : 1;
        var serviceId = scenario == "missing-service" ? 999 : 10;
        var reservation = await AddReservationAsync(db, branchId, serviceId, 3, 1000,
            Now.AddHours(1), "REF-42");
        var repository = CreateRealRepository(db);

        var ticket = await repository.CreateKioskTicketAsync(branchId, serviceId,
            "REF-42", Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        var booking = await repository.CreateReservationAsync(branchId, serviceId, 3,
            Now.AddHours(1), "REF-42", Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        var conversion = await repository.CreateTicketFromReservationAsync(branchId,
            reservation.Id, Performer, CancellationToken.None);
        var search = await repository.SearchReservationsForKioskAsync(branchId, serviceId,
            "REF-42", Array.Empty<CustomInputSubmission>(), CancellationToken.None);

        Assert.Equal(expected, ticket.IsSuccess);
        Assert.Equal(expected, booking.IsSuccess);
        Assert.Equal(expected, conversion.IsSuccess);
        Assert.Equal(expected, search.IsSuccess);
        if (!expected)
        {
            Assert.Equal("Tickets.Create.ServiceUnavailable", ticket.Errors[0].Code);
            Assert.Equal("Reservations.Create.ServiceUnavailable", booking.Errors[0].Code);
            Assert.Equal("Reservations.Convert.ServiceUnavailable", conversion.Errors[0].Code);
            Assert.Equal("Kiosk.Reservations.Search.ServiceUnavailable", search.Errors[0].Code);
            Assert.Empty(await db.Set<Ticket>().ToListAsync());
            Assert.Empty(await db.Set<BranchServiceSegmentDailyUsage>().ToListAsync());
            Assert.Empty(await db.Set<TicketNumberSequence>().ToListAsync());
        }
    }

    [Theory]
    [InlineData(30, 15, 0, true)]
    [InlineData(20, 15, 0, false)]
    [InlineData(20, 15, 10, true)]
    [InlineData(20, 15, 9, false)]
    [InlineData(0, 15, 29, false)]
    [InlineData(0, 15, 30, true)]
    [InlineData(30, 16, 0, false)]
    [InlineData(30, 16, 1, false)]
    [InlineData(0, 15, 59, true)]
    [InlineData(30, 23, 50, false)]
    public async Task Arrival_allowance_applies_to_walk_in_and_conversion_but_not_appointment_booking(
        int allowanceMinutes, int hour, int minute, bool allowed)
    {
        await using var db = CreateContext();
        var service = await SeedServiceGraphAsync(db, 10);
        SetReservationSupport(service, true);
        var configuration = EntityTestFactory.BranchConfiguration(1, 1,
            TimeSpan.FromMinutes(allowanceMinutes));
        db.Add(configuration);
        db.Add(EntityTestFactory.ServiceSchedule(1, 1, 10, timeSlots: new[]
        {
            new ServiceScheduleTimeSlotDefinition(Now.DayOfWeek,
                new TimeOnly(15, 30), new TimeOnly(16, 0))
        }));
        await db.SaveChangesAsync();
        await AddDefaultSegmentAsync(db);
        var now = Now.Date.AddHours(hour).AddMinutes(minute);
        var reservation = await AddReservationAsync(db, 1, 10, 3, 1000,
            Now.Date.AddHours(15).AddMinutes(45), "REF-42");
        var repository = CreateRealRepository(db, now);
        var resolver = new PlatformWriteReadDbContextResolver(db);
        var discovery = await new GetBranchTicketIssuableServicesQueryHandler(
            new PlatformWriteReadRepository<Branch>(resolver),
            new PlatformWriteReadRepository<BranchService>(resolver),
            new PlatformWriteReadRepository<Service>(resolver),
            new PlatformWriteReadRepository<ServiceSchedule>(resolver),
            new PlatformWriteReadRepository<GeneralBrand>(resolver),
            new TestCurrentUser(), new TestBranchAccessValidator(),
            new TestDateTimeProvider { UtcNow = now })
            .Handle(new() { BranchId = 1 }, CancellationToken.None);
        Assert.Equal(allowed, discovery.Value.Services.Count > 0);

        var ticket = await repository.CreateKioskTicketAsync(1, 10, "REF-42",
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        var conversion = await repository.CreateTicketFromReservationAsync(1,
            reservation.Id, Performer, CancellationToken.None);

        Assert.Equal(allowed, ticket.IsSuccess);
        Assert.Equal(allowed, conversion.IsSuccess);

        // Booking checks the requested appointment against the ordinary schedule,
        // even when a customer could arrive earlier under AllowedTime.
        var bookingRepository = CreateRealRepository(db, Now);
        var tooEarly = await bookingRepository.CreateReservationAsync(1, 10, 3,
            Now.Date.AddHours(15).AddMinutes(10), "REF-42",
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        var valid = await bookingRepository.CreateReservationAsync(1, 10, 3,
            Now.Date.AddHours(15).AddMinutes(30), "REF-42",
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        var end = await bookingRepository.CreateReservationAsync(1, 10, 3,
            Now.Date.AddHours(16), "REF-42",
            Array.Empty<CustomInputSubmission>(), Performer, CancellationToken.None);
        Assert.True(tooEarly.IsFailure);
        Assert.True(valid.IsSuccess, FormatErrors(valid.Errors));
        Assert.True(end.IsFailure);
        Assert.Equal(3, valid.Value.SegmentId);
    }

    [Fact]
    public async Task Future_booking_uses_appointment_weekday_and_date_without_arrival_allowance()
    {
        await using var db = CreateContext();
        var service = await SeedServiceGraphAsync(db, 10);
        SetReservationSupport(service, true);
        var tomorrow = Now.AddDays(1);
        db.AddRange(BranchConfiguration.Create(1, TimeSpan.FromHours(1)),
            EntityTestFactory.ServiceSchedule(1, 1, 10, timeSlots: new[]
            {
                new ServiceScheduleTimeSlotDefinition(tomorrow.DayOfWeek,
                    new TimeOnly(9, 0), new TimeOnly(10, 0))
            }));
        await db.SaveChangesAsync();
        var repository = CreateRealRepository(db);
        var valid = await repository.CreateReservationAsync(1, 10, 3,
            tomorrow.Date.AddHours(9), "REF-42", Array.Empty<CustomInputSubmission>(),
            Performer, CancellationToken.None);
        var early = await repository.CreateReservationAsync(1, 10, 3,
            tomorrow.Date.AddHours(8).AddMinutes(59), "REF-42", Array.Empty<CustomInputSubmission>(),
            Performer, CancellationToken.None);
        Assert.True(valid.IsSuccess, FormatErrors(valid.Errors));
        Assert.Equal(DateOnly.FromDateTime(tomorrow), valid.Value.BusinessDate);
        Assert.Equal(tomorrow.Date.AddHours(9), valid.Value.ScheduledOnUtc);
        Assert.True(early.IsFailure);
        Assert.Equal(1, (await db.Set<BranchServiceSegmentDailyUsage>().SingleAsync()).ConsumedCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Reservation_creation_requires_reservation_support(bool supportsReservation)
    {
        await using var db = CreateContext();
        var service = await SeedServiceGraphAsync(db, 10);
        SetReservationSupport(service, supportsReservation);
        db.Add(EntityTestFactory.ServiceSchedule(1, 1, 10, timeSlots: new[]
        {
            new ServiceScheduleTimeSlotDefinition(Now.DayOfWeek,
                new TimeOnly(8, 0), new TimeOnly(16, 0))
        }));
        await db.SaveChangesAsync();
        var result = await CreateRealRepository(db).CreateReservationAsync(1, 10, 3,
            Now.AddHours(1), "REF-42", Array.Empty<CustomInputSubmission>(), Performer,
            CancellationToken.None);
        Assert.Equal(supportsReservation, result.IsSuccess);
    }

    [Theory]
    [InlineData(ReservationStatus.Active, 0, true)]
    [InlineData(ReservationStatus.Active, -1, false)]
    [InlineData(ReservationStatus.Active, 1, false)]
    [InlineData(ReservationStatus.NoShow, 0, true)]
    [InlineData(ReservationStatus.NoShow, -1, false)]
    [InlineData(ReservationStatus.NoShow, 1, false)]
    [InlineData(ReservationStatus.Cancelled, 0, false)]
    [InlineData(ReservationStatus.Expired, 0, false)]
    [InlineData(ReservationStatus.ConvertedToTicket, 0, false)]
    public async Task Conversion_requires_same_UTC_business_date_and_preserves_status_guards(
        ReservationStatus status, int dayOffset, bool expected)
    {
        await using var db = CreateContext();
        await SeedServiceGraphAsync(db, 10);
        var reservation = await AddReservationAsync(db, 1, 10, 3, 1000,
            Now.AddDays(dayOffset).AddHours(1), "REF-42");
        reservation = await db.Set<Reservation>().SingleAsync();
        if (status is ReservationStatus.NoShow or ReservationStatus.Expired)
            Assert.True(reservation.TryMarkNoShow(Now));
        if (status == ReservationStatus.Expired) Assert.True(reservation.TryExpire(Now));
        if (status == ReservationStatus.Cancelled) Assert.True(reservation.TryCancel("reason", Performer, Now));
        if (status == ReservationStatus.ConvertedToTicket) Assert.True(reservation.TryConvert(Now));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = CreateRepository(db);

        var result = await repository.CreateTicketFromReservationAsync(1, reservation.Id,
            Performer, CancellationToken.None);

        Assert.Equal(expected, result.IsSuccess);
        if (expected)
        {
            Assert.Equal(3, result.Value.SegmentId);
            Assert.Equal("REF-42", result.Value.Field);
            Assert.Equal("A1", result.Value.TicketNumber);
            var duplicate = await repository.CreateTicketFromReservationAsync(1,
                reservation.Id, Performer, CancellationToken.None);
            Assert.Equal("Reservations.Convert.AlreadyConverted", duplicate.Errors[0].Code);
            Assert.Single(await db.Set<Ticket>().ToListAsync());
        }
        else
        {
            var code = status switch
            {
                ReservationStatus.Cancelled or ReservationStatus.Expired => "Terminal",
                ReservationStatus.ConvertedToTicket => "AlreadyConverted",
                _ => "DifferentDay"
            };
            Assert.Equal($"Reservations.Convert.{code}", result.Errors[0].Code);
            Assert.Empty(await db.Set<Ticket>().ToListAsync());
            db.ChangeTracker.Clear();
            Assert.Equal(status, (await db.Set<Reservation>().SingleAsync()).Status);
        }
        Assert.Empty(await db.Set<BranchServiceSegmentDailyUsage>().ToListAsync());
    }

    private static TicketRuntimeRepository CreateRealRepository(PlatformWriteDbContext db,
        DateTime? now = null) => new(db, new BranchServiceAvailabilityChecker(db),
        new TestDateTimeProvider { UtcNow = now ?? Now },
        NullLogger<TicketRuntimeRepository>.Instance);

    private static void SetReservationSupport(Service service, bool value) =>
        typeof(Service).GetProperty(nameof(Service.HasReservation))!.SetValue(service, value);
}
