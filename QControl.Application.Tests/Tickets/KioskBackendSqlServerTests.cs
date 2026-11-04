using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
        var reads = new ServiceReadInterceptor();
        var options = CreateOptions(reads);
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
            await db.Database.MigrateAsync();
            db.Add(CreatePerformer());
            await db.SaveChangesAsync();
            var branch = EntityTestFactory.Branch(0);
            var service = EntityTestFactory.Service(0, isTicketIssuable: true);
            typeof(Service).GetProperty(nameof(Service.HasReservation))!.SetValue(service, true);
            typeof(Service).GetProperty(nameof(Service.IsClientInputRequired))!.SetValue(service, true);
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

            var originalId = originalInput.Id;
            var saved = await db.Set<ReservationCustomInputValue>().AsNoTracking().SingleAsync();
            Assert.Equal(originalId, saved.ServiceCustomInputId);
            Assert.Equal(originalId, saved.OriginalServiceCustomInputId);

            var resolver = new PlatformWriteReadDbContextResolver(db);
            var handler = new GetBranchTicketIssuableServicesQueryHandler(
                new PlatformWriteReadRepository<Branch>(resolver),
                new PlatformWriteReadRepository<BranchService>(resolver),
                new PlatformWriteReadRepository<Service>(resolver),
                new PlatformWriteReadRepository<ServiceSchedule>(resolver),
                new PlatformWriteReadRepository<GeneralBrand>(resolver),
                new PlatformWriteReadRepository<Reservation>(resolver),
                new TestCurrentUser { IsAuthenticated = false, UserId = null },
                new TestBranchAccessValidator(), new TestDateTimeProvider { UtcNow = now });
            var discovery = await handler.Handle(new() { BranchId = branch.Id }, CancellationToken.None);
            Assert.True(discovery.IsSuccess);
            var node = Assert.Single(discovery.Value.Services);
            Assert.True(node.HasReservation);
            Assert.Equal("/Media/Services/logo.png", node.LogoUrl);
            Assert.Equal("/Media/Services/icon.png", node.IconUrl);

            Assert.Equal(originalId, Assert.Single(node.CustomInputs!).CustomInputId);
            Assert.Null(node.HistoricalSearchInputs);

            service.DeactivateCustomInput(originalInput, performer);
            var replacement = service.AddCustomInput("Replacement", null, null,
                ServiceCustomInputType.String, true, null, null, null, null, null, 1, performer);
            db.Add(replacement);
            await db.SaveChangesAsync();
            var search = await repository.SearchReservationsForKioskAsync(
                branch.Id, service.Id, null, inputs, CancellationToken.None);
            Assert.True(search.IsSuccess);
            Assert.Equal(booking.Value.Id, Assert.Single(search.Value).ReservationId);

            db.ChangeTracker.Clear();
            Assert.Equal(1, await db.Set<ServiceCustomInput>().Where(x => x.Id == originalId).ExecuteDeleteAsync());
            var snapshot = await db.Set<ReservationCustomInputValue>().AsNoTracking().SingleAsync();
            Assert.Null(snapshot.ServiceCustomInputId);
            Assert.Equal(originalId, snapshot.OriginalServiceCustomInputId);

            var deletedLookup = await repository.SearchReservationsForKioskAsync(
                branch.Id, service.Id, null, inputs, CancellationToken.None);
            Assert.True(deletedLookup.IsSuccess, string.Join(" | ", deletedLookup.Errors));
            Assert.Equal(booking.Value.Id, Assert.Single(deletedLookup.Value).ReservationId);
            var wrongValue = await repository.SearchReservationsForKioskAsync(
                branch.Id, service.Id, null, [new(originalId, "wrong")], CancellationToken.None);
            Assert.True(wrongValue.IsSuccess);
            Assert.Empty(wrongValue.Value);

            reads.HistoricalSnapshotQueries.Clear();
            var updatedDiscovery = await handler.Handle(new() { BranchId = branch.Id }, CancellationToken.None);
            var updatedNode = Assert.Single(updatedDiscovery.Value.Services);
            Assert.Equal(replacement.Id, Assert.Single(updatedNode.CustomInputs!).CustomInputId);
            var historical = Assert.Single(updatedNode.HistoricalSearchInputs!);
            Assert.Equal(originalId, historical.CustomInputId);
            Assert.Equal("Original", historical.Name);
            Assert.Equal("Original label", historical.LabelEn);
            Assert.Equal("الأصلي", historical.LabelAr);
            Assert.Equal(ServiceCustomInputType.String, historical.Type);
            Assert.Equal(1, historical.Order);
            Assert.False(historical.IsRequired);
            Assert.Null(historical.MinLength);
            Assert.DoesNotContain("[Value]", Assert.Single(reads.HistoricalSnapshotQueries));

            var rejectedCreation = await Repository(db, now.AddMinutes(35)).CreateKioskTicketAsync(
                branch.Id, service.Id, null, inputs, performer, CancellationToken.None);
            Assert.Equal("TicketRuntime.CustomInputs.NotApplicable", rejectedCreation.Errors[0].Code);

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

            var convertedSnapshot = await db.Set<TicketCustomInputValue>().AsNoTracking().SingleAsync();
            Assert.Equal(originalId, convertedSnapshot.OriginalServiceCustomInputId);
            Assert.Equal((null, "Original", "Original label", "الأصلي", ServiceCustomInputType.String, 1, "original-value"),
                (convertedSnapshot.ServiceCustomInputId, convertedSnapshot.NameSnapshot, convertedSnapshot.LabelEnSnapshot,
                    convertedSnapshot.LabelArSnapshot, convertedSnapshot.TypeSnapshot, convertedSnapshot.OrderSnapshot, convertedSnapshot.Value));

            var activeTicket = await db.Set<Ticket>().SingleAsync();
            Assert.True(activeTicket.TryCancel("test archive", performer, now));
            await db.SaveChangesAsync();
            var archiver = Repository(db, now.AddDays(31));
            Assert.Equal(1, await archiver.ArchiveEligibleTicketsAsync(10, CancellationToken.None));
            Assert.Equal(1, await archiver.ArchiveEligibleReservationsAsync(10, CancellationToken.None));
            var archivedTicket = await db.Set<TicketCustomInputValueArchive>().AsNoTracking().SingleAsync();
            var archivedReservation = await db.Set<ReservationCustomInputValueArchive>().AsNoTracking().SingleAsync();
            var expected = (snapshot.ServiceCustomInputId, snapshot.OriginalServiceCustomInputId,
                snapshot.NameSnapshot, snapshot.LabelEnSnapshot, snapshot.LabelArSnapshot,
                snapshot.TypeSnapshot, snapshot.OrderSnapshot, snapshot.Value);
            Assert.Equal(expected, (archivedTicket.ServiceCustomInputId, archivedTicket.OriginalServiceCustomInputId,
                archivedTicket.NameSnapshot, archivedTicket.LabelEnSnapshot, archivedTicket.LabelArSnapshot,
                archivedTicket.TypeSnapshot, archivedTicket.OrderSnapshot, archivedTicket.Value));
            Assert.Equal(expected, (archivedReservation.ServiceCustomInputId, archivedReservation.OriginalServiceCustomInputId,
                archivedReservation.NameSnapshot, archivedReservation.LabelEnSnapshot, archivedReservation.LabelArSnapshot,
                archivedReservation.TypeSnapshot, archivedReservation.OrderSnapshot, archivedReservation.Value));

            // Create rows with a live FK alongside rows that already lost their identity.
            var currentInputs = new[] { new CustomInputSubmission(replacement.Id, "current-value") };
            var secondBooking = await repository.CreateReservationAsync(branch.Id, service.Id,
                segment.Id, now.AddMinutes(45), null, currentInputs, performer, CancellationToken.None);
            Assert.True(secondBooking.IsSuccess, string.Join(" | ", secondBooking.Errors));
            var direct = await Repository(db, now.AddMinutes(35)).CreateKioskTicketAsync(
                branch.Id, service.Id, null, currentInputs, performer, CancellationToken.None);
            Assert.True(direct.IsSuccess, string.Join(" | ", direct.Errors));
            await VerifyMigrationBackfillAsync(db, secondBooking.Value.Id, direct.Value.Id,
                archivedReservation.ReservationArchiveId, archivedTicket.TicketArchiveId, replacement.Id, now);
        }
        finally
        {
            // options always target the unique database generated above.
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static async Task VerifyMigrationBackfillAsync(
        PlatformWriteDbContext db, int reservationId, int ticketId,
        long reservationArchiveId, long ticketArchiveId, int currentInputId, DateTime now)
    {
        var reservation = await db.Set<Reservation>().SingleAsync(x => x.Id == reservationId);
        var ticket = await db.Set<Ticket>().SingleAsync(x => x.Id == ticketId);
        var legacy = ReservationCustomInputValue.Create(currentInputId, currentInputId,
            "Legacy", null, null, ServiceCustomInputType.String, "legacy-value", now);
        reservation.AddCustomInput(legacy);
        db.Entry(legacy).Property(x => x.ServiceCustomInputId).CurrentValue = null;
        ticket.AddCustomInput(TicketCustomInputValue.Create(null, null,
            "Legacy", null, null, ServiceCustomInputType.String, "legacy-value", now));
        db.AddRange(
            new ReservationCustomInputValueArchive
            {
                ReservationArchiveId = reservationArchiveId,
                ServiceCustomInputId = currentInputId,
                NameSnapshot = "Current",
                Value = "current-value"
            },
            new TicketCustomInputValueArchive
            {
                TicketArchiveId = ticketArchiveId,
                ServiceCustomInputId = currentInputId,
                NameSnapshot = "Current",
                Value = "current-value"
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await db.GetService<IMigrator>().MigrateAsync("20260909124351_AddTicketPrintConfiguration");
        await db.Database.MigrateAsync();

        var tables = new[]
        {
            await db.Set<ReservationCustomInputValue>().Select(x => new
                { x.ServiceCustomInputId, x.OriginalServiceCustomInputId }).ToArrayAsync(),
            await db.Set<TicketCustomInputValue>().Select(x => new
                { x.ServiceCustomInputId, x.OriginalServiceCustomInputId }).ToArrayAsync(),
            await db.Set<ReservationCustomInputValueArchive>().Select(x => new
                { x.ServiceCustomInputId, x.OriginalServiceCustomInputId }).ToArrayAsync(),
            await db.Set<TicketCustomInputValueArchive>().Select(x => new
                { x.ServiceCustomInputId, x.OriginalServiceCustomInputId }).ToArrayAsync()
        };
        Assert.All(tables, rows =>
        {
            Assert.Equal(2, rows.Length);
            Assert.Equal(currentInputId, Assert.Single(rows, x => x.ServiceCustomInputId.HasValue).OriginalServiceCustomInputId);
            Assert.Null(Assert.Single(rows, x => !x.ServiceCustomInputId.HasValue).OriginalServiceCustomInputId);
        });
    }

    [SqlServerFact]
    public async Task SqlServer_availability_reads_only_the_requested_service_and_its_ancestor_chain()
    {
        var reads = new ServiceReadInterceptor();
        var options = CreateOptions(reads);
        await using var db = new PlatformWriteDbContext(
            options, new TestTenantContext(), new TestCurrentBranchContext());
        var performer = EntityTestFactory.CurrentUserId;
        var now = new DateTime(2026, 9, 1, 15, 30, 0, DateTimeKind.Utc);
        try
        {
            await db.Database.EnsureCreatedAsync();
            db.Add(CreatePerformer());
            var branch = EntityTestFactory.Branch(0);
            var leaf = EntityTestFactory.Service(0, isTicketIssuable: true);
            // Cross SQL Server's default recursion limit and put unrelated rows in the same catalog.
            var ancestors = Enumerable.Range(1, 105).Select(i => EntityTestFactory.Service(
                0, arabicName: $"أصل {i}", englishName: $"Ancestor {i}")).ToArray();
            db.AddRange(branch, leaf);
            db.AddRange(ancestors);
            db.AddRange(Enumerable.Range(1, 500).Select(i => EntityTestFactory.Service(
                0, arabicName: $"غير مرتبط {i}", englishName: $"Unrelated {i}")));
            await db.SaveChangesAsync();
            for (var i = 1; i < ancestors.Length; i++)
                ancestors[i].ChangeParent(ancestors[i - 1].Id, performer);
            leaf.ChangeParent(ancestors[^1].Id, performer);
            db.Add(BranchService.Create(branch.Id, leaf, performer));
            db.Add(EntityTestFactory.ServiceSchedule(0, branch.Id, leaf.Id, timeSlots: new[]
            {
                new ServiceScheduleTimeSlotDefinition(now.DayOfWeek,
                    new TimeOnly(15, 30), new TimeOnly(16, 0))
            }));
            await db.SaveChangesAsync();
            Assert.Equal(606, await db.Set<Service>().CountAsync());

            var checker = new BranchServiceAvailabilityChecker(db);
            async Task<bool> CheckAsync() =>
                (await checker.CheckAsync(branch.Id, leaf.Id, now, CancellationToken.None,
                    requireKioskEligibility: true)).IsAvailable;

            reads.Commands.Clear();
            Assert.True(await CheckAsync());
            Assert.Collection(reads.Commands,
                target =>
                {
                    Assert.Matches(@"WHERE \[\w+\]\.\[Id\] = @", target.Text);
                    Assert.Contains("[ParentServiceId] =", target.Text);
                    Assert.Equal(leaf.Id, Assert.Single(target.Parameters));
                },
                chain =>
                {
                    Assert.Contains("WHERE [Id] = @", chain.Text);
                    Assert.Contains("[parent].[Id] = [ancestor].[ParentServiceId]", chain.Text);
                    Assert.Contains("OPTION (MAXRECURSION 0)", chain.Text);
                    Assert.Equal(ancestors[^1].Id, Assert.Single(chain.Parameters));
                });

            var root = ancestors[0];
            EntityTestFactory.SetServiceActive(root, false);
            await db.SaveChangesAsync();
            Assert.False(await CheckAsync());
            EntityTestFactory.SetServiceActive(root, true);
            root.IsDeleted = true;
            await db.SaveChangesAsync();
            Assert.False(await CheckAsync());
            root.IsDeleted = false;
            root.ChangeParent(ancestors[1].Id, performer);
            await db.SaveChangesAsync();
            Assert.False(await CheckAsync());
            root.ChangeParent(null, performer);
            await db.SaveChangesAsync();
            Assert.True(await CheckAsync());

            var deletedChild = EntityTestFactory.Service(0, parentServiceId: leaf.Id,
                arabicName: "خدمة محذوفة", englishName: "Deleted child");
            deletedChild.IsDeleted = true;
            db.Add(deletedChild);
            await db.SaveChangesAsync();
            Assert.False(await CheckAsync());

            reads.Commands.Clear();
            var standard = await checker.CheckAsync(branch.Id, leaf.Id, now, CancellationToken.None);
            Assert.True(standard.IsAvailable);
            Assert.Single(reads.Commands);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static DbContextOptions<PlatformWriteDbContext> CreateOptions(params IInterceptor[] interceptors)
    {
        var connection = new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable(ConnectionVariable)!)
        {
            InitialCatalog = $"QControl_KioskRegression_{Guid.NewGuid():N}"
        };
        return new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer(connection.ConnectionString)
            .AddInterceptors(interceptors)
            .Options;
    }

    private static ApplicationUser CreatePerformer()
    {
        var performer = EntityTestFactory.CurrentUserId;
        var user = ApplicationUser.Create("kiosk-regression", "kiosk@example.test", "Test",
            null, null, "test-fixture", UserType.TechnicalAdmin, performer);
        EntityTestFactory.AssignId(user, performer);
        return user;
    }

    private sealed class ServiceReadInterceptor : DbCommandInterceptor
    {
        public List<(string Text, object?[] Parameters)> Commands { get; } = new();
        public List<string> HistoricalSnapshotQueries { get; } = new();

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("[ReservationCustomInputValue]", StringComparison.Ordinal))
                HistoricalSnapshotQueries.Add(command.CommandText);
            if (command.CommandText.Contains("FROM [Service]", StringComparison.Ordinal))
            {
                Commands.Add((command.CommandText,
                    command.Parameters.Cast<DbParameter>().Select(x => x.Value).ToArray()));
            }

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
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
