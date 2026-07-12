namespace Qcontrol.Application.Features.Services.Query.GetServiceSelectionOptions;

public sealed class GetServiceSelectionOptionsResponse
{
    public int? ParentServiceId { get; init; }

    public IReadOnlyList<ServiceSelectionOptionResponse> Items { get; init; } =
        Array.Empty<ServiceSelectionOptionResponse>();
}
