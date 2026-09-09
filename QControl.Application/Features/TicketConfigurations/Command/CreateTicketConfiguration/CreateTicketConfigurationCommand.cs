using BuildingBlock.Application.Abstraction;
using QControl.Application.Features.TicketConfigurations.Shared;

namespace QControl.Application.Features.TicketConfigurations.Command.CreateTicketConfiguration;

public sealed record CreateTicketConfigurationCommand
    : TicketConfigurationCommandBase, ICommand<TicketConfigurationResponse>;
