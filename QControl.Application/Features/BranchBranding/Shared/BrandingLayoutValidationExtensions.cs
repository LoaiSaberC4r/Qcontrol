using FluentValidation;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchBranding.Shared;

internal static class BrandingLayoutValidationExtensions
{
    public static void AddBrandingLayoutRules<T>(
        this AbstractValidator<T> validator,
        BrandingLayoutValidationMessages? messages = null)
        where T : IBrandingLayoutInput
    {
        messages ??= BrandingLayoutValidationMessages.ForBranchBranding();

        AddRequiredColorRule(validator, x => x.MainColor, messages);
        AddRequiredColorRule(validator, x => x.SecondaryColor, messages);
        AddRequiredColorRule(validator, x => x.BackgroundColor, messages);

        AddOptionalColorRule(validator, x => x.HeaderColor, messages);
        AddOptionalColorRule(validator, x => x.FooterColor, messages);
        AddOptionalColorRule(validator, x => x.MainTextColor, messages);
        AddOptionalColorRule(
            validator,
            x => x.LanguageButtonBackgroundColor,
            messages);
        AddOptionalColorRule(
            validator,
            x => x.LanguageButtonTextColor,
            messages);
        AddOptionalColorRule(
            validator,
            x => x.ServiceButtonBackgroundColor,
            messages);
        AddOptionalColorRule(
            validator,
            x => x.ServiceButtonTextColor,
            messages);
        AddOptionalColorRule(
            validator,
            x => x.KeypadButtonBackgroundColor,
            messages);
        AddOptionalColorRule(
            validator,
            x => x.KeypadButtonTextColor,
            messages);
        AddOptionalColorRule(
            validator,
            x => x.FooterButtonBackgroundColor,
            messages);
        AddOptionalColorRule(
            validator,
            x => x.FooterButtonTextColor,
            messages);

        AddPositivePercentageRule(
            validator,
            x => x.LanguageButtonWidth,
            messages);
        AddPositivePercentageRule(
            validator,
            x => x.LanguageButtonHeight,
            messages);
        AddPositivePercentageRule(
            validator,
            x => x.ServiceButtonWidth,
            messages);
        AddPositivePercentageRule(
            validator,
            x => x.ServiceButtonHeight,
            messages);
        AddSpacingRule(validator, x => x.ServiceButtonSpace, messages);
        AddPositivePercentageRule(
            validator,
            x => x.ServiceButtonFontSize,
            messages);
        AddPositivePercentageRule(
            validator,
            x => x.KeypadButtonWidth,
            messages);
        AddPositivePercentageRule(
            validator,
            x => x.KeypadButtonHeight,
            messages);
        AddPositivePercentageRule(
            validator,
            x => x.FooterButtonWidth,
            messages);
        AddPositivePercentageRule(
            validator,
            x => x.FooterButtonHeight,
            messages);

        AddTextRule(validator, x => x.LanguageButtonText, messages);
        AddTextRule(validator, x => x.ServiceButtonText, messages);
        AddTextRule(validator, x => x.KeypadButtonText, messages);
        AddTextRule(validator, x => x.FooterButtonText, messages);
    }

    private static void AddRequiredColorRule<T>(
        AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, string>> property,
        BrandingLayoutValidationMessages messages)
        where T : IBrandingLayoutInput
    {
        validator.RuleFor(property)
            .NotEmpty()
            .WithMessage(messages.InvalidColor)
            .Must(value =>
                BranchBrandingColorNormalizer.TryNormalize(value, out _))
            .WithMessage(messages.InvalidColor);
    }

    private static void AddOptionalColorRule<T>(
        AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, string?>> property,
        BrandingLayoutValidationMessages messages)
        where T : IBrandingLayoutInput
    {
        validator.RuleFor(property)
            .Must(value => value is null ||
                BranchBrandingColorNormalizer.TryNormalize(value, out _))
            .WithMessage(messages.InvalidColor);
    }

    private static void AddPositivePercentageRule<T>(
        AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, decimal?>> property,
        BrandingLayoutValidationMessages messages)
        where T : IBrandingLayoutInput
    {
        validator.RuleFor(property)
            .Must(value => value is null || value is > 0 and <= 100)
            .WithMessage(messages.InvalidDimension);
    }

    private static void AddSpacingRule<T>(
        AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, decimal?>> property,
        BrandingLayoutValidationMessages messages)
        where T : IBrandingLayoutInput
    {
        validator.RuleFor(property)
            .Must(value => value is null || value is >= 0 and <= 100)
            .WithMessage(messages.InvalidSpacing);
    }

    private static void AddTextRule<T>(
        AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, string?>> property,
        BrandingLayoutValidationMessages messages)
        where T : IBrandingLayoutInput
    {
        validator.RuleFor(property)
            .MaximumLength(200)
            .WithMessage(messages.ButtonTextTooLong);
    }
}

internal sealed record BrandingLayoutValidationMessages(
    string InvalidColor,
    string InvalidDimension,
    string InvalidSpacing,
    string ButtonTextTooLong)
{
    public static BrandingLayoutValidationMessages ForBranchBranding() =>
        new(
            BranchFeatureMessages.InvalidColor,
            BranchFeatureMessages.InvalidDimension,
            BranchFeatureMessages.InvalidSpacing,
            BranchFeatureMessages.ButtonTextTooLong);
}
