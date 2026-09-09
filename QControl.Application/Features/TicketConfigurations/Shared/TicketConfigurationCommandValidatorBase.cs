using FluentValidation;

namespace QControl.Application.Features.TicketConfigurations.Shared;

internal abstract class TicketConfigurationCommandValidatorBase<TCommand>
    : AbstractValidator<TCommand>
    where TCommand : TicketConfigurationCommandBase
{
    protected TicketConfigurationCommandValidatorBase(string codePrefix)
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x).Custom((request, context) =>
        {
            var result = TicketPrintLayoutValidator.Validate(
                request.TicketWidthMm, request.TicketHeightMm, request.Elements, codePrefix);
            foreach (var error in result.Errors)
            {
                context.AddFailure(error.Code, error.Message);
            }
        });
    }
}
