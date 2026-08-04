using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchBranding.Shared;

internal static class BrandingLayoutSettingsFactory
{
    public static BrandingLayoutSettings FromInput(
        IBrandingLayoutInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new BrandingLayoutSettings
        {
            MainColor = input.MainColor,
            SecondaryColor = input.SecondaryColor,
            BackgroundColor = input.BackgroundColor,
            HeaderColor = input.HeaderColor,
            FooterColor = input.FooterColor,
            MainTextColor = input.MainTextColor,
            ShowLanguagePage = input.ShowLanguagePage,
            DefaultLanguageIsArabic = input.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = input.AlwaysRequireUserInput,
            ShowServiceNavigationPath = input.ShowServiceNavigationPath,
            AllowOperatorSelection = input.AllowOperatorSelection,
            AllowRequestMoreServices = input.AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                input.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = input.LanguageButtonTextColor,
            LanguageButtonWidth = input.LanguageButtonWidth,
            LanguageButtonHeight = input.LanguageButtonHeight,
            LanguageButtonText = input.LanguageButtonText,
            ServiceButtonBackgroundColor =
                input.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = input.ServiceButtonTextColor,
            ServiceButtonWidth = input.ServiceButtonWidth,
            ServiceButtonHeight = input.ServiceButtonHeight,
            ServiceButtonSpace = input.ServiceButtonSpace,
            ServiceButtonFontSize = input.ServiceButtonFontSize,
            ServiceButtonText = input.ServiceButtonText,
            KeypadButtonBackgroundColor =
                input.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = input.KeypadButtonTextColor,
            KeypadButtonWidth = input.KeypadButtonWidth,
            KeypadButtonHeight = input.KeypadButtonHeight,
            KeypadButtonText = input.KeypadButtonText,
            FooterButtonBackgroundColor =
                input.FooterButtonBackgroundColor,
            FooterButtonTextColor = input.FooterButtonTextColor,
            FooterButtonWidth = input.FooterButtonWidth,
            FooterButtonHeight = input.FooterButtonHeight,
            FooterButtonText = input.FooterButtonText
        };
    }
}
