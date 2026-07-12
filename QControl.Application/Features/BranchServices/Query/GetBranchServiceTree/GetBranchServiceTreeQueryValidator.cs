using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServices.Query.GetBranchServiceTree;

internal sealed class GetBranchServiceTreeQueryValidator
    : AbstractValidator<GetBranchServiceTreeQuery>
{
    public GetBranchServiceTreeQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.BranchNotFound);
    }
}
