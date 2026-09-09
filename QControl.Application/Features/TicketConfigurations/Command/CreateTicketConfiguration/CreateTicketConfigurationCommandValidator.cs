using QControl.Application.Features.TicketConfigurations.Shared;

namespace QControl.Application.Features.TicketConfigurations.Command.CreateTicketConfiguration;

internal sealed class CreateTicketConfigurationCommandValidator
    : TicketConfigurationCommandValidatorBase<CreateTicketConfigurationCommand>
{
    public CreateTicketConfigurationCommandValidator()
        : base("TicketConfigurations.Create")
    {
    }
}
