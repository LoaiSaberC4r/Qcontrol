using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceSchedules.Command.CreateServiceSchedule;

internal sealed class CreateServiceScheduleCommandValidator
    : AbstractValidator<CreateServiceScheduleCommand>
{
    public CreateServiceScheduleCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceScheduleMessages.BranchIdRequired);

        RuleFor(x => x.LeafServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceScheduleMessages.LeafServiceIdRequired);

        Include(new ServiceScheduleFieldsValidator<CreateServiceScheduleCommand>(
            "Create",
            x => x.Days,
            x => x.IsSlotCodeRequired,
            x => x.SlotCode));
    }
}

internal sealed class ServiceScheduleFieldsValidator<T>
    : AbstractValidator<T>
{
    public ServiceScheduleFieldsValidator(
        string operation,
        Expression<Func<T, IReadOnlyCollection<ServiceScheduleDayCommandItem>?>>
            days,
        Expression<Func<T, bool>> isSlotCodeRequired,
        Expression<Func<T, string?>> slotCode)
    {
        var getDays = days.Compile();
        var getIsSlotCodeRequired = isSlotCodeRequired.Compile();

        RuleFor(days)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(ServiceScheduleMessages.DaysRequired)
            .WithErrorCode(Code(operation, "DaysRequired"))
            .Must(value => value!.Count > 0)
            .WithMessage(ServiceScheduleMessages.DaysRequired)
            .WithErrorCode(Code(operation, "DaysRequired"));

        RuleFor(x => x)
            .Custom((value, context) => ValidateTimeSlots(
                getDays(value),
                operation,
                context));

        RuleFor(slotCode)
            .Must(HaveValidSlotCodeLength)
            .WithMessage(ServiceScheduleMessages.SlotCodeMaxLength);

        RuleFor(slotCode)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage(ServiceScheduleMessages.SlotCodeRequired)
            .When(x => getIsSlotCodeRequired(x));

        RuleFor(slotCode)
            .Must(value => value is null || !string.IsNullOrWhiteSpace(value))
            .WithMessage(ServiceScheduleMessages.SlotCodeInvalid)
            .When(x => !getIsSlotCodeRequired(x));
    }

    private static void ValidateTimeSlots(
        IReadOnlyCollection<ServiceScheduleDayCommandItem>? days,
        string operation,
        ValidationContext<T> context)
    {
        if (days is null)
        {
            return;
        }

        var dayIndex = 0;
        var suppliedDays = new HashSet<DayOfWeek>();

        foreach (var day in days)
        {
            var timeSlots = day?.TimeSlots;
            var dayPath = $"Days[{dayIndex}]";

            if (day is null || !Enum.IsDefined(day.DayOfWeek))
            {
                AddFailure(
                    context,
                    $"{dayPath}.DayOfWeek",
                    operation,
                    "InvalidDay",
                    ServiceScheduleMessages.InvalidDay);
            }
            else if (!suppliedDays.Add(day.DayOfWeek))
            {
                AddFailure(
                    context,
                    $"{dayPath}.DayOfWeek",
                    operation,
                    "DuplicateDay",
                    ServiceScheduleMessages.DuplicateDay);
            }

            if (timeSlots is null || timeSlots.Count == 0)
            {
                AddFailure(
                    context,
                    $"{dayPath}.TimeSlots",
                    operation,
                    "TimeSlotsRequired",
                    ServiceScheduleMessages.TimeSlotsRequired);
                dayIndex++;
                continue;
            }

            var validRanges = new List<(TimeOnly StartTime, TimeOnly EndTime)>();
            var slotIndex = 0;

            foreach (var slot in timeSlots)
            {
                var slotPath = $"{dayPath}.TimeSlots[{slotIndex}]";
                var startTime = slot?.StartTime;
                var endTime = slot?.EndTime;

                if (!startTime.HasValue)
                {
                    AddFailure(
                        context,
                        $"{slotPath}.StartTime",
                        operation,
                        "StartTimeRequired",
                        ServiceScheduleMessages.StartTimeRequired);
                }

                if (!endTime.HasValue)
                {
                    AddFailure(
                        context,
                        $"{slotPath}.EndTime",
                        operation,
                        "EndTimeRequired",
                        ServiceScheduleMessages.EndTimeRequired);
                }

                if (startTime.HasValue && endTime.HasValue)
                {
                    if (startTime.Value >= endTime.Value)
                    {
                        AddFailure(
                            context,
                            slotPath,
                            operation,
                            "InvalidTimeRange",
                            ServiceScheduleMessages.InvalidTimeRange);
                    }
                    else
                    {
                        validRanges.Add((startTime.Value, endTime.Value));
                    }
                }

                slotIndex++;
            }

            ValidateDuplicateAndOverlappingRanges(
                validRanges,
                dayPath,
                operation,
                context);

            dayIndex++;
        }
    }

    private static void ValidateDuplicateAndOverlappingRanges(
        IReadOnlyCollection<(TimeOnly StartTime, TimeOnly EndTime)> ranges,
        string dayPath,
        string operation,
        ValidationContext<T> context)
    {
        var duplicateExists = ranges
            .GroupBy(range => range)
            .Any(group => group.Count() > 1);

        if (duplicateExists)
        {
            AddFailure(
                context,
                $"{dayPath}.TimeSlots",
                operation,
                "DuplicateTimeSlot",
                ServiceScheduleMessages.DuplicateTimeSlot);
        }

        TimeOnly? greatestEndTime = null;

        foreach (var range in ranges
            .Distinct()
            .OrderBy(range => range.StartTime)
            .ThenBy(range => range.EndTime))
        {
            if (greatestEndTime.HasValue &&
                range.StartTime < greatestEndTime.Value)
            {
                AddFailure(
                    context,
                    $"{dayPath}.TimeSlots",
                    operation,
                    "OverlappingTimeSlots",
                    ServiceScheduleMessages.OverlappingTimeSlots);
                break;
            }

            if (!greatestEndTime.HasValue ||
                range.EndTime > greatestEndTime.Value)
            {
                greatestEndTime = range.EndTime;
            }
        }
    }

    private static bool HaveValidSlotCodeLength(string? slotCode)
    {
        return string.IsNullOrWhiteSpace(slotCode) ||
            slotCode.Trim().Length <=
            ServiceScheduleSlotCodeNormalizer.MaxLength;
    }

    private static void AddFailure(
        ValidationContext<T> context,
        string propertyName,
        string operation,
        string code,
        string message)
    {
        context.AddFailure(new ValidationFailure(propertyName, message)
        {
            ErrorCode = Code(operation, code)
        });
    }

    private static string Code(string operation, string code) =>
        $"ServiceSchedules.{operation}.{code}";
}
