using QControl.Application.Features.TicketConfigurations.Shared;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.TicketConfigurations;

public sealed class TicketPrintLayoutValidatorTests
{
    [Theory]
    [InlineData(0, 120)]
    [InlineData(-1, 120)]
    [InlineData(80, 0)]
    [InlineData(80, -1)]
    public void Non_positive_ticket_dimensions_are_invalid(decimal width, decimal height)
    {
        var result = Validate(width, height, HiddenElements());
        Assert.Contains(result.Errors, x => x.Code.Contains("InvalidDimensions"));
    }

    [Theory]
    [InlineData(-1, 0, 10, 10)]
    [InlineData(0, -1, 10, 10)]
    [InlineData(0, 0, 0, 10)]
    [InlineData(0, 0, 10, 0)]
    [InlineData(0, 0, -1, 10)]
    [InlineData(0, 0, 10, -1)]
    public void Invalid_visible_rectangle_is_rejected(
        decimal x, decimal y, decimal width, decimal height)
    {
        var elements = HiddenElements();
        elements[1] = VisibleText(TicketPrintElementType.BranchName, x, y, width, height);

        var result = Validate(80, 120, elements);
        Assert.Contains(result.Errors, error => error.Code.Contains("InvalidLayout"));
    }

    [Theory]
    [InlineData(75, 0, 6, 10)]
    [InlineData(0, 115, 10, 6)]
    public void Visible_rectangle_outside_ticket_is_rejected(
        decimal x, decimal y, decimal width, decimal height)
    {
        var elements = HiddenElements();
        elements[1] = VisibleText(TicketPrintElementType.BranchName, x, y, width, height);

        var result = Validate(80, 120, elements);
        Assert.Contains(result.Errors, error => error.Code.Contains("OutsideBounds"));
    }

    [Fact]
    public void Hidden_rectangle_is_ignored_for_bounds_and_overlap()
    {
        var elements = HiddenElements();
        elements[1] = elements[1] with
        {
            XMm = -100,
            YMm = -100,
            WidthMm = 500,
            HeightMm = 500
        };
        elements[2] = VisibleText(TicketPrintElementType.LeafServiceName, 0, 0, 20, 10);

        Assert.True(Validate(80, 120, elements).IsSuccess);
    }

    [Fact]
    public void Visible_text_requires_complete_valid_typography()
    {
        var elements = HiddenElements();
        elements[1] = VisibleText(TicketPrintElementType.BranchName, 0, 0, 20, 10) with
        {
            FontSizePt = null,
            FontWeight = (TicketFontWeight)99,
            TextAlign = (TicketTextAlign)99,
            Language = (TicketPrintLanguage)99
        };

        var result = Validate(80, 120, elements);
        Assert.Contains(result.Errors, error => error.Code.Contains("InvalidTypography"));
    }

    [Fact]
    public void Visible_logo_requires_layout_and_rejects_typography()
    {
        var missingLayout = HiddenElements();
        missingLayout[0] = missingLayout[0] with { IsVisible = true };
        Assert.Contains(Validate(80, 120, missingLayout).Errors,
            error => error.Code.Contains("InvalidLayout"));

        var typography = HiddenElements();
        typography[0] = VisibleLogo(0, 0, 20, 10) with { FontSizePt = 12 };
        Assert.Contains(Validate(80, 120, typography).Errors,
            error => error.Code.Contains("TypographyNotApplicable"));
    }

    [Fact]
    public void Missing_duplicate_and_unknown_element_types_are_rejected()
    {
        var missing = HiddenElements()[..7];
        Assert.Contains(Validate(80, 120, missing).Errors,
            error => error.Code.Contains("MissingElement"));

        var duplicate = HiddenElements();
        duplicate[7] = duplicate[7] with { ElementType = TicketPrintElementType.BranchName };
        var duplicateResult = Validate(80, 120, duplicate);
        Assert.Contains(duplicateResult.Errors, error => error.Code.Contains("DuplicateElement"));
        Assert.Contains(duplicateResult.Errors, error => error.Code.Contains("MissingElement"));

        var unknown = HiddenElements();
        unknown[7] = unknown[7] with { ElementType = (TicketPrintElementType)99 };
        Assert.Contains(Validate(80, 120, unknown).Errors,
            error => error.Code.Contains("UnknownElement"));
    }

    [Fact]
    public void All_eight_unique_hidden_elements_are_valid()
    {
        Assert.True(Validate(80, 120, HiddenElements()).IsSuccess);
    }

    [Fact]
    public void Actual_intersection_is_invalid_but_touching_edges_are_valid()
    {
        var overlapping = HiddenElements();
        overlapping[0] = VisibleLogo(0, 0, 20, 10);
        overlapping[4] = VisibleText(TicketPrintElementType.TicketNumber, 19, 0, 20, 10);
        Assert.Contains(Validate(80, 120, overlapping).Errors,
            error => error.Code.Contains("Overlap"));

        var touchingRight = HiddenElements();
        touchingRight[1] = VisibleText(TicketPrintElementType.BranchName, 0, 0, 20, 10);
        touchingRight[2] = VisibleText(TicketPrintElementType.LeafServiceName, 20, 0, 20, 10);
        Assert.True(Validate(80, 120, touchingRight).IsSuccess);

        var touchingBottom = HiddenElements();
        touchingBottom[1] = VisibleText(TicketPrintElementType.BranchName, 0, 0, 20, 10);
        touchingBottom[2] = VisibleText(TicketPrintElementType.LeafServiceName, 0, 10, 20, 10);
        Assert.True(Validate(80, 120, touchingBottom).IsSuccess);
    }

    [Fact]
    public void Values_that_would_be_rounded_by_sql_are_rejected()
    {
        var elements = HiddenElements();
        elements[1] = VisibleText(TicketPrintElementType.BranchName, 0.001m, 0, 20, 10);
        Assert.Contains(Validate(80, 120, elements).Errors,
            error => error.Code.Contains("InvalidPrecision"));
    }

    private static BuildingBlock.Domain.Results.Result Validate(
        decimal width,
        decimal height,
        IReadOnlyCollection<TicketPrintElementInput> elements) =>
        TicketPrintLayoutValidator.Validate(width, height, elements);

    private static TicketPrintElementInput[] HiddenElements() =>
        Enum.GetValues<TicketPrintElementType>()
            .Select(type => new TicketPrintElementInput { ElementType = type })
            .ToArray();

    private static TicketPrintElementInput VisibleLogo(
        decimal x, decimal y, decimal width, decimal height) => new()
        {
            ElementType = TicketPrintElementType.BranchLogo,
            IsVisible = true,
            XMm = x,
            YMm = y,
            WidthMm = width,
            HeightMm = height
        };

    private static TicketPrintElementInput VisibleText(
        TicketPrintElementType type,
        decimal x,
        decimal y,
        decimal width,
        decimal height) => new()
        {
            ElementType = type,
            IsVisible = true,
            XMm = x,
            YMm = y,
            WidthMm = width,
            HeightMm = height,
            FontSizePt = 12,
            FontWeight = TicketFontWeight.Normal,
            TextAlign = TicketTextAlign.Center,
            Language = TicketPrintLanguage.En
        };
}
