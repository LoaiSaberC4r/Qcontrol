using FluentValidation.Results;
using Qcontrol.Application.Features.ServiceSchedules.Command.CreateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Command.UpdateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Shared;

namespace QControl.Application.Tests.ServiceSchedules;

public sealed class ServiceScheduleValidatorTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_and_update_reject_empty_days(bool update)
    {
        var result = Validate(update, Array.Empty<ServiceScheduleDayCommandItem>());

        AssertFailure(result, ServiceScheduleMessages.DaysRequired);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_and_update_reject_duplicate_days(bool update)
    {
        var result = Validate(update, new[]
        {
            Day(DayOfWeek.Sunday, Slot(8, 12)),
            Day(DayOfWeek.Sunday, Slot(14, 18))
        });

        AssertFailure(result, ServiceScheduleMessages.DuplicateDay);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_and_update_reject_invalid_day(bool update)
    {
        var result = Validate(update, new[]
        {
            Day((DayOfWeek)9, Slot(8, 12))
        });

        AssertFailure(result, ServiceScheduleMessages.InvalidDay);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_and_update_reject_day_without_slots(bool update)
    {
        var result = Validate(update, new[]
        {
            Day(DayOfWeek.Sunday)
        });

        AssertFailure(result, ServiceScheduleMessages.TimeSlotsRequired);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void Create_and_update_reject_missing_slot_boundaries(
        bool update,
        bool missingStart)
    {
        var result = Validate(update, new[]
        {
            Day(DayOfWeek.Sunday, new ServiceScheduleTimeSlotCommandItem
            {
                StartTime = missingStart ? null : new TimeOnly(8, 0),
                EndTime = missingStart ? new TimeOnly(12, 0) : null
            })
        });

        AssertFailure(
            result,
            missingStart
                ? ServiceScheduleMessages.StartTimeRequired
                : ServiceScheduleMessages.EndTimeRequired);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_and_update_reject_invalid_range(bool update)
    {
        var result = Validate(update, new[]
        {
            Day(DayOfWeek.Sunday, Slot(16, 8))
        });

        AssertFailure(result, ServiceScheduleMessages.InvalidTimeRange);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_and_update_reject_duplicate_range_on_same_day(bool update)
    {
        var result = Validate(update, new[]
        {
            Day(DayOfWeek.Sunday, Slot(8, 12), Slot(8, 12))
        });

        AssertFailure(result, ServiceScheduleMessages.DuplicateTimeSlot);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_and_update_reject_overlapping_ranges(bool update)
    {
        var result = Validate(update, new[]
        {
            Day(
                DayOfWeek.Sunday,
                Slot(3, 10),
                Slot(4, 5),
                Slot(6, 7))
        });

        AssertFailure(result, ServiceScheduleMessages.OverlappingTimeSlots);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_and_update_accept_adjacent_ranges_and_same_range_on_other_day(
        bool update)
    {
        var result = Validate(update, new[]
        {
            Day(DayOfWeek.Saturday, Slot(3, 6), Slot(6, 9)),
            Day(DayOfWeek.Sunday, Slot(3, 6))
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_rejects_missing_required_slot_code()
    {
        var result = new CreateServiceScheduleCommandValidator().Validate(
            ValidCreate() with
            {
                IsSlotCodeRequired = true,
                SlotCode = " "
            });

        AssertFailure(result, ServiceScheduleMessages.SlotCodeRequired);
    }

    [Fact]
    public void Create_rejects_whitespace_optional_slot_code()
    {
        var result = new CreateServiceScheduleCommandValidator().Validate(
            ValidCreate() with
            {
                IsSlotCodeRequired = false,
                SlotCode = " "
            });

        AssertFailure(result, ServiceScheduleMessages.SlotCodeInvalid);
    }

    [Fact]
    public void Create_rejects_slot_code_longer_than_100()
    {
        var result = new CreateServiceScheduleCommandValidator().Validate(
            ValidCreate() with { SlotCode = new string('A', 101) });

        AssertFailure(result, ServiceScheduleMessages.SlotCodeMaxLength);
    }

    [Fact]
    public void Update_rejects_missing_or_invalid_row_version()
    {
        var validator = new UpdateServiceScheduleCommandValidator();

        Assert.False(validator.Validate(
            ValidUpdate() with { RowVersion = string.Empty }).IsValid);
        Assert.False(validator.Validate(
            ValidUpdate() with { RowVersion = "not-base64" }).IsValid);
    }

    private static ValidationResult Validate(
        bool update,
        IReadOnlyCollection<ServiceScheduleDayCommandItem> days)
    {
        return update
            ? new UpdateServiceScheduleCommandValidator().Validate(
                ValidUpdate() with { Days = days })
            : new CreateServiceScheduleCommandValidator().Validate(
                ValidCreate() with { Days = days });
    }

    private static CreateServiceScheduleCommand ValidCreate() => new()
    {
        BranchId = 1,
        LeafServiceId = 10,
        Days = new[] { Day(DayOfWeek.Sunday, Slot(8, 16)) },
        IsSlotCodeRequired = false,
        SlotCode = null
    };

    private static UpdateServiceScheduleCommand ValidUpdate() => new()
    {
        BranchId = 1,
        LeafServiceId = 10,
        Days = new[] { Day(DayOfWeek.Sunday, Slot(8, 16)) },
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

    private static void AssertFailure(
        ValidationResult result,
        string expectedMessage)
    {
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == expectedMessage);
    }
}
