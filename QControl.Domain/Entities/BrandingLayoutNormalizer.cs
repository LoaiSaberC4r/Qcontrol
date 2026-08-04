namespace QControl.Domain.Entities;

internal static class BrandingLayoutNormalizer
{
    public static BrandingLayoutSettings Normalize(
        BrandingLayoutSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings with
        {
            MainColor = NormalizeRequiredColor(settings.MainColor),
            SecondaryColor = NormalizeRequiredColor(settings.SecondaryColor),
            BackgroundColor = NormalizeRequiredColor(settings.BackgroundColor),
            HeaderColor = NormalizeOptionalColor(settings.HeaderColor),
            FooterColor = NormalizeOptionalColor(settings.FooterColor),
            MainTextColor = NormalizeOptionalColor(settings.MainTextColor),
            LanguageButtonBackgroundColor = NormalizeOptionalColor(
                settings.LanguageButtonBackgroundColor),
            LanguageButtonTextColor = NormalizeOptionalColor(
                settings.LanguageButtonTextColor),
            LanguageButtonText = NormalizeOptionalText(
                settings.LanguageButtonText),
            ServiceButtonBackgroundColor = NormalizeOptionalColor(
                settings.ServiceButtonBackgroundColor),
            ServiceButtonTextColor = NormalizeOptionalColor(
                settings.ServiceButtonTextColor),
            ServiceButtonText = NormalizeOptionalText(
                settings.ServiceButtonText),
            KeypadButtonBackgroundColor = NormalizeOptionalColor(
                settings.KeypadButtonBackgroundColor),
            KeypadButtonTextColor = NormalizeOptionalColor(
                settings.KeypadButtonTextColor),
            KeypadButtonText = NormalizeOptionalText(
                settings.KeypadButtonText),
            FooterButtonBackgroundColor = NormalizeOptionalColor(
                settings.FooterButtonBackgroundColor),
            FooterButtonTextColor = NormalizeOptionalColor(
                settings.FooterButtonTextColor),
            FooterButtonText = NormalizeOptionalText(
                settings.FooterButtonText)
        };
    }

    private static string NormalizeRequiredColor(string value) =>
        value.Trim().ToUpperInvariant();

    private static string? NormalizeOptionalColor(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : NormalizeRequiredColor(value);

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
