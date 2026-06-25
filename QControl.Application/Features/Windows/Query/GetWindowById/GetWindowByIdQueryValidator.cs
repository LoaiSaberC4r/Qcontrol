using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Windows.Query.GetWindowById;

internal sealed class GetWindowByIdQueryValidator
    : AbstractValidator<GetWindowByIdQuery>
{
    public GetWindowByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Window_Id_Required);
    }
}
