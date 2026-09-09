using FluentValidation;

namespace QControl.Application.Features.TicketConfigurations.Query.GetTicketConfiguration;

internal sealed class GetTicketConfigurationQueryValidator
    : AbstractValidator<GetTicketConfigurationQuery>
{
    public GetTicketConfigurationQueryValidator() =>
        RuleFor(x => x.BranchId).GreaterThan(0);
}
