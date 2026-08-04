using Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;

namespace QControl.Application.Tests.Branding;

public sealed class UpdateBranchThemeCommandValidatorTests
{
    private readonly UpdateBranchThemeCommandValidator _validator = new();

    [Fact]
    public void Accepts_complete_valid_layout_with_arabic_text()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("FFF")]
    [InlineData("#FFF")]
    [InlineData("rgb(0,0,0)")]
    [InlineData("   ")]
    public void Rejects_invalid_optional_color(string color)
    {
        var result = _validator.Validate(
            ValidCommand() with { HeaderColor = color });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(UpdateBranchThemeCommand.HeaderColor));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void Rejects_invalid_dimensions(decimal value)
    {
        var result = _validator.Validate(
            ValidCommand() with { ServiceButtonWidth = value });

        Assert.Contains(result.Errors, error =>
            error.PropertyName ==
                nameof(UpdateBranchThemeCommand.ServiceButtonWidth));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Rejects_invalid_spacing(decimal value)
    {
        var result = _validator.Validate(
            ValidCommand() with { ServiceButtonSpace = value });

        Assert.Contains(result.Errors, error =>
            error.PropertyName ==
                nameof(UpdateBranchThemeCommand.ServiceButtonSpace));
    }

    [Fact]
    public void Rejects_button_text_over_200_characters()
    {
        var result = _validator.Validate(
            ValidCommand() with { FooterButtonText = new string('x', 201) });

        Assert.Contains(result.Errors, error =>
            error.PropertyName ==
                nameof(UpdateBranchThemeCommand.FooterButtonText));
    }

    private static UpdateBranchThemeCommand ValidCommand() =>
        new()
        {
            BranchId = 1,
            MainColor = "#0070C4",
            SecondaryColor = "#FFFFFF",
            BackgroundColor = "#FFFFFF",
            HeaderColor = "#FFFFFF",
            FooterColor = "#FFFFFF",
            MainTextColor = "#A7060F",
            ShowLanguagePage = true,
            DefaultLanguageIsArabic = true,
            ShowServiceNavigationPath = true,
            AllowRequestMoreServices = true,
            LanguageButtonBackgroundColor = "#0070C4",
            LanguageButtonTextColor = "#FFFFFF",
            LanguageButtonWidth = 40m,
            LanguageButtonHeight = 22m,
            LanguageButtonText = "اختيار اللغة",
            ServiceButtonBackgroundColor = "#0070C4",
            ServiceButtonTextColor = "#FFFFFF",
            ServiceButtonWidth = 40m,
            ServiceButtonHeight = 20m,
            ServiceButtonSpace = 0m,
            ServiceButtonFontSize = 1.8m,
            ServiceButtonText = "اختيار الخدمة",
            KeypadButtonBackgroundColor = "#0070C4",
            KeypadButtonTextColor = "#FFFFFF",
            KeypadButtonWidth = 45m,
            KeypadButtonHeight = 10m,
            KeypadButtonText = "تأكيد",
            FooterButtonBackgroundColor = "#0070C4",
            FooterButtonTextColor = "#FFFFFF",
            FooterButtonWidth = 10m,
            FooterButtonHeight = 12m,
            FooterButtonText = "رجوع"
        };
}
