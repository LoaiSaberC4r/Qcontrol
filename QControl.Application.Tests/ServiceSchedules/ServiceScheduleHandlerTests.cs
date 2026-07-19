using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceSchedules.Command.CreateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Command.UpdateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Query.GetServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.ServiceSchedules;

public sealed class ServiceScheduleHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Create_succeeds_for_assigned_ticket_issuing_leaf_service()
    {
        var fixture = CreateFixture();
        var unitOfWork = new TestUnitOfWork();
        var scheduleWriteRepository =
            new InMemoryWriteRepository<ServiceSchedule>(fixture.Schedules);
        var handler = CreateHandler(fixture, scheduleWriteRepository, unitOfWork);

        var result = await handler.Handle(
            ValidCreateCommand() with
            {
                SlotCode = " med-01 "
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.Schedules);
        Assert.Equal("MED-01", fixture.Schedules[0].SlotCode);
        Assert.Equal(3, fixture.Schedules[0].TimeSlots.Count);
        Assert.Collection(
            result.Value.Days,
            day =>
            {
                Assert.Equal(DayOfWeek.Sunday, day.DayOfWeek);
                Assert.Collection(
                    day.TimeSlots,
                    slot =>
                    {
                        Assert.Equal(new TimeOnly(9, 0), slot.StartTime);
                        Assert.Equal(new TimeOnly(12, 0), slot.EndTime);
                    });
            },
            day =>
            {
                Assert.Equal(DayOfWeek.Saturday, day.DayOfWeek);
                Assert.Collection(
                    day.TimeSlots,
                    slot =>
                    {
                        Assert.Equal(new TimeOnly(8, 0), slot.StartTime);
                        Assert.Equal(new TimeOnly(10, 0), slot.EndTime);
                    },
                    slot =>
                    {
                        Assert.Equal(new TimeOnly(14, 0), slot.StartTime);
                        Assert.Equal(new TimeOnly(18, 0), slot.EndTime);
                    });
            });
        Assert.Equal(1, scheduleWriteRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_rejects_missing_branch()
    {
        var fixture = CreateFixture(branches: new List<Branch>());
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.BranchNotFound");
    }

    [Fact]
    public async Task Create_rejects_inactive_branch()
    {
        var branch = EntityTestFactory.Branch(1);
        branch.Deactivate(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var fixture = CreateFixture(branches: new List<Branch> { branch });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.BranchInactive");
    }

    [Fact]
    public async Task Create_rejects_missing_service()
    {
        var fixture = CreateFixture(services: new List<Service>());
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.ServiceNotFound");
    }

    [Fact]
    public async Task Create_rejects_service_not_assigned_to_branch()
    {
        var fixture = CreateFixture(assignments: new List<BranchService>());
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.ServiceNotAssignedToBranch");
    }

    [Fact]
    public async Task Create_rejects_parent_service()
    {
        var parent = EntityTestFactory.Service(10, isTicketIssuable: true);
        var child = EntityTestFactory.Service(
            11,
            parentServiceId: parent.Id,
            isTicketIssuable: true);
        var fixture = CreateFixture(
            services: new List<Service> { parent, child },
            assignments: new List<BranchService>
            {
                EntityTestFactory.BranchService(1, 1, parent.Id)
            });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.ServiceIsNotLeaf");
    }

    [Fact]
    public async Task Create_rejects_non_ticket_issuing_service()
    {
        var service = EntityTestFactory.Service(
            10,
            isTicketIssuable: false);
        var fixture = CreateFixture(services: new List<Service> { service });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.ServiceNotTicketIssuable");
    }

    [Fact]
    public async Task Create_rejects_deleted_service()
    {
        var service = EntityTestFactory.Service(10, isTicketIssuable: true);
        service.SoftDelete(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var fixture = CreateFixture(services: new List<Service> { service });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.ServiceDeleted");
    }

    [Fact]
    public async Task Create_rejects_inactive_service()
    {
        var service = EntityTestFactory.Service(10, isTicketIssuable: true);
        EntityTestFactory.SetServiceActive(service, false);
        var fixture = CreateFixture(services: new List<Service> { service });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.ServiceInactive");
    }

    [Fact]
    public async Task Create_rejects_effectively_inactive_service()
    {
        var parent = EntityTestFactory.Service(9, isTicketIssuable: false);
        EntityTestFactory.SetServiceActive(parent, false);
        var child = EntityTestFactory.Service(
            10,
            parentServiceId: parent.Id,
            isTicketIssuable: true);
        var fixture = CreateFixture(
            services: new List<Service> { parent, child });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.ServiceNotEffectivelyActive");
    }

    [Fact]
    public async Task Create_rejects_duplicate_schedule()
    {
        var fixture = CreateFixture(schedules: new List<ServiceSchedule>
        {
            EntityTestFactory.ServiceSchedule(20, 1, 10)
        });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.ScheduleAlreadyExists");
    }

    [Fact]
    public async Task Create_treats_slot_code_case_insensitively()
    {
        var fixture = CreateFixture(schedules: new List<ServiceSchedule>
        {
            EntityTestFactory.ServiceSchedule(
                20,
                1,
                11,
                slotCode: "MED-01")
        });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand() with
            {
                SlotCode = " med-01 "
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Create.SlotCodeAlreadyExists");
    }

    [Fact]
    public async Task Create_allows_same_slot_code_in_another_branch()
    {
        var fixture = CreateFixture(schedules: new List<ServiceSchedule>
        {
            EntityTestFactory.ServiceSchedule(
                20,
                2,
                10,
                slotCode: "MED-01")
        });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand() with
            {
                SlotCode = "MED-01"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, fixture.Schedules.Count);
    }

    [Fact]
    public async Task Create_allows_multiple_null_slot_codes_in_same_branch()
    {
        var fixture = CreateFixture(schedules: new List<ServiceSchedule>
        {
            EntityTestFactory.ServiceSchedule(20, 1, 11, slotCode: null)
        });
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(
            ValidCreateCommand(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, fixture.Schedules.Count);
    }

    [Fact]
    public async Task Update_succeeds_and_replaces_all_time_slots()
    {
        var schedule = EntityTestFactory.ServiceSchedule(
            20,
            1,
            10,
            timeSlots: new[]
            {
                Definition(DayOfWeek.Sunday, 8, 12),
                Definition(DayOfWeek.Monday, 8, 12)
            });
        var fixture = CreateFixture(schedules: new List<ServiceSchedule>
        {
            schedule
        });
        var scheduleWriteRepository =
            new InMemoryWriteRepository<ServiceSchedule>(fixture.Schedules);
        var tokenManager = new TestConcurrencyTokenManager();
        var handler = UpdateHandler(
            fixture,
            scheduleWriteRepository: scheduleWriteRepository,
            concurrencyTokenManager: tokenManager);

        var result = await handler.Handle(
            ValidUpdateCommand() with
            {
                Days = new[]
                {
                    Day(
                        DayOfWeek.Tuesday,
                        Slot(8, 10),
                        Slot(14, 18)),
                    Day(DayOfWeek.Wednesday, Slot(9, 12))
                }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[]
            {
                (DayOfWeek.Tuesday, new TimeOnly(8, 0), new TimeOnly(10, 0)),
                (DayOfWeek.Tuesday, new TimeOnly(14, 0), new TimeOnly(18, 0)),
                (DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(12, 0))
            },
            schedule.TimeSlots.Select(x =>
                (x.DayOfWeek, x.StartTime, x.EndTime)));
        Assert.Equal(1, scheduleWriteRepository.UpdateCallCount);
        Assert.Equal(1, tokenManager.SetOriginalRowVersionCallCount);
    }

    [Fact]
    public async Task Update_rejects_missing_schedule()
    {
        var fixture = CreateFixture();
        var handler = UpdateHandler(fixture);

        var result = await handler.Handle(
            ValidUpdateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Update.ScheduleNotFound");
    }

    [Fact]
    public async Task Update_maps_invalid_time_slots_without_mutating_or_saving()
    {
        var schedule = EntityTestFactory.ServiceSchedule(20, 1, 10);
        var fixture = CreateFixture(schedules: new List<ServiceSchedule>
        {
            schedule
        });
        var unitOfWork = new TestUnitOfWork();
        var handler = UpdateHandler(fixture, unitOfWork: unitOfWork);

        var result = await handler.Handle(
            ValidUpdateCommand() with
            {
                Days = new[]
                {
                    Day(
                        DayOfWeek.Monday,
                        Slot(8, 12),
                        Slot(10, 14))
                }
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Update.InvalidSchedule");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Collection(
            schedule.TimeSlots,
            slot =>
            {
                Assert.Equal(DayOfWeek.Sunday, slot.DayOfWeek);
                Assert.Equal(new TimeOnly(8, 0), slot.StartTime);
                Assert.Equal(new TimeOnly(16, 0), slot.EndTime);
            });
    }

    [Fact]
    public async Task Update_rejects_duplicate_slot_code()
    {
        var schedule = EntityTestFactory.ServiceSchedule(
            20,
            1,
            10,
            slotCode: "MED-01");
        var other = EntityTestFactory.ServiceSchedule(
            21,
            1,
            11,
            slotCode: "LAB-01");
        var fixture = CreateFixture(schedules: new List<ServiceSchedule>
        {
            schedule,
            other
        });
        var handler = UpdateHandler(fixture);

        var result = await handler.Handle(
            ValidUpdateCommand() with
            {
                SlotCode = " lab-01 "
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Update.SlotCodeAlreadyExists");
    }

    [Fact]
    public async Task Update_rejects_stale_row_version()
    {
        var schedule = EntityTestFactory.ServiceSchedule(20, 1, 10);
        var fixture = CreateFixture(schedules: new List<ServiceSchedule>
        {
            schedule
        });
        var handler = UpdateHandler(
            fixture,
            unitOfWork: new TestUnitOfWork
            {
                SaveChangesException = new DbUpdateConcurrencyException()
            });

        var result = await handler.Handle(
            ValidUpdateCommand(),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Errors,
            error => error.Code ==
                "ServiceSchedules.Update.ConcurrencyConflict");
    }

    [Fact]
    public async Task Get_returns_stored_schedule_when_unassigned_as_unavailable()
    {
        var schedule = EntityTestFactory.ServiceSchedule(20, 1, 10);
        var fixture = CreateFixture(
            assignments: new List<BranchService>(),
            schedules: new List<ServiceSchedule> { schedule });
        var handler = GetHandler(fixture);

        var result = await handler.Handle(
            new GetServiceScheduleQuery
            {
                BranchId = 1,
                LeafServiceId = 10
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsCurrentlyOperational);
        Assert.Collection(
            result.Value.Days,
            day =>
            {
                Assert.Equal(DayOfWeek.Sunday, day.DayOfWeek);
                Assert.Collection(
                    day.TimeSlots,
                    slot =>
                    {
                        Assert.Equal(new TimeOnly(8, 0), slot.StartTime);
                        Assert.Equal(new TimeOnly(16, 0), slot.EndTime);
                    });
            });
    }

    private static CreateServiceScheduleCommand ValidCreateCommand()
        => new()
        {
            BranchId = 1,
            LeafServiceId = 10,
            Days = new[]
            {
                Day(
                    DayOfWeek.Saturday,
                    Slot(14, 18),
                    Slot(8, 10)),
                Day(DayOfWeek.Sunday, Slot(9, 12))
            },
            IsSlotCodeRequired = false,
            SlotCode = null
        };

    private static UpdateServiceScheduleCommand ValidUpdateCommand()
        => new()
        {
            BranchId = 1,
            LeafServiceId = 10,
            Days = new[] { Day(DayOfWeek.Monday, Slot(9, 14)) },
            IsSlotCodeRequired = false,
            SlotCode = null,
            RowVersion = ValidRowVersion
        };

    private static ServiceScheduleDayCommandItem Day(
        DayOfWeek day,
        params ServiceScheduleTimeSlotCommandItem[] timeSlots) => new()
        {
            DayOfWeek = day,
            TimeSlots = timeSlots
        };

    private static ServiceScheduleTimeSlotCommandItem Slot(
        int startHour,
        int endHour) => new()
        {
            StartTime = new TimeOnly(startHour, 0),
            EndTime = new TimeOnly(endHour, 0)
        };

    private static ServiceScheduleTimeSlotDefinition Definition(
        DayOfWeek day,
        int startHour,
        int endHour) => new(
            day,
            new TimeOnly(startHour, 0),
            new TimeOnly(endHour, 0));

    private static CreateServiceScheduleCommandHandler CreateHandler(
        Fixture fixture,
        InMemoryWriteRepository<ServiceSchedule>? scheduleWriteRepository = null,
        TestUnitOfWork? unitOfWork = null)
        => new(
            new InMemoryWriteReadRepository<ServiceSchedule>(fixture.Schedules),
            scheduleWriteRepository ??
                new InMemoryWriteRepository<ServiceSchedule>(fixture.Schedules),
            new InMemoryWriteReadRepository<Service>(fixture.Services),
            new InMemoryWriteReadRepository<Branch>(fixture.Branches),
            new InMemoryWriteReadRepository<BranchService>(fixture.Assignments),
            new TestCurrentUser(),
            new TestBranchAccessValidator(),
            unitOfWork ?? new TestUnitOfWork());

    private static UpdateServiceScheduleCommandHandler UpdateHandler(
        Fixture fixture,
        InMemoryWriteRepository<ServiceSchedule>? scheduleWriteRepository = null,
        TestConcurrencyTokenManager? concurrencyTokenManager = null,
        TestUnitOfWork? unitOfWork = null)
        => new(
            new InMemoryWriteReadRepository<ServiceSchedule>(fixture.Schedules),
            scheduleWriteRepository ??
                new InMemoryWriteRepository<ServiceSchedule>(fixture.Schedules),
            new InMemoryWriteReadRepository<Service>(fixture.Services),
            new InMemoryWriteReadRepository<Branch>(fixture.Branches),
            new InMemoryWriteReadRepository<BranchService>(fixture.Assignments),
            concurrencyTokenManager ?? new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            new TestBranchAccessValidator(),
            new TestDateTimeProvider(),
            unitOfWork ?? new TestUnitOfWork());

    private static GetServiceScheduleQueryHandler GetHandler(Fixture fixture)
        => new(
            new InMemoryWriteReadRepository<ServiceSchedule>(fixture.Schedules),
            new InMemoryWriteReadRepository<Service>(fixture.Services),
            new InMemoryWriteReadRepository<Branch>(fixture.Branches),
            new InMemoryWriteReadRepository<BranchService>(fixture.Assignments),
            new TestCurrentUser(),
            new TestBranchAccessValidator(),
            new TestServiceVisibilityPolicy());

    private static Fixture CreateFixture(
        List<Branch>? branches = null,
        List<Service>? services = null,
        List<BranchService>? assignments = null,
        List<ServiceSchedule>? schedules = null)
    {
        services ??= new List<Service>
        {
            EntityTestFactory.Service(10, isTicketIssuable: true)
        };

        assignments ??= new List<BranchService>
        {
            EntityTestFactory.BranchService(1, 1, 10)
        };

        return new Fixture(
            branches ?? new List<Branch>
            {
                EntityTestFactory.Branch(1)
            },
            services,
            assignments,
            schedules ?? new List<ServiceSchedule>());
    }

    private sealed record Fixture(
        List<Branch> Branches,
        List<Service> Services,
        List<BranchService> Assignments,
        List<ServiceSchedule> Schedules);
}
