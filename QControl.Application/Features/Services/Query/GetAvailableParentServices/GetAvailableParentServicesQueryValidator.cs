using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Services.Query.GetAvailableParentServices;

internal sealed class GetAvailableParentServicesQueryValidator
    : AbstractValidator<GetAvailableParentServicesQuery>
{
    public GetAvailableParentServicesQueryValidator()
    {
        RuleFor(x => x.ExcludeServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired)
            .When(x => x.ExcludeServiceId.HasValue);

        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .WithMessage(ErrorMessage.SearchTerm_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
    }
}
