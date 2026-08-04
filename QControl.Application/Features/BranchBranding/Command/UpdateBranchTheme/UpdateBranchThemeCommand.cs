using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchBranding.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;

public sealed record UpdateBranchThemeCommand
    : ICommand<BranchBrandingResponse>,
      ICacheInvalidator,
      IBrandingLayoutInput
{
    public int BranchId { get; init; }

    public string MainColor { get; init; } = string.Empty;

    public string SecondaryColor { get; init; } = string.Empty;

    public string BackgroundColor { get; init; } = string.Empty;

    public string? HeaderColor { get; init; }

    public string? FooterColor { get; init; }

    public string? MainTextColor { get; init; }

    public bool ShowLanguagePage { get; init; }

    public bool DefaultLanguageIsArabic { get; init; }

    public bool AlwaysRequireUserInput { get; init; }

    public bool ShowServiceNavigationPath { get; init; }

    public bool AllowOperatorSelection { get; init; }

    public bool AllowRequestMoreServices { get; init; }

    public string? LanguageButtonBackgroundColor { get; init; }

    public string? LanguageButtonTextColor { get; init; }

    public decimal? LanguageButtonWidth { get; init; }

    public decimal? LanguageButtonHeight { get; init; }

    public string? LanguageButtonText { get; init; }

    public string? ServiceButtonBackgroundColor { get; init; }

    public string? ServiceButtonTextColor { get; init; }

    public decimal? ServiceButtonWidth { get; init; }

    public decimal? ServiceButtonHeight { get; init; }

    public decimal? ServiceButtonSpace { get; init; }

    public decimal? ServiceButtonFontSize { get; init; }

    public string? ServiceButtonText { get; init; }

    public string? KeypadButtonBackgroundColor { get; init; }

    public string? KeypadButtonTextColor { get; init; }

    public decimal? KeypadButtonWidth { get; init; }

    public decimal? KeypadButtonHeight { get; init; }

    public string? KeypadButtonText { get; init; }

    public string? FooterButtonBackgroundColor { get; init; }

    public string? FooterButtonTextColor { get; init; }

    public decimal? FooterButtonWidth { get; init; }

    public decimal? FooterButtonHeight { get; init; }

    public string? FooterButtonText { get; init; }

    public string? RowVersion { get; init; }

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.BranchBranding,
        OperationalCacheTags.BranchBrandingForBranch(BranchId)
    };
}
