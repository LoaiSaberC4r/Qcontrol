namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

using Qcontrol.Application.Features.Services.Shared;

public sealed record GetBranchTicketIssuableServicesResponse
{
    public int BranchId { get; init; }

    public TicketIssuanceBranchBrandingResponse? BranchBranding
        { get; init; }

    public IReadOnlyList<TicketIssuableServiceTreeNodeResponse> Services
        { get; init; } =
        Array.Empty<TicketIssuableServiceTreeNodeResponse>();
}

public sealed record TicketIssuanceBranchBrandingResponse
{
    public string? LogoUrl { get; init; }

    public TicketIssuanceBranchThemeResponse Theme { get; init; } = new();

    public TicketIssuanceBranchBehaviorResponse Behavior { get; init; } = new();

    public TicketIssuanceButtonResponse LanguageButton { get; init; } = new();

    public TicketIssuanceServiceButtonResponse ServiceButton
        { get; init; } = new();

    public TicketIssuanceButtonResponse KeypadButton { get; init; } = new();

    public TicketIssuanceButtonResponse FooterButton { get; init; } = new();
}

public sealed record TicketIssuanceBranchThemeResponse
{
    public string? MainColor { get; init; }

    public string? SecondaryColor { get; init; }

    public string? BackgroundColor { get; init; }

    public string? HeaderColor { get; init; }

    public string? FooterColor { get; init; }

    public string? MainTextColor { get; init; }
}

public sealed record TicketIssuanceBranchBehaviorResponse
{
    public bool ShowLanguagePage { get; init; }

    public bool DefaultLanguageIsArabic { get; init; }

    public bool AlwaysRequireUserInput { get; init; }

    public bool ShowServiceNavigationPath { get; init; }

    public bool AllowOperatorSelection { get; init; }

    public bool AllowRequestMoreServices { get; init; }
}

public record TicketIssuanceButtonResponse
{
    public string? BackgroundColor { get; init; }

    public string? TextColor { get; init; }

    public decimal? Width { get; init; }

    public decimal? Height { get; init; }

    public string? Text { get; init; }
}

public sealed record TicketIssuanceServiceButtonResponse
    : TicketIssuanceButtonResponse
{
    public decimal? Space { get; init; }

    public decimal? FontSize { get; init; }
}

public sealed record TicketIssuableServiceTreeNodeResponse
{
    public int ServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ArabicUserMessage { get; init; }

    public string? EnglishUserMessage { get; init; }

    public bool IsTicketIssuable { get; init; }

    public bool IsClientInputRequired { get; init; }

    public IReadOnlyList<ServiceCustomInputResponse>? CustomInputs { get; init; }

    public string? RangePrefix { get; init; }

    public int? RangeStartNumber { get; init; }

    public int? RangeEndNumber { get; init; }

    public IReadOnlyList<TicketIssuableServiceTreeNodeResponse> Children
        { get; init; } =
        Array.Empty<TicketIssuableServiceTreeNodeResponse>();
}
