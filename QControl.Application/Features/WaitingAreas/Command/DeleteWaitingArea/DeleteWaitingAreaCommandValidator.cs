using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.WaitingAreas.Command.DeleteWaitingArea;

internal sealed class DeleteWaitingAreaCommandValidator
    : AbstractValidator<DeleteWaitingAreaCommand>
{
    public DeleteWaitingAreaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.WaitingArea_Id_Required);
    }
}
