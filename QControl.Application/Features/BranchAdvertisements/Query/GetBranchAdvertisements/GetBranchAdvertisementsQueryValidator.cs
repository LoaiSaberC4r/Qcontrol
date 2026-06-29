using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchAdvertisements.Query.GetBranchAdvertisements;

internal sealed class GetBranchAdvertisementsQueryValidator
    : AbstractValidator<GetBranchAdvertisementsQuery>
{
    public GetBranchAdvertisementsQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);
    }
}
