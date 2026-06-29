using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Displays.Command.ReactivateDisplay;

internal sealed class ReactivateDisplayCommandValidator
    : AbstractValidator<ReactivateDisplayCommand>
{
    public ReactivateDisplayCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Display_Id_Required);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
