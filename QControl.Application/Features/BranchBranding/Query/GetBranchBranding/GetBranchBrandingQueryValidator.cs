using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchBranding.Query.GetBranchBranding;

internal sealed class GetBranchBrandingQueryValidator
    : AbstractValidator<GetBranchBrandingQuery>
{
    public GetBranchBrandingQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);
    }
}
