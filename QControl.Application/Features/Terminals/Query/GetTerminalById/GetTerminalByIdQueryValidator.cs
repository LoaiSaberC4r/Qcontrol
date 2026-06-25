using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Terminals.Query.GetTerminalById;

internal sealed class GetTerminalByIdQueryValidator
    : AbstractValidator<GetTerminalByIdQuery>
{
    public GetTerminalByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_Id_Required);
    }
}
