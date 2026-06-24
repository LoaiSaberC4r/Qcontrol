using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreaById;

internal sealed class GetWaitingAreaByIdQueryValidator
    : AbstractValidator<GetWaitingAreaByIdQuery>
{
    public GetWaitingAreaByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.WaitingArea_Id_Required);
    }
}
