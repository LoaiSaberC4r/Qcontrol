namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

internal interface ITicketIssuanceBrandingLayout
{
    string? MainColor { get; }

    string? SecondaryColor { get; }

    string? BackgroundColor { get; }

    string? HeaderColor { get; }

    string? FooterColor { get; }

    string? MainTextColor { get; }

    bool ShowLanguagePage { get; }

    bool DefaultLanguageIsArabic { get; }

    bool AlwaysRequireUserInput { get; }

    bool ShowServiceNavigationPath { get; }

    bool AllowOperatorSelection { get; }

    bool AllowRequestMoreServices { get; }

    string? LanguageButtonBackgroundColor { get; }

    string? LanguageButtonTextColor { get; }

    decimal? LanguageButtonWidth { get; }

    decimal? LanguageButtonHeight { get; }

    string? LanguageButtonText { get; }

    string? ServiceButtonBackgroundColor { get; }

    string? ServiceButtonTextColor { get; }

    decimal? ServiceButtonWidth { get; }

    decimal? ServiceButtonHeight { get; }

    decimal? ServiceButtonSpace { get; }

    decimal? ServiceButtonFontSize { get; }

    string? ServiceButtonText { get; }

    string? KeypadButtonBackgroundColor { get; }

    string? KeypadButtonTextColor { get; }

    decimal? KeypadButtonWidth { get; }

    decimal? KeypadButtonHeight { get; }

    string? KeypadButtonText { get; }

    string? FooterButtonBackgroundColor { get; }

    string? FooterButtonTextColor { get; }

    decimal? FooterButtonWidth { get; }

    decimal? FooterButtonHeight { get; }

    string? FooterButtonText { get; }
}

internal static class TicketIssuanceBrandingResponseFactory
{
    public static TicketIssuanceBranchBrandingResponse FromLayout(
        ITicketIssuanceBrandingLayout layout,
        string? logoUrl)
    {
        return new TicketIssuanceBranchBrandingResponse
        {
            LogoUrl = logoUrl,
            Theme = new TicketIssuanceBranchThemeResponse
            {
                MainColor = layout.MainColor,
                SecondaryColor = layout.SecondaryColor,
                BackgroundColor = layout.BackgroundColor,
                HeaderColor = layout.HeaderColor,
                FooterColor = layout.FooterColor,
                MainTextColor = layout.MainTextColor
            },
            Behavior = new TicketIssuanceBranchBehaviorResponse
            {
                ShowLanguagePage = layout.ShowLanguagePage,
                DefaultLanguageIsArabic = layout.DefaultLanguageIsArabic,
                AlwaysRequireUserInput = layout.AlwaysRequireUserInput,
                ShowServiceNavigationPath = layout.ShowServiceNavigationPath,
                AllowOperatorSelection = layout.AllowOperatorSelection,
                AllowRequestMoreServices = layout.AllowRequestMoreServices
            },
            LanguageButton = new TicketIssuanceButtonResponse
            {
                BackgroundColor = layout.LanguageButtonBackgroundColor,
                TextColor = layout.LanguageButtonTextColor,
                Width = layout.LanguageButtonWidth,
                Height = layout.LanguageButtonHeight,
                Text = layout.LanguageButtonText
            },
            ServiceButton = new TicketIssuanceServiceButtonResponse
            {
                BackgroundColor = layout.ServiceButtonBackgroundColor,
                TextColor = layout.ServiceButtonTextColor,
                Width = layout.ServiceButtonWidth,
                Height = layout.ServiceButtonHeight,
                Space = layout.ServiceButtonSpace,
                FontSize = layout.ServiceButtonFontSize,
                Text = layout.ServiceButtonText
            },
            KeypadButton = new TicketIssuanceButtonResponse
            {
                BackgroundColor = layout.KeypadButtonBackgroundColor,
                TextColor = layout.KeypadButtonTextColor,
                Width = layout.KeypadButtonWidth,
                Height = layout.KeypadButtonHeight,
                Text = layout.KeypadButtonText
            },
            FooterButton = new TicketIssuanceButtonResponse
            {
                BackgroundColor = layout.FooterButtonBackgroundColor,
                TextColor = layout.FooterButtonTextColor,
                Width = layout.FooterButtonWidth,
                Height = layout.FooterButtonHeight,
                Text = layout.FooterButtonText
            }
        };
    }
}
