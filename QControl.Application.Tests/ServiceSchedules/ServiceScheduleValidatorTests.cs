using Qcontrol.Application.Features.ServiceSchedules.Command.CreateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Command.UpdateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Shared;

namespace QControl.Application.Tests.ServiceSchedules;

public sealed class ServiceScheduleValidatorTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public void Create_rejects_missing_start_time()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                StartTime = null
            });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Create_rejects_missing_end_time()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                EndTime = null
            });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Create_rejects_invalid_time_range()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                StartTime = new TimeOnly(16, 0),
                EndTime = new TimeOnly(8, 0)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceScheduleMessages.InvalidTimeRange);
    }

    [Fact]
    public void Create_rejects_empty_work_days()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                WorkDays = Array.Empty<DayOfWeek>()
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceScheduleMessages.WorkDaysRequired);
    }

    [Fact]
    public void Create_rejects_duplicate_work_days()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                WorkDays = new[]
                {
                    DayOfWeek.Sunday,
                    DayOfWeek.Sunday
                }
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceScheduleMessages.DuplicateWorkDay);
    }

    [Fact]
    public void Create_rejects_invalid_work_day()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                WorkDays = new[] { (DayOfWeek)9 }
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceScheduleMessages.InvalidWorkDay);
    }

    [Fact]
    public void Create_rejects_missing_required_slot_code()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                IsSlotCodeRequired = true,
                SlotCode = " "
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceScheduleMessages.SlotCodeRequired);
    }

    [Fact]
    public void Create_allows_null_optional_slot_code()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                IsSlotCodeRequired = false,
                SlotCode = null
            });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_rejects_whitespace_optional_slot_code()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                IsSlotCodeRequired = false,
                SlotCode = " "
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceScheduleMessages.SlotCodeInvalid);
    }

    [Fact]
    public void Create_rejects_slot_code_longer_than_100()
    {
        var result = new CreateServiceScheduleCommandValidator()
            .Validate(ValidCreate() with
            {
                SlotCode = new string('A', 101)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ServiceScheduleMessages.SlotCodeMaxLength);
    }

    [Fact]
    public void Update_rejects_missing_row_version()
    {
        var result = new UpdateServiceScheduleCommandValidator()
            .Validate(ValidUpdate() with
            {
                RowVersion = string.Empty
            });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Update_rejects_invalid_row_version()
    {
        var result = new UpdateServiceScheduleCommandValidator()
            .Validate(ValidUpdate() with
            {
                RowVersion = "not-base64"
            });

        Assert.False(result.IsValid);
    }

    private static CreateServiceScheduleCommand ValidCreate()
        => new()
        {
            BranchId = 1,
            LeafServiceId = 10,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            WorkDays = new[] { DayOfWeek.Sunday },
            IsSlotCodeRequired = false,
            SlotCode = null
        };

    private static UpdateServiceScheduleCommand ValidUpdate()
        => new()
        {
            BranchId = 1,
            LeafServiceId = 10,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0),
            WorkDays = new[] { DayOfWeek.Sunday },
            IsSlotCodeRequired = false,
            SlotCode = null,
            RowVersion = ValidRowVersion
        };
}
