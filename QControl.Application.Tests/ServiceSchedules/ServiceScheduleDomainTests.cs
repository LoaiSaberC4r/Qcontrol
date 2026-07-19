using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.ServiceSchedules;

public sealed class ServiceScheduleDomainTests
{
    [Fact]
    public void Create_supports_multiple_slots_and_days_and_sorts_them()
    {
        var schedule = CreateSchedule(new[]
        {
            Slot(DayOfWeek.Tuesday, 14, 18),
            Slot(DayOfWeek.Sunday, 9, 12),
            Slot(DayOfWeek.Tuesday, 8, 10),
            Slot(DayOfWeek.Sunday, 8, 9)
        });

        Assert.Collection(
            schedule.TimeSlots,
            slot => AssertSlot(slot, DayOfWeek.Sunday, 8, 9),
            slot => AssertSlot(slot, DayOfWeek.Sunday, 9, 12),
            slot => AssertSlot(slot, DayOfWeek.Tuesday, 8, 10),
            slot => AssertSlot(slot, DayOfWeek.Tuesday, 14, 18));
    }

    [Fact]
    public void Create_accepts_adjacent_slots()
    {
        var schedule = CreateSchedule(new[]
        {
            Slot(DayOfWeek.Saturday, 3, 6),
            Slot(DayOfWeek.Saturday, 6, 9)
        });

        Assert.Equal(2, schedule.TimeSlots.Count);
    }

    [Fact]
    public void Create_accepts_same_range_on_different_days()
    {
        var schedule = CreateSchedule(new[]
        {
            Slot(DayOfWeek.Saturday, 3, 6),
            Slot(DayOfWeek.Sunday, 3, 6)
        });

        Assert.Equal(2, schedule.TimeSlots.Count);
    }

    [Theory]
    [MemberData(nameof(InvalidRanges))]
    public void Create_rejects_invalid_or_overlapping_ranges(
        IReadOnlyCollection<ServiceScheduleTimeSlotDefinition> timeSlots)
    {
        Assert.Throws<ArgumentException>(() => CreateSchedule(timeSlots));
    }

    [Fact]
    public void Create_rejects_empty_collection()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSchedule(Array.Empty<ServiceScheduleTimeSlotDefinition>()));
    }

    [Fact]
    public void Create_rejects_invalid_day()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSchedule(new[] { Slot((DayOfWeek)9, 8, 12) }));
    }

    [Fact]
    public void Create_normalizes_slot_code()
    {
        var schedule = CreateSchedule(
            new[] { Slot(DayOfWeek.Sunday, 8, 12) },
            isSlotCodeRequired: true,
            slotCode: " med-01 ");

        Assert.Equal("MED-01", schedule.SlotCode);
    }

    [Fact]
    public void Create_rejects_missing_required_slot_code()
    {
        Assert.Throws<ArgumentException>(() => CreateSchedule(
            new[] { Slot(DayOfWeek.Sunday, 8, 12) },
            isSlotCodeRequired: true,
            slotCode: " "));
    }

    [Theory]
    [InlineData(8, 0, true)]
    [InlineData(9, 30, true)]
    [InlineData(10, 0, false)]
    [InlineData(11, 0, false)]
    [InlineData(14, 0, true)]
    [InlineData(16, 0, false)]
    public void Is_available_at_uses_any_slot_with_end_exclusive_semantics(
        int hour,
        int minute,
        bool expected)
    {
        var schedule = CreateSchedule(new[]
        {
            Slot(DayOfWeek.Sunday, 8, 10),
            Slot(DayOfWeek.Sunday, 14, 16)
        });

        Assert.Equal(
            expected,
            schedule.IsAvailableAt(
                DayOfWeek.Sunday,
                new TimeOnly(hour, minute)));
    }

    [Fact]
    public void Update_fully_replaces_time_slots()
    {
        var schedule = CreateSchedule(new[]
        {
            Slot(DayOfWeek.Sunday, 8, 12),
            Slot(DayOfWeek.Monday, 8, 12)
        });
        var modifiedBy = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var modifiedOnUtc = new DateTime(
            2026,
            7,
            16,
            12,
            0,
            0,
            DateTimeKind.Utc);

        schedule.Update(
            new[]
            {
                Slot(DayOfWeek.Saturday, 3, 6),
                Slot(DayOfWeek.Saturday, 9, 12)
            },
            isSlotCodeRequired: false,
            slotCode: null,
            modifiedByApplicationUserId: modifiedBy,
            modifiedOnUtc: modifiedOnUtc);

        Assert.Collection(
            schedule.TimeSlots,
            slot => AssertSlot(slot, DayOfWeek.Saturday, 3, 6),
            slot => AssertSlot(slot, DayOfWeek.Saturday, 9, 12));
        Assert.Equal(modifiedBy, schedule.LastModifiedByApplicationUserId);
        Assert.Equal(modifiedOnUtc, schedule.ModifiedOnUtc);
    }

    [Fact]
    public void Failed_update_does_not_partially_mutate_schedule()
    {
        var schedule = CreateSchedule(
            new[] { Slot(DayOfWeek.Sunday, 8, 12) },
            isSlotCodeRequired: true,
            slotCode: "ORIGINAL");

        Assert.Throws<ArgumentException>(() => schedule.Update(
            new[]
            {
                Slot(DayOfWeek.Tuesday, 8, 12),
                Slot(DayOfWeek.Tuesday, 10, 14)
            },
            isSlotCodeRequired: false,
            slotCode: null,
            modifiedByApplicationUserId: EntityTestFactory.CurrentUserId,
            modifiedOnUtc: DateTime.UtcNow));

        Assert.Equal("ORIGINAL", schedule.SlotCode);
        Assert.Null(schedule.LastModifiedByApplicationUserId);
        Assert.Collection(
            schedule.TimeSlots,
            slot => AssertSlot(slot, DayOfWeek.Sunday, 8, 12));
    }

    public static IEnumerable<object[]> InvalidRanges()
    {
        yield return new object[]
        {
            new[] { Slot(DayOfWeek.Sunday, 8, 8) }
        };
        yield return new object[]
        {
            new[] { Slot(DayOfWeek.Sunday, 22, 6) }
        };
        yield return new object[]
        {
            new[]
            {
                Slot(DayOfWeek.Sunday, 3, 6),
                Slot(DayOfWeek.Sunday, 5, 9)
            }
        };
        yield return new object[]
        {
            new[]
            {
                Slot(DayOfWeek.Sunday, 3, 10),
                Slot(DayOfWeek.Sunday, 5, 7)
            }
        };
        yield return new object[]
        {
            new[]
            {
                Slot(DayOfWeek.Sunday, 3, 6),
                Slot(DayOfWeek.Sunday, 3, 6)
            }
        };
        yield return new object[]
        {
            new[]
            {
                Slot(DayOfWeek.Sunday, 3, 10),
                Slot(DayOfWeek.Sunday, 4, 5),
                Slot(DayOfWeek.Sunday, 6, 7)
            }
        };
    }

    private static ServiceSchedule CreateSchedule(
        IReadOnlyCollection<ServiceScheduleTimeSlotDefinition> timeSlots,
        bool isSlotCodeRequired = false,
        string? slotCode = null)
    {
        return ServiceSchedule.Create(
            branchId: 1,
            serviceId: 10,
            timeSlots,
            isSlotCodeRequired,
            slotCode,
            EntityTestFactory.CurrentUserId);
    }

    private static ServiceScheduleTimeSlotDefinition Slot(
        DayOfWeek day,
        int startHour,
        int endHour) =>
        new(day, new TimeOnly(startHour, 0), new TimeOnly(endHour, 0));

    private static void AssertSlot(
        ServiceScheduleTimeSlot slot,
        DayOfWeek day,
        int startHour,
        int endHour)
    {
        Assert.Equal(day, slot.DayOfWeek);
        Assert.Equal(new TimeOnly(startHour, 0), slot.StartTime);
        Assert.Equal(new TimeOnly(endHour, 0), slot.EndTime);
    }
}
