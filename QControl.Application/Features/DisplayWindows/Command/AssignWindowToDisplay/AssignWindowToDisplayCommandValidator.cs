using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;

internal sealed class AssignWindowToDisplayCommandValidator
    : AbstractValidator<AssignWindowToDisplayCommand>
{
    public AssignWindowToDisplayCommandValidator()
    {
        RuleFor(x => x.DisplayId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.DisplayWindow_DisplayId_Required);

        RuleFor(x => x.WindowId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.DisplayWindow_WindowId_Required);
    }
}
