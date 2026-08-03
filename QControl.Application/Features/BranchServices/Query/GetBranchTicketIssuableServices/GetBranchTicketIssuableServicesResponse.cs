namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

public sealed record GetBranchTicketIssuableServicesResponse
{
    public int BranchId { get; init; }

    public TicketIssuanceBranchBrandingResponse BranchBranding
        { get; init; } = new();

    public IReadOnlyList<TicketIssuableServiceTreeNodeResponse> Services
        { get; init; } =
        Array.Empty<TicketIssuableServiceTreeNodeResponse>();
}

public sealed record TicketIssuanceBranchBrandingResponse
{
    public string? LogoUrl { get; init; }

    public TicketIssuanceBranchThemeResponse Theme { get; init; } = new();
}

public sealed record TicketIssuanceBranchThemeResponse
{
    public string? MainColor { get; init; }

    public string? SecondaryColor { get; init; }

    public string? BackgroundColor { get; init; }
}

public sealed record TicketIssuableServiceTreeNodeResponse
{
    public int ServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ArabicUserMessage { get; init; }

    public string? EnglishUserMessage { get; init; }

    public bool IsTicketIssuable { get; init; }

    public string? RangePrefix { get; init; }

    public int? RangeStartNumber { get; init; }

    public int? RangeEndNumber { get; init; }

    public IReadOnlyList<TicketIssuableServiceTreeNodeResponse> Children
        { get; init; } =
        Array.Empty<TicketIssuableServiceTreeNodeResponse>();
}
