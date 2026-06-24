using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;

internal sealed class CreateWaitingAreaCommandValidator
    : AbstractValidator<CreateWaitingAreaCommand>
{
    public CreateWaitingAreaCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.WaitingArea_BranchId_Required);

        RuleFor(x => x.Number)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.WaitingArea_Number_Required);

        RuleFor(x => x.AudioDevice)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.WaitingArea_AudioDevice_MaxLength);

        RuleFor(x => x.ControlDevice)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.WaitingArea_ControlDevice_MaxLength);

        RuleFor(x => x.DescriptiveName)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.WaitingArea_DescriptiveName_MaxLength);
    }
}
