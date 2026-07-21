using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Services.Query.GetServicesTree;

internal sealed class GetServicesTreeQueryValidator
    : AbstractValidator<GetServicesTreeQuery>
{
    public GetServicesTreeQueryValidator()
    {
        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .WithMessage(ErrorMessage.SearchTerm_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
    }
}
