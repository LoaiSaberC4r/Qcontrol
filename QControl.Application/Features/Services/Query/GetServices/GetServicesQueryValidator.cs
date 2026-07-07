using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Services.Query.GetServices;

internal sealed class GetServicesQueryValidator
    : AbstractValidator<GetServicesQuery>
{
    public GetServicesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ServiceFeatureMessages.PageNumberInvalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ServiceFeatureMessages.PageSizeInvalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ServiceFeatureMessages.PageSizeMax);

        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .WithMessage(ErrorMessage.SearchTerm_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));

        RuleFor(x => x.ParentServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.ParentIdRequired)
            .When(x => x.ParentServiceId.HasValue);
    }
}
