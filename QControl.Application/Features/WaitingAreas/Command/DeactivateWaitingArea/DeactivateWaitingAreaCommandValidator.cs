using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.WaitingAreas.Command.DeactivateWaitingArea;

internal sealed class DeactivateWaitingAreaCommandValidator
    : AbstractValidator<DeactivateWaitingAreaCommand>
{
    public DeactivateWaitingAreaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.WaitingArea_Id_Required);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
