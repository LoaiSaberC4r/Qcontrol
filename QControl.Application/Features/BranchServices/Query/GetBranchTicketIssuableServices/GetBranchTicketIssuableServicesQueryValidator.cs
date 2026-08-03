using FluentValidation;

namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

internal sealed class GetBranchTicketIssuableServicesQueryValidator
    : AbstractValidator<GetBranchTicketIssuableServicesQuery>
{
    public GetBranchTicketIssuableServicesQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(BranchTicketIssuableServicesMessages.BranchIdRequired);
    }
}
