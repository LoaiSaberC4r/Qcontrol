using BuildingBlock.Application.Abstraction;
using QControl.Application.Features.TicketConfigurations.Shared;

namespace QControl.Application.Features.TicketConfigurations.Command.UpdateTicketConfiguration;

public sealed record UpdateTicketConfigurationCommand
    : TicketConfigurationCommandBase, ICommand<TicketConfigurationResponse>
{
    public string RowVersion { get; init; } = string.Empty;
}
