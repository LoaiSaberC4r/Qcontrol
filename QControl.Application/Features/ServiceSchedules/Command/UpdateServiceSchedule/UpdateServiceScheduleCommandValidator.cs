using FluentValidation;
using Qcontrol.Application.Features.ServiceSchedules.Command.CreateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceSchedules.Command.UpdateServiceSchedule;

internal sealed class UpdateServiceScheduleCommandValidator
    : AbstractValidator<UpdateServiceScheduleCommand>
{
    public UpdateServiceScheduleCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceScheduleMessages.BranchIdRequired);

        RuleFor(x => x.LeafServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceScheduleMessages.LeafServiceIdRequired);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);

        Include(new ServiceScheduleFieldsValidator<UpdateServiceScheduleCommand>(
            "Update",
            x => x.Days,
            x => x.IsSlotCodeRequired,
            x => x.SlotCode));
    }
}
