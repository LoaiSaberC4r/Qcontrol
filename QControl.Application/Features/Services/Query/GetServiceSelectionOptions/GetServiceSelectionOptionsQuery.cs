using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Services.Query.GetServiceSelectionOptions;

public sealed record GetServiceSelectionOptionsQuery
    : IQuery<GetServiceSelectionOptionsResponse>
{
    public int? ServiceId { get; init; }
}
