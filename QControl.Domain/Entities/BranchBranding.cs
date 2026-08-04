using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class BranchBranding : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public string? LogoPath { get; private set; }

    public string? MainColor { get; private set; }

    public string? SecondaryColor { get; private set; }

    public string? BackgroundColor { get; private set; }

    public string? HeaderColor { get; private set; }

    public string? FooterColor { get; private set; }

    public string? MainTextColor { get; private set; }

    public bool ShowLanguagePage { get; private set; }

    public bool DefaultLanguageIsArabic { get; private set; }

    public bool AlwaysRequireUserInput { get; private set; }

    public bool ShowServiceNavigationPath { get; private set; }

    public bool AllowOperatorSelection { get; private set; }

    public bool AllowRequestMoreServices { get; private set; }

    public string? LanguageButtonBackgroundColor { get; private set; }

    public string? LanguageButtonTextColor { get; private set; }

    public decimal? LanguageButtonWidth { get; private set; }

    public decimal? LanguageButtonHeight { get; private set; }

    public string? LanguageButtonText { get; private set; }

    public string? ServiceButtonBackgroundColor { get; private set; }

    public string? ServiceButtonTextColor { get; private set; }

    public decimal? ServiceButtonWidth { get; private set; }

    public decimal? ServiceButtonHeight { get; private set; }

    public decimal? ServiceButtonSpace { get; private set; }

    public decimal? ServiceButtonFontSize { get; private set; }

    public string? ServiceButtonText { get; private set; }

    public string? KeypadButtonBackgroundColor { get; private set; }

    public string? KeypadButtonTextColor { get; private set; }

    public decimal? KeypadButtonWidth { get; private set; }

    public decimal? KeypadButtonHeight { get; private set; }

    public string? KeypadButtonText { get; private set; }

    public string? FooterButtonBackgroundColor { get; private set; }

    public string? FooterButtonTextColor { get; private set; }

    public decimal? FooterButtonWidth { get; private set; }

    public decimal? FooterButtonHeight { get; private set; }

    public string? FooterButtonText { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    private BranchBranding()
    {
    }

    public static BranchBranding Create(
        int branchId,
        Guid createdByApplicationUserId)
    {
        return new BranchBranding
        {
            BranchId = branchId,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void UpdateTheme(
        string mainColor,
        string secondaryColor,
        string backgroundColor,
        Guid lastModifiedByApplicationUserId)
    {
        MainColor = NormalizeColor(mainColor);
        SecondaryColor = NormalizeColor(secondaryColor);
        BackgroundColor = NormalizeColor(backgroundColor);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public void UpdateLayout(
        BrandingLayoutSettings settings,
        Guid lastModifiedByApplicationUserId)
    {
        settings = BrandingLayoutNormalizer.Normalize(settings);

        MainColor = settings.MainColor;
        SecondaryColor = settings.SecondaryColor;
        BackgroundColor = settings.BackgroundColor;
        HeaderColor = settings.HeaderColor;
        FooterColor = settings.FooterColor;
        MainTextColor = settings.MainTextColor;

        ShowLanguagePage = settings.ShowLanguagePage;
        DefaultLanguageIsArabic = settings.DefaultLanguageIsArabic;
        AlwaysRequireUserInput = settings.AlwaysRequireUserInput;
        ShowServiceNavigationPath = settings.ShowServiceNavigationPath;
        AllowOperatorSelection = settings.AllowOperatorSelection;
        AllowRequestMoreServices = settings.AllowRequestMoreServices;

        LanguageButtonBackgroundColor = settings.LanguageButtonBackgroundColor;
        LanguageButtonTextColor = settings.LanguageButtonTextColor;
        LanguageButtonWidth = settings.LanguageButtonWidth;
        LanguageButtonHeight = settings.LanguageButtonHeight;
        LanguageButtonText = settings.LanguageButtonText;

        ServiceButtonBackgroundColor = settings.ServiceButtonBackgroundColor;
        ServiceButtonTextColor = settings.ServiceButtonTextColor;
        ServiceButtonWidth = settings.ServiceButtonWidth;
        ServiceButtonHeight = settings.ServiceButtonHeight;
        ServiceButtonSpace = settings.ServiceButtonSpace;
        ServiceButtonFontSize = settings.ServiceButtonFontSize;
        ServiceButtonText = settings.ServiceButtonText;

        KeypadButtonBackgroundColor = settings.KeypadButtonBackgroundColor;
        KeypadButtonTextColor = settings.KeypadButtonTextColor;
        KeypadButtonWidth = settings.KeypadButtonWidth;
        KeypadButtonHeight = settings.KeypadButtonHeight;
        KeypadButtonText = settings.KeypadButtonText;

        FooterButtonBackgroundColor = settings.FooterButtonBackgroundColor;
        FooterButtonTextColor = settings.FooterButtonTextColor;
        FooterButtonWidth = settings.FooterButtonWidth;
        FooterButtonHeight = settings.FooterButtonHeight;
        FooterButtonText = settings.FooterButtonText;

        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public void ReplaceLogo(
        string logoPath,
        Guid lastModifiedByApplicationUserId)
    {
        LogoPath = NormalizePath(logoPath);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    private static string NormalizeColor(string value) =>
        value.Trim().ToUpperInvariant();

    private static string NormalizePath(string value) =>
        value.Trim().Replace('\\', '/');
}
