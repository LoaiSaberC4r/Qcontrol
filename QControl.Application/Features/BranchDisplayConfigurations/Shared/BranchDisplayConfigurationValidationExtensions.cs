using FluentValidation;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

internal static class BranchDisplayConfigurationValidationExtensions
{
    public static void AddDisplayConfigurationRules<T>(this AbstractValidator<T> validator)
        where T : IBranchDisplayConfigurationInput
    {
        AddColorRule(validator, x => x.DisplayBackgroundColor);
        AddTextRule(validator, x => x.MainTitleAr);
        AddTextRule(validator, x => x.MainTitleEn);
        AddColorRule(validator, x => x.HeaderBackgroundColor);
        AddColorRule(validator, x => x.MainTitleTextColor);
        AddFontRule(validator, x => x.MainTitleFontSize);
        AddColorRule(validator, x => x.TableHeaderBackgroundColor);
        AddColorRule(validator, x => x.TableHeaderTextColor);
        AddColorRule(validator, x => x.TableRowBackgroundColor);
        AddColorRule(validator, x => x.TableRowTextColor);
        AddColorRule(validator, x => x.TicketNumberBackgroundColor);
        AddColorRule(validator, x => x.TicketNumberTextColor);
        AddTextRule(validator, x => x.TicketColumnTitleAr);
        AddTextRule(validator, x => x.TicketColumnTitleEn);
        AddTextRule(validator, x => x.ServiceColumnTitleAr);
        AddTextRule(validator, x => x.ServiceColumnTitleEn);
        AddTextRule(validator, x => x.WindowColumnTitleAr);
        AddTextRule(validator, x => x.WindowColumnTitleEn);
        AddColorRule(validator, x => x.TickerBackgroundColor);
        AddColorRule(validator, x => x.TickerTextColor);
        AddFontRule(validator, x => x.TickerFontSize);
        AddColorRule(validator, x => x.ClockBackgroundColor);
        AddColorRule(validator, x => x.ClockTextColor);
    }

    private static void AddColorRule<T>(
        AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, string>> expression)
        where T : IBranchDisplayConfigurationInput
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .WithMessage(BranchDisplayFeatureMessages.ColorRequired)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .WithMessage(BranchDisplayFeatureMessages.InvalidColor);
    }

    private static void AddTextRule<T>(
        AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, string>> expression)
        where T : IBranchDisplayConfigurationInput
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .WithMessage(BranchDisplayFeatureMessages.TitleRequired)
            .MaximumLength(200)
            .WithMessage(BranchDisplayFeatureMessages.TitleMaxLength);
    }

    private static void AddFontRule<T>(
        AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, int>> expression)
        where T : IBranchDisplayConfigurationInput
    {
        validator.RuleFor(expression)
            .InclusiveBetween(1, 100)
            .WithMessage(BranchDisplayFeatureMessages.InvalidFontSize);
    }
}
