using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;

internal sealed class PermanentDeleteWindowCommandValidator
    : AbstractValidator<PermanentDeleteWindowCommand>
{
    public PermanentDeleteWindowCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Window_Id_Required);
    }
}
