namespace Qcontrol.Application.Features.BranchBranding.Shared;

public interface IBrandingLayoutInput
{
    string MainColor { get; }

    string SecondaryColor { get; }

    string BackgroundColor { get; }

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
