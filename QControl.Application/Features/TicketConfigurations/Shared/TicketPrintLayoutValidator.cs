using BuildingBlock.Domain.Results;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Features.TicketConfigurations.Shared;

public static class TicketPrintLayoutValidator
{
    private const decimal MaximumLayoutValue = 9_999_999.99m;
    private const decimal MaximumFontSize = 9_999.99m;
    private static readonly TicketPrintElementType[] RequiredTypes =
        Enum.GetValues<TicketPrintElementType>();

    public static Result Validate(
        decimal ticketWidthMm,
        decimal ticketHeightMm,
        IReadOnlyCollection<TicketPrintElementInput>? elements,
        string codePrefix = "TicketConfigurations.Layout")
    {
        var errors = new List<Error>();
        if (!IsPositiveLayoutValue(ticketWidthMm) || !IsPositiveLayoutValue(ticketHeightMm))
        {
            errors.Add(Validation($"{codePrefix}.InvalidDimensions",
                TicketConfigurationFeatureMessages.InvalidDimensions));
        }

        elements ??= Array.Empty<TicketPrintElementInput>();
        if (elements.Any(x => !Enum.IsDefined(x.ElementType)))
        {
            errors.Add(Validation($"{codePrefix}.UnknownElement",
                TicketConfigurationFeatureMessages.UnknownElement));
        }

        var validElements = elements.Where(x => Enum.IsDefined(x.ElementType)).ToArray();
        if (validElements.GroupBy(x => x.ElementType).Any(x => x.Count() > 1))
        {
            errors.Add(Validation($"{codePrefix}.DuplicateElement",
                TicketConfigurationFeatureMessages.DuplicateElement));
        }

        if (RequiredTypes.Any(type => validElements.Count(x => x.ElementType == type) != 1) ||
            validElements.Length != RequiredTypes.Length)
        {
            errors.Add(Validation($"{codePrefix}.MissingElement",
                TicketConfigurationFeatureMessages.MissingElement));
        }

        foreach (var element in validElements)
        {
            ValidateElement(element, ticketWidthMm, ticketHeightMm, codePrefix, errors);
        }

        var visible = validElements
            .Where(x => x.IsVisible && HasCompleteRectangle(x))
            .ToArray();
        for (var first = 0; first < visible.Length; first++)
        {
            for (var second = first + 1; second < visible.Length; second++)
            {
                if (Overlaps(visible[first], visible[second]))
                {
                    errors.Add(Validation($"{codePrefix}.Overlap",
                        TicketConfigurationFeatureMessages.Overlap,
                        $"{visible[first].ElementType},{visible[second].ElementType}"));
                }
            }
        }

        return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
    }

    public static IReadOnlyList<TicketPrintElementSettings> ToSettings(
        IEnumerable<TicketPrintElementInput> elements) =>
        elements.Select(x => new TicketPrintElementSettings(
            x.ElementType, x.IsVisible, x.XMm, x.YMm, x.WidthMm, x.HeightMm,
            x.FontSizePt, x.FontWeight, x.TextAlign, x.Language)).ToArray();

    private static void ValidateElement(
        TicketPrintElementInput element,
        decimal ticketWidthMm,
        decimal ticketHeightMm,
        string codePrefix,
        ICollection<Error> errors)
    {
        var values = new[] { element.XMm, element.YMm, element.WidthMm, element.HeightMm };
        if (values.Where(x => x.HasValue).Any(x => !HasLayoutPrecision(x!.Value)) ||
            element.FontSizePt.HasValue && !HasFontPrecision(element.FontSizePt.Value))
        {
            errors.Add(Validation($"{codePrefix}.{element.ElementType}.InvalidPrecision",
                TicketConfigurationFeatureMessages.InvalidPrecision));
        }

        if (element.ElementType == TicketPrintElementType.BranchLogo)
        {
            if (element.FontSizePt.HasValue || element.FontWeight.HasValue ||
                element.TextAlign.HasValue || element.Language.HasValue)
            {
                errors.Add(Validation($"{codePrefix}.BranchLogo.TypographyNotApplicable",
                    TicketConfigurationFeatureMessages.LogoTypography));
            }
        }
        else if (element.IsVisible &&
                 (!element.FontSizePt.HasValue || element.FontSizePt <= 0 ||
                  !element.FontWeight.HasValue || !Enum.IsDefined(element.FontWeight.Value) ||
                  !element.TextAlign.HasValue || !Enum.IsDefined(element.TextAlign.Value) ||
                  !element.Language.HasValue || !Enum.IsDefined(element.Language.Value)))
        {
            errors.Add(Validation($"{codePrefix}.{element.ElementType}.InvalidTypography",
                TicketConfigurationFeatureMessages.InvalidTypography));
        }

        if (!element.IsVisible)
        {
            return;
        }

        if (!HasCompleteRectangle(element) || element.XMm < 0 || element.YMm < 0 ||
            element.WidthMm <= 0 || element.HeightMm <= 0)
        {
            errors.Add(Validation($"{codePrefix}.{element.ElementType}.InvalidLayout",
                TicketConfigurationFeatureMessages.InvalidLayout));
            return;
        }

        if (element.XMm + element.WidthMm > ticketWidthMm ||
            element.YMm + element.HeightMm > ticketHeightMm)
        {
            errors.Add(Validation($"{codePrefix}.{element.ElementType}.OutsideBounds",
                TicketConfigurationFeatureMessages.OutsideBounds));
        }
    }

    private static bool HasCompleteRectangle(TicketPrintElementInput element) =>
        element.XMm.HasValue && element.YMm.HasValue &&
        element.WidthMm.HasValue && element.HeightMm.HasValue;

    private static bool Overlaps(TicketPrintElementInput first, TicketPrintElementInput second) =>
        first.XMm!.Value < second.XMm!.Value + second.WidthMm!.Value &&
        first.XMm.Value + first.WidthMm!.Value > second.XMm.Value &&
        first.YMm!.Value < second.YMm!.Value + second.HeightMm!.Value &&
        first.YMm.Value + first.HeightMm!.Value > second.YMm.Value;

    private static bool IsPositiveLayoutValue(decimal value) =>
        value > 0 && HasLayoutPrecision(value);

    private static bool HasLayoutPrecision(decimal value) =>
        value >= -MaximumLayoutValue && value <= MaximumLayoutValue &&
        decimal.Round(value, 2) == value;

    private static bool HasFontPrecision(decimal value) =>
        value >= -MaximumFontSize && value <= MaximumFontSize &&
        decimal.Round(value, 2) == value;

    private static Error Validation(string code, string message, string? details = null) =>
        new(code, message, ErrorType.Validation, details);
}
