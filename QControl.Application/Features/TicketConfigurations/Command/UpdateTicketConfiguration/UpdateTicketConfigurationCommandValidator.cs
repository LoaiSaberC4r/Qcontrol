using FluentValidation;
using QControl.Application.Features.TicketConfigurations.Shared;
using QControl.Application.Shared.Operational;

namespace QControl.Application.Features.TicketConfigurations.Command.UpdateTicketConfiguration;

internal sealed class UpdateTicketConfigurationCommandValidator
    : TicketConfigurationCommandValidatorBase<UpdateTicketConfigurationCommand>
{
    public UpdateTicketConfigurationCommandValidator()
        : base("TicketConfigurations.Update")
    {
        RuleFor(x => x.RowVersion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(value => RowVersionConverter.TryDecode(value, out _));
    }
}
