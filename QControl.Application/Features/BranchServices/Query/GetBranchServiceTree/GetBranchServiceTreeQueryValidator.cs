using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchServices.Query.GetBranchServiceTree;

internal sealed class GetBranchServiceTreeQueryValidator
    : AbstractValidator<GetBranchServiceTreeQuery>
{
    public GetBranchServiceTreeQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.BranchNotFound);

        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .WithMessage(ErrorMessage.SearchTerm_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
    }
}
