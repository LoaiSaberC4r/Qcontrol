using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.ServiceSchedules;

public sealed class ServiceScheduleDomainTests
{
    [Fact]
    public void Create_normalizes_slot_code()
    {
        var schedule = CreateSchedule(
            isSlotCodeRequired: true,
            slotCode: " med-01 ");

        Assert.Equal("MED-01", schedule.SlotCode);
    }

    [Fact]
    public void Create_allows_null_slot_code_when_optional()
    {
        var schedule = CreateSchedule(
            isSlotCodeRequired: false,
            slotCode: null);

        Assert.Null(schedule.SlotCode);
    }

    [Fact]
    public void Create_rejects_missing_slot_code_when_required()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSchedule(
                isSlotCodeRequired: true,
                slotCode: " "));
    }

    [Fact]
    public void Create_rejects_start_time_equal_to_end_time()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSchedule(
                startTime: new TimeOnly(8, 0),
                endTime: new TimeOnly(8, 0)));
    }

    [Fact]
    public void Create_rejects_start_time_after_end_time()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSchedule(
                startTime: new TimeOnly(22, 0),
                endTime: new TimeOnly(6, 0)));
    }

    [Fact]
    public void Create_rejects_empty_work_days()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSchedule(workDays: Array.Empty<DayOfWeek>()));
    }

    [Fact]
    public void Create_rejects_duplicate_work_days()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSchedule(workDays: new[]
            {
                DayOfWeek.Sunday,
                DayOfWeek.Sunday
            }));
    }

    [Fact]
    public void Is_available_at_returns_true_during_working_hours()
    {
        var schedule = CreateSchedule();

        var available = schedule.IsAvailableAt(
            DayOfWeek.Sunday,
            new TimeOnly(15, 59));

        Assert.True(available);
    }

    [Fact]
    public void Is_available_at_returns_false_before_start_time()
    {
        var schedule = CreateSchedule();

        var available = schedule.IsAvailableAt(
            DayOfWeek.Sunday,
            new TimeOnly(7, 59));

        Assert.False(available);
    }

    [Fact]
    public void Is_available_at_returns_false_exactly_at_end_time()
    {
        var schedule = CreateSchedule();

        var available = schedule.IsAvailableAt(
            DayOfWeek.Sunday,
            new TimeOnly(16, 0));

        Assert.False(available);
    }

    [Fact]
    public void Is_available_at_returns_false_on_non_working_day()
    {
        var schedule = CreateSchedule();

        var available = schedule.IsAvailableAt(
            DayOfWeek.Monday,
            new TimeOnly(10, 0));

        Assert.False(available);
    }

    [Fact]
    public void Update_replaces_working_days()
    {
        var schedule = CreateSchedule();
        var modifiedBy = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var modifiedOnUtc = new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);

        schedule.Update(
            new TimeOnly(9, 0),
            new TimeOnly(14, 0),
            new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday },
            isSlotCodeRequired: false,
            slotCode: null,
            modifiedBy,
            modifiedOnUtc);

        Assert.Equal(
            new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday },
            schedule.WorkDays.Select(x => x.DayOfWeek));
        Assert.Equal(modifiedBy, schedule.LastModifiedByApplicationUserId);
        Assert.Equal(modifiedOnUtc, schedule.ModifiedOnUtc);
    }

    private static ServiceSchedule CreateSchedule(
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        IReadOnlyCollection<DayOfWeek>? workDays = null,
        bool isSlotCodeRequired = false,
        string? slotCode = null)
    {
        return ServiceSchedule.Create(
            branchId: 1,
            serviceId: 10,
            startTime ?? new TimeOnly(8, 0),
            endTime ?? new TimeOnly(16, 0),
            workDays ?? new[] { DayOfWeek.Sunday },
            isSlotCodeRequired,
            slotCode,
            EntityTestFactory.CurrentUserId);
    }
}
