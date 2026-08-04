using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.GeneralBrand.Shared;

internal static class GeneralBrandResponseFactory
{
    public static GeneralBrandResponse FromEntity(
        QControl.Domain.Entities.GeneralBrand generalBrand,
        string? message = null)
    {
        return new GeneralBrandResponse
        {
            Id = generalBrand.Id,
            MainColor = generalBrand.MainColor,
            SecondaryColor = generalBrand.SecondaryColor,
            BackgroundColor = generalBrand.BackgroundColor,
            HeaderColor = generalBrand.HeaderColor,
            FooterColor = generalBrand.FooterColor,
            MainTextColor = generalBrand.MainTextColor,
            ShowLanguagePage = generalBrand.ShowLanguagePage,
            DefaultLanguageIsArabic =
                generalBrand.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = generalBrand.AlwaysRequireUserInput,
            ShowServiceNavigationPath =
                generalBrand.ShowServiceNavigationPath,
            AllowOperatorSelection = generalBrand.AllowOperatorSelection,
            AllowRequestMoreServices =
                generalBrand.AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                generalBrand.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = generalBrand.LanguageButtonTextColor,
            LanguageButtonWidth = generalBrand.LanguageButtonWidth,
            LanguageButtonHeight = generalBrand.LanguageButtonHeight,
            LanguageButtonText = generalBrand.LanguageButtonText,
            ServiceButtonBackgroundColor =
                generalBrand.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = generalBrand.ServiceButtonTextColor,
            ServiceButtonWidth = generalBrand.ServiceButtonWidth,
            ServiceButtonHeight = generalBrand.ServiceButtonHeight,
            ServiceButtonSpace = generalBrand.ServiceButtonSpace,
            ServiceButtonFontSize = generalBrand.ServiceButtonFontSize,
            ServiceButtonText = generalBrand.ServiceButtonText,
            KeypadButtonBackgroundColor =
                generalBrand.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = generalBrand.KeypadButtonTextColor,
            KeypadButtonWidth = generalBrand.KeypadButtonWidth,
            KeypadButtonHeight = generalBrand.KeypadButtonHeight,
            KeypadButtonText = generalBrand.KeypadButtonText,
            FooterButtonBackgroundColor =
                generalBrand.FooterButtonBackgroundColor,
            FooterButtonTextColor = generalBrand.FooterButtonTextColor,
            FooterButtonWidth = generalBrand.FooterButtonWidth,
            FooterButtonHeight = generalBrand.FooterButtonHeight,
            FooterButtonText = generalBrand.FooterButtonText,
            IsConfigured = true,
            RowVersion = RowVersionConverter.ToBase64(generalBrand.RowVersion),
            Message = message
        };
    }

    public static GeneralBrandResponse NotConfigured() => new();
}
