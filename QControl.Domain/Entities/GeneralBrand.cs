using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class GeneralBrand : Entity<int>
{
    public byte SingletonKey { get; private set; }

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

    public ApplicationUser CreatedByApplicationUser { get; private set; } =
        null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    private GeneralBrand()
    {
    }

    public static GeneralBrand Create(
        BrandingLayoutSettings layout,
        Guid createdByApplicationUserId)
    {
        var generalBrand = new GeneralBrand
        {
            SingletonKey = 1,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };

        generalBrand.ApplyLayout(
            BrandingLayoutNormalizer.Normalize(layout));

        return generalBrand;
    }

    public void UpdateLayout(
        BrandingLayoutSettings layout,
        Guid lastModifiedByApplicationUserId)
    {
        ApplyLayout(BrandingLayoutNormalizer.Normalize(layout));
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    private void ApplyLayout(BrandingLayoutSettings layout)
    {
        MainColor = layout.MainColor;
        SecondaryColor = layout.SecondaryColor;
        BackgroundColor = layout.BackgroundColor;
        HeaderColor = layout.HeaderColor;
        FooterColor = layout.FooterColor;
        MainTextColor = layout.MainTextColor;
        ShowLanguagePage = layout.ShowLanguagePage;
        DefaultLanguageIsArabic = layout.DefaultLanguageIsArabic;
        AlwaysRequireUserInput = layout.AlwaysRequireUserInput;
        ShowServiceNavigationPath = layout.ShowServiceNavigationPath;
        AllowOperatorSelection = layout.AllowOperatorSelection;
        AllowRequestMoreServices = layout.AllowRequestMoreServices;
        LanguageButtonBackgroundColor =
            layout.LanguageButtonBackgroundColor;
        LanguageButtonTextColor = layout.LanguageButtonTextColor;
        LanguageButtonWidth = layout.LanguageButtonWidth;
        LanguageButtonHeight = layout.LanguageButtonHeight;
        LanguageButtonText = layout.LanguageButtonText;
        ServiceButtonBackgroundColor = layout.ServiceButtonBackgroundColor;
        ServiceButtonTextColor = layout.ServiceButtonTextColor;
        ServiceButtonWidth = layout.ServiceButtonWidth;
        ServiceButtonHeight = layout.ServiceButtonHeight;
        ServiceButtonSpace = layout.ServiceButtonSpace;
        ServiceButtonFontSize = layout.ServiceButtonFontSize;
        ServiceButtonText = layout.ServiceButtonText;
        KeypadButtonBackgroundColor = layout.KeypadButtonBackgroundColor;
        KeypadButtonTextColor = layout.KeypadButtonTextColor;
        KeypadButtonWidth = layout.KeypadButtonWidth;
        KeypadButtonHeight = layout.KeypadButtonHeight;
        KeypadButtonText = layout.KeypadButtonText;
        FooterButtonBackgroundColor = layout.FooterButtonBackgroundColor;
        FooterButtonTextColor = layout.FooterButtonTextColor;
        FooterButtonWidth = layout.FooterButtonWidth;
        FooterButtonHeight = layout.FooterButtonHeight;
        FooterButtonText = layout.FooterButtonText;
    }
}
