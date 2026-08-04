using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Branding;

public sealed class BranchBrandingDomainTests
{
    [Fact]
    public void Update_layout_normalizes_and_updates_every_field_without_touching_logo()
    {
        var branding = QControl.Domain.Entities.BranchBranding.Create(
            branchId: 12,
            EntityTestFactory.CurrentUserId);
        branding.ReplaceLogo(
            "Media\\Branches\\12\\Logo\\logo.png",
            EntityTestFactory.CurrentUserId);
        var modifierId = Guid.Parse(
            "22222222-2222-2222-2222-222222222222");

        branding.UpdateLayout(CompleteLayout(), modifierId);

        Assert.Equal(12, branding.BranchId);
        Assert.Equal(
            "Media/Branches/12/Logo/logo.png",
            branding.LogoPath);
        Assert.Equal(EntityTestFactory.CurrentUserId,
            branding.CreatedByApplicationUserId);
        Assert.Equal(modifierId, branding.LastModifiedByApplicationUserId);
        Assert.Equal("#0070C4", branding.MainColor);
        Assert.Equal("#FFFFFF", branding.SecondaryColor);
        Assert.Equal("#F0F0F0", branding.BackgroundColor);
        Assert.Equal("#111111", branding.HeaderColor);
        Assert.Equal("#222222", branding.FooterColor);
        Assert.Equal("#333333", branding.MainTextColor);
        Assert.True(branding.ShowLanguagePage);
        Assert.True(branding.DefaultLanguageIsArabic);
        Assert.True(branding.AlwaysRequireUserInput);
        Assert.True(branding.ShowServiceNavigationPath);
        Assert.True(branding.AllowOperatorSelection);
        Assert.True(branding.AllowRequestMoreServices);
        Assert.Equal("#444444", branding.LanguageButtonBackgroundColor);
        Assert.Equal("#555555", branding.LanguageButtonTextColor);
        Assert.Equal(40m, branding.LanguageButtonWidth);
        Assert.Equal(22m, branding.LanguageButtonHeight);
        Assert.Equal("اختيار اللغة", branding.LanguageButtonText);
        Assert.Equal("#666666", branding.ServiceButtonBackgroundColor);
        Assert.Equal("#777777", branding.ServiceButtonTextColor);
        Assert.Equal(41m, branding.ServiceButtonWidth);
        Assert.Equal(20m, branding.ServiceButtonHeight);
        Assert.Equal(2m, branding.ServiceButtonSpace);
        Assert.Equal(1.8m, branding.ServiceButtonFontSize);
        Assert.Equal("Choose service", branding.ServiceButtonText);
        Assert.Equal("#888888", branding.KeypadButtonBackgroundColor);
        Assert.Equal("#999999", branding.KeypadButtonTextColor);
        Assert.Equal(45m, branding.KeypadButtonWidth);
        Assert.Equal(10m, branding.KeypadButtonHeight);
        Assert.Equal("تأكيد", branding.KeypadButtonText);
        Assert.Equal("#AAAAAA", branding.FooterButtonBackgroundColor);
        Assert.Equal("#BBBBBB", branding.FooterButtonTextColor);
        Assert.Equal(10m, branding.FooterButtonWidth);
        Assert.Equal(12m, branding.FooterButtonHeight);
        Assert.Equal("رجوع", branding.FooterButtonText);
    }

    private static BrandingLayoutSettings CompleteLayout() =>
        new()
        {
            MainColor = " #0070c4 ",
            SecondaryColor = "#ffffff",
            BackgroundColor = "#f0f0f0",
            HeaderColor = "#111111",
            FooterColor = "#222222",
            MainTextColor = "#333333",
            ShowLanguagePage = true,
            DefaultLanguageIsArabic = true,
            AlwaysRequireUserInput = true,
            ShowServiceNavigationPath = true,
            AllowOperatorSelection = true,
            AllowRequestMoreServices = true,
            LanguageButtonBackgroundColor = "#444444",
            LanguageButtonTextColor = "#555555",
            LanguageButtonWidth = 40m,
            LanguageButtonHeight = 22m,
            LanguageButtonText = "  اختيار اللغة  ",
            ServiceButtonBackgroundColor = "#666666",
            ServiceButtonTextColor = "#777777",
            ServiceButtonWidth = 41m,
            ServiceButtonHeight = 20m,
            ServiceButtonSpace = 2m,
            ServiceButtonFontSize = 1.8m,
            ServiceButtonText = "  Choose service  ",
            KeypadButtonBackgroundColor = "#888888",
            KeypadButtonTextColor = "#999999",
            KeypadButtonWidth = 45m,
            KeypadButtonHeight = 10m,
            KeypadButtonText = "  تأكيد  ",
            FooterButtonBackgroundColor = "#aaaaaa",
            FooterButtonTextColor = "#bbbbbb",
            FooterButtonWidth = 10m,
            FooterButtonHeight = 12m,
            FooterButtonText = "  رجوع  "
        };
}
