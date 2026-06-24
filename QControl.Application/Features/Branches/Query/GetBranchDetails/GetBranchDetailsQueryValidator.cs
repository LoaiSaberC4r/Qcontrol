using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchDetails;

internal sealed class GetBranchDetailsQueryValidator
    : AbstractValidator<GetBranchDetailsQuery>
{
    public GetBranchDetailsQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);
    }
}