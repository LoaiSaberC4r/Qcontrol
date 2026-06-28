using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;

internal sealed class UnassignWindowFromDisplayCommandValidator
    : AbstractValidator<UnassignWindowFromDisplayCommand>
{
    public UnassignWindowFromDisplayCommandValidator()
    {
        RuleFor(x => x.DisplayId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.DisplayWindow_DisplayId_Required);

        RuleFor(x => x.WindowId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.DisplayWindow_WindowId_Required);
    }
}
