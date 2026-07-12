using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.Services.Query.GetServiceSelectionOptions;

internal sealed class GetServiceSelectionOptionsQueryValidator
    : AbstractValidator<GetServiceSelectionOptionsQuery>
{
    public GetServiceSelectionOptionsQueryValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.ParentIdRequired)
            .When(x => x.ServiceId.HasValue);
    }
}
