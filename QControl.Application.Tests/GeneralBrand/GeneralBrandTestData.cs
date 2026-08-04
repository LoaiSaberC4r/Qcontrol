using Qcontrol.Application.Features.GeneralBrand.Command.CreateGeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.GeneralBranding;

internal static class GeneralBrandTestData
{
    public static BrandingLayoutSettings Layout() =>
        new()
        {
            MainColor = "#0070c4",
            SecondaryColor = "#ffffff",
            BackgroundColor = "#f0f0f0",
            HeaderColor = "#111111",
            FooterColor = "#222222",
            MainTextColor = "#a7060f",
            ShowLanguagePage = true,
            DefaultLanguageIsArabic = true,
            AlwaysRequireUserInput = true,
            ShowServiceNavigationPath = true,
            AllowOperatorSelection = true,
            AllowRequestMoreServices = true,
            LanguageButtonBackgroundColor = "#333333",
            LanguageButtonTextColor = "#444444",
            LanguageButtonWidth = 40m,
            LanguageButtonHeight = 22m,
            LanguageButtonText = "  اختيار اللغة  ",
            ServiceButtonBackgroundColor = "#555555",
            ServiceButtonTextColor = "#666666",
            ServiceButtonWidth = 40m,
            ServiceButtonHeight = 20m,
            ServiceButtonSpace = 2m,
            ServiceButtonFontSize = 1.8m,
            ServiceButtonText = "  اختيار الخدمة  ",
            KeypadButtonBackgroundColor = "#777777",
            KeypadButtonTextColor = "#888888",
            KeypadButtonWidth = 45m,
            KeypadButtonHeight = 10m,
            KeypadButtonText = "  تأكيد  ",
            FooterButtonBackgroundColor = "#999999",
            FooterButtonTextColor = "#aaaaaa",
            FooterButtonWidth = 10m,
            FooterButtonHeight = 12m,
            FooterButtonText = "  رجوع  "
        };

    public static CreateGeneralBrandCommand CreateCommand()
    {
        var layout = Layout();
        return new CreateGeneralBrandCommand
        {
            MainColor = layout.MainColor,
            SecondaryColor = layout.SecondaryColor,
            BackgroundColor = layout.BackgroundColor,
            HeaderColor = layout.HeaderColor,
            FooterColor = layout.FooterColor,
            MainTextColor = layout.MainTextColor,
            ShowLanguagePage = layout.ShowLanguagePage,
            DefaultLanguageIsArabic = layout.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = layout.AlwaysRequireUserInput,
            ShowServiceNavigationPath = layout.ShowServiceNavigationPath,
            AllowOperatorSelection = layout.AllowOperatorSelection,
            AllowRequestMoreServices = layout.AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                layout.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = layout.LanguageButtonTextColor,
            LanguageButtonWidth = layout.LanguageButtonWidth,
            LanguageButtonHeight = layout.LanguageButtonHeight,
            LanguageButtonText = layout.LanguageButtonText,
            ServiceButtonBackgroundColor =
                layout.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = layout.ServiceButtonTextColor,
            ServiceButtonWidth = layout.ServiceButtonWidth,
            ServiceButtonHeight = layout.ServiceButtonHeight,
            ServiceButtonSpace = layout.ServiceButtonSpace,
            ServiceButtonFontSize = layout.ServiceButtonFontSize,
            ServiceButtonText = layout.ServiceButtonText,
            KeypadButtonBackgroundColor =
                layout.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = layout.KeypadButtonTextColor,
            KeypadButtonWidth = layout.KeypadButtonWidth,
            KeypadButtonHeight = layout.KeypadButtonHeight,
            KeypadButtonText = layout.KeypadButtonText,
            FooterButtonBackgroundColor =
                layout.FooterButtonBackgroundColor,
            FooterButtonTextColor = layout.FooterButtonTextColor,
            FooterButtonWidth = layout.FooterButtonWidth,
            FooterButtonHeight = layout.FooterButtonHeight,
            FooterButtonText = layout.FooterButtonText
        };
    }

    public static UpdateGeneralBrandCommand UpdateCommand() =>
        new()
        {
            MainColor = CreateCommand().MainColor,
            SecondaryColor = CreateCommand().SecondaryColor,
            BackgroundColor = CreateCommand().BackgroundColor,
            HeaderColor = CreateCommand().HeaderColor,
            FooterColor = CreateCommand().FooterColor,
            MainTextColor = CreateCommand().MainTextColor,
            ShowLanguagePage = CreateCommand().ShowLanguagePage,
            DefaultLanguageIsArabic = CreateCommand().DefaultLanguageIsArabic,
            AlwaysRequireUserInput = CreateCommand().AlwaysRequireUserInput,
            ShowServiceNavigationPath =
                CreateCommand().ShowServiceNavigationPath,
            AllowOperatorSelection = CreateCommand().AllowOperatorSelection,
            AllowRequestMoreServices = CreateCommand().AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                CreateCommand().LanguageButtonBackgroundColor,
            LanguageButtonTextColor = CreateCommand().LanguageButtonTextColor,
            LanguageButtonWidth = CreateCommand().LanguageButtonWidth,
            LanguageButtonHeight = CreateCommand().LanguageButtonHeight,
            LanguageButtonText = CreateCommand().LanguageButtonText,
            ServiceButtonBackgroundColor =
                CreateCommand().ServiceButtonBackgroundColor,
            ServiceButtonTextColor = CreateCommand().ServiceButtonTextColor,
            ServiceButtonWidth = CreateCommand().ServiceButtonWidth,
            ServiceButtonHeight = CreateCommand().ServiceButtonHeight,
            ServiceButtonSpace = CreateCommand().ServiceButtonSpace,
            ServiceButtonFontSize = CreateCommand().ServiceButtonFontSize,
            ServiceButtonText = CreateCommand().ServiceButtonText,
            KeypadButtonBackgroundColor =
                CreateCommand().KeypadButtonBackgroundColor,
            KeypadButtonTextColor = CreateCommand().KeypadButtonTextColor,
            KeypadButtonWidth = CreateCommand().KeypadButtonWidth,
            KeypadButtonHeight = CreateCommand().KeypadButtonHeight,
            KeypadButtonText = CreateCommand().KeypadButtonText,
            FooterButtonBackgroundColor =
                CreateCommand().FooterButtonBackgroundColor,
            FooterButtonTextColor = CreateCommand().FooterButtonTextColor,
            FooterButtonWidth = CreateCommand().FooterButtonWidth,
            FooterButtonHeight = CreateCommand().FooterButtonHeight,
            FooterButtonText = CreateCommand().FooterButtonText,
            RowVersion = Convert.ToBase64String(
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })
        };
}
