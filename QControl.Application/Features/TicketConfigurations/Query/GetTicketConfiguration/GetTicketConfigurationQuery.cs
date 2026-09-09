using BuildingBlock.Application.Abstraction;
using QControl.Application.Features.TicketConfigurations.Shared;

namespace QControl.Application.Features.TicketConfigurations.Query.GetTicketConfiguration;

public sealed record GetTicketConfigurationQuery(int BranchId)
    : IQuery<TicketConfigurationResponse>;
