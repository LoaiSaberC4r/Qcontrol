using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Qcontrol.Application.Features.BranchServices.Query.GetBranchTicketIssuableServices;
using Qcontrol.Domain.Identity;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;
using QControl.infrastructure.Repositories;
using QControl.infrastructure.Services;

namespace QControl.Application.Tests.Tickets;

public sealed class KioskBackendSqlServerTests
{
    // Opt in with a SQL Server connection that can create a disposable database.
    // The caller's database is never used; InitialCatalog is always replaced.
    private const string ConnectionVariable = "QCONTROL_KIOSK_TEST_SQLSERVER";

    [SqlServerFact]
    public async Task SqlServer_executes_kiosk_queries_and_preserves_conversion_and_unique_reservation_guards()
    {
        var databaseName = $"QControl_KioskRegression_{Guid.NewGuid():N}";
        var connection = new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable(ConnectionVariable)!)
        {
            InitialCatalog = databaseName
        };
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer(connection.ConnectionString).Options;
        PlatformWriteDbContext OpenContext() =>
            new(options, new TestTenantContext(), new TestCurrentBranchContext());
        var performer = EntityTestFactory.CurrentUserId;
        var now = new DateTime(2026, 9, 1, 15, 0, 0, DateTimeKind.Utc);
        TicketRuntimeRepository Repository(PlatformWriteDbContext context, DateTime at) =>
            new(context, new BranchServiceAvailabilityChecker(context),
                new TestDateTimeProvider { UtcNow = at }, NullLogger<TicketRuntimeRepository>.Instance);

        await using var db = OpenContext();
        try
        {
            await db.Database.EnsureCreatedAsync();
            var user = ApplicationUser.Create("kiosk-regression", "kiosk@example.test", "Test",
                null, null, "test-fixture", UserType.TechnicalAdmin, performer);
            EntityTestFactory.AssignId(user, performer);
            db.Add(user);
            await db.SaveChangesAsync();
            var branch = EntityTestFactory.Branch(0);
            var service = EntityTestFactory.Service(0, isTicketIssuable: true);
            typeof(Service).GetProperty(nameof(Service.HasReservation))!.SetValue(service, true);
            var segment = Segment.CreateGlobal("شريحة", "Reserved", 1, performer);
            var defaultSegment = Segment.CreateSystemDefault("افتراضي", "Default", performer);
            db.AddRange(branch, service, segment, defaultSegment);
            await db.SaveChangesAsync();
            var assignment = BranchService.Create(branch.Id, service, performer);
            db.Add(assignment);
            await db.SaveChangesAsync();
            var relation = BranchServiceSegment.Create(assignment, segment, 100, performer);
            var defaultRelation = BranchServiceSegment.Create(assignment, defaultSegment, 100, performer);
            var originalInput = service.AddCustomInput("Original", "Original label", "الأصلي",
                ServiceCustomInputType.String, true, 3, null, null, null, null, 1, performer);
            var logo = ServiceImage.Create(service.Id, "Services/logo.png", ServiceImageType.Logo, 0, performer);
            var icon = ServiceImage.Create(service.Id, "Services/icon.png", ServiceImageType.Icon, 0, performer);
            db.AddRange(relation, defaultRelation, originalInput, logo, icon,
                BranchConfiguration.Create(branch.Id, TimeSpan.FromMinutes(30)),
                EntityTestFactory.ServiceSchedule(0, branch.Id, service.Id, timeSlots: new[]
                {
                    new ServiceScheduleTimeSlotDefinition(now.DayOfWeek,
                        new TimeOnly(15, 30), new TimeOnly(16, 0))
                }));
            await db.SaveChangesAsync();
            var repository = Repository(db, now);
            var inputs = new[] { new CustomInputSubmission(originalInput.Id, "original-value") };

            var earlyBooking = await repository.CreateReservationAsync(branch.Id, service.Id,
                segment.Id, now.AddMinutes(10), null, inputs, performer, CancellationToken.None);
            Assert.True(earlyBooking.IsFailure);
            var booking = await repository.CreateReservationAsync(branch.Id, service.Id,
                segment.Id, now.AddMinutes(45), null, inputs, performer, CancellationToken.None);
            Assert.True(booking.IsSuccess, string.Join(" | ", booking.Errors));

            var resolver = new PlatformWriteReadDbContextResolver(db);
            var handler = new GetBranchTicketIssuableServicesQueryHandler(
                new PlatformWriteReadRepository<Branch>(resolver),
                new PlatformWriteReadRepository<BranchService>(resolver),
                new PlatformWriteReadRepository<Service>(resolver),
                new PlatformWriteReadRepository<ServiceSchedule>(resolver),
                new PlatformWriteReadRepository<GeneralBrand>(resolver),
                new TestCurrentUser { IsAuthenticated = false, UserId = null },
                new TestBranchAccessValidator(), new TestDateTimeProvider { UtcNow = now });
            var discovery = await handler.Handle(new() { BranchId = branch.Id }, CancellationToken.None);
            Assert.True(discovery.IsSuccess);
            var node = Assert.Single(discovery.Value.Services);
            Assert.True(node.HasReservation);
            Assert.Equal("/Media/Services/logo.png", node.LogoUrl);
            Assert.Equal("/Media/Services/icon.png", node.IconUrl);

            service.DeactivateCustomInput(originalInput, performer);
            var replacement = service.AddCustomInput("Replacement", null, null,
                ServiceCustomInputType.String, true, null, null, null, null, null, 1, performer);
            db.Add(replacement);
            await db.SaveChangesAsync();
            var search = await Repository(db, now.AddMinutes(35)).SearchReservationsForKioskAsync(
                branch.Id, service.Id, null, inputs, CancellationToken.None);
            Assert.True(search.IsSuccess);
            Assert.Equal(booking.Value.Id, Assert.Single(search.Value).ReservationId);

            var ticket = await repository.CreateTicketFromReservationAsync(branch.Id, booking.Value.Id,
                performer, CancellationToken.None);
            Assert.True(ticket.IsSuccess, string.Join(" | ", ticket.Errors));
            Assert.Equal(segment.Id, ticket.Value.SegmentId);
            Assert.Equal("Original", Assert.Single(ticket.Value.CustomInputs).Name);
            Assert.Equal(1, (await db.Set<BranchServiceSegmentDailyUsage>().AsNoTracking().SingleAsync()).ConsumedCount);

            await using (var other = OpenContext())
            {
                var duplicate = await Repository(other, now).CreateTicketFromReservationAsync(branch.Id,
                    booking.Value.Id, performer, CancellationToken.None);
                Assert.Equal("Reservations.Convert.AlreadyConverted", duplicate.Errors[0].Code);
                var duplicateTicket = Ticket.Create(branch.Id, service.Id, segment.Id, booking.Value.Id,
                    "unique-index-probe", DateOnly.FromDateTime(now), now, performer);
                other.Add(duplicateTicket);
                await Assert.ThrowsAsync<DbUpdateException>(() => other.SaveChangesAsync());
            }
            Assert.Single(await db.Set<Ticket>().AsNoTracking().ToListAsync());

            // SQL Server's SET NULL action exposes the existing contract limit:
            // snapshots survive deletion, but the old public input ID is lost.
            db.Remove(originalInput);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            var snapshot = await db.Set<ReservationCustomInputValue>().AsNoTracking().SingleAsync();
            Assert.Null(snapshot.ServiceCustomInputId);
            Assert.Equal("Original", snapshot.NameSnapshot);
            Assert.Equal("original-value", snapshot.Value);
            var deletedLookup = await Repository(db, now.AddMinutes(35)).SearchReservationsForKioskAsync(
                branch.Id, service.Id, null, inputs, CancellationToken.None);
            Assert.Equal("TicketRuntime.CustomInputs.NotApplicable", deletedLookup.Errors[0].Code);
        }
        finally
        {
            // options always target the unique database generated above.
            await db.Database.EnsureDeletedAsync();
        }
    }

    private sealed class SqlServerFactAttribute : FactAttribute
    {
        public SqlServerFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionVariable)))
                Skip = $"Set {ConnectionVariable} to run against a disposable SQL Server database.";
        }
    }
}
