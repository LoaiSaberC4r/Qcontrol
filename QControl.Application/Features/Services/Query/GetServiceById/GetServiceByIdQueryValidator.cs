using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.Services.Query.GetServiceById;

internal sealed class GetServiceByIdQueryValidator
    : AbstractValidator<GetServiceByIdQuery>
{
    public GetServiceByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired);
    }
}
