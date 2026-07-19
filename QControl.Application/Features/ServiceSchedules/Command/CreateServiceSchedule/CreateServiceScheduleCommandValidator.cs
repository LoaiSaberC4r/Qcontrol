using FluentValidation;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using QControl.Domain.Entities;
using System.Linq.Expressions;

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
            x => x.StartTime,
            x => x.EndTime,
            x => x.WorkDays,
            x => x.IsSlotCodeRequired,
            x => x.SlotCode));
    }
}

internal sealed class ServiceScheduleFieldsValidator<T>
    : AbstractValidator<T>
{
    public ServiceScheduleFieldsValidator(
        Expression<Func<T, TimeOnly?>> startTime,
        Expression<Func<T, TimeOnly?>> endTime,
        Expression<Func<T, IReadOnlyCollection<DayOfWeek>?>> workDays,
        Expression<Func<T, bool>> isSlotCodeRequired,
        Expression<Func<T, string?>> slotCode)
    {
        RuleFor(startTime)
            .NotNull()
            .WithMessage(ServiceScheduleMessages.InvalidTimeRange);

        RuleFor(endTime)
            .NotNull()
            .WithMessage(ServiceScheduleMessages.InvalidTimeRange);

        RuleFor(x => x)
            .Must(x => HasValidTimeRange(
                startTime.Compile()(x),
                endTime.Compile()(x)))
            .WithMessage(ServiceScheduleMessages.InvalidTimeRange);

        RuleFor(workDays)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(ServiceScheduleMessages.WorkDaysRequired)
            .Must(x => x!.Count > 0)
            .WithMessage(ServiceScheduleMessages.WorkDaysRequired)
            .Must(HaveValidWorkDays)
            .WithMessage(ServiceScheduleMessages.InvalidWorkDay)
            .Must(HaveDistinctWorkDays)
            .WithMessage(ServiceScheduleMessages.DuplicateWorkDay);

        RuleFor(slotCode)
            .Must(HaveValidSlotCodeLength)
            .WithMessage(ServiceScheduleMessages.SlotCodeMaxLength);

        RuleFor(slotCode)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage(ServiceScheduleMessages.SlotCodeRequired)
            .When(x => isSlotCodeRequired.Compile()(x));

        RuleFor(slotCode)
            .Must(value => value is null || !string.IsNullOrWhiteSpace(value))
            .WithMessage(ServiceScheduleMessages.SlotCodeInvalid)
            .When(x => !isSlotCodeRequired.Compile()(x));
    }

    private static bool HasValidTimeRange(
        TimeOnly? startTime,
        TimeOnly? endTime)
    {
        return startTime.HasValue &&
            endTime.HasValue &&
            startTime.Value < endTime.Value;
    }

    private static bool HaveValidWorkDays(
        IReadOnlyCollection<DayOfWeek>? workDays)
    {
        return workDays is not null &&
            workDays.All(day => Enum.IsDefined(day));
    }

    private static bool HaveDistinctWorkDays(
        IReadOnlyCollection<DayOfWeek>? workDays)
    {
        return workDays is not null &&
            workDays.Select(day => (int)day).Distinct().Count() ==
            workDays.Count;
    }

    private static bool HaveValidSlotCodeLength(string? slotCode)
    {
        return string.IsNullOrWhiteSpace(slotCode) ||
            slotCode.Trim().Length <=
            ServiceScheduleSlotCodeNormalizer.MaxLength;
    }
}
