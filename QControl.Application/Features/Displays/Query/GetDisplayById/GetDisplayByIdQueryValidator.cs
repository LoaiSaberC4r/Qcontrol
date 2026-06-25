using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Displays.Query.GetDisplayById;

internal sealed class GetDisplayByIdQueryValidator
    : AbstractValidator<GetDisplayByIdQuery>
{
    public GetDisplayByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Display_Id_Required);
    }
}
