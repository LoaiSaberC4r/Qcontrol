using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.GeneralBranding;

public sealed class GeneralBrandDomainTests
{
    [Fact]
    public void Create_sets_singleton_and_complete_normalized_layout()
    {
        var generalBrand = QControl.Domain.Entities.GeneralBrand.Create(
            GeneralBrandTestData.Layout(),
            EntityTestFactory.CurrentUserId);

        Assert.Equal(1, generalBrand.SingletonKey);
        Assert.Equal("#0070C4", generalBrand.MainColor);
        Assert.Equal("#FFFFFF", generalBrand.SecondaryColor);
        Assert.Equal("#F0F0F0", generalBrand.BackgroundColor);
        Assert.Equal("#A7060F", generalBrand.MainTextColor);
        Assert.True(generalBrand.ShowLanguagePage);
        Assert.True(generalBrand.AllowRequestMoreServices);
        Assert.Equal(40m, generalBrand.LanguageButtonWidth);
        Assert.Equal("اختيار اللغة", generalBrand.LanguageButtonText);
        Assert.Equal(1.8m, generalBrand.ServiceButtonFontSize);
        Assert.Equal("اختيار الخدمة", generalBrand.ServiceButtonText);
        Assert.Equal("تأكيد", generalBrand.KeypadButtonText);
        Assert.Equal("رجوع", generalBrand.FooterButtonText);
        Assert.Equal(
            EntityTestFactory.CurrentUserId,
            generalBrand.CreatedByApplicationUserId);
        Assert.Null(generalBrand.LastModifiedByApplicationUserId);
    }

    [Fact]
    public void Update_replaces_layout_and_preserves_creation_and_singleton()
    {
        var generalBrand = QControl.Domain.Entities.GeneralBrand.Create(
            GeneralBrandTestData.Layout(),
            EntityTestFactory.CurrentUserId);
        var modifier = Guid.Parse(
            "22222222-2222-2222-2222-222222222222");
        var updated = GeneralBrandTestData.Layout() with
        {
            HeaderColor = "#abcdef",
            LanguageButtonText = "   ",
            FooterButtonText = "  Back  ",
            AllowOperatorSelection = false
        };

        generalBrand.UpdateLayout(updated, modifier);

        Assert.Equal(1, generalBrand.SingletonKey);
        Assert.Equal("#ABCDEF", generalBrand.HeaderColor);
        Assert.Null(generalBrand.LanguageButtonText);
        Assert.Equal("Back", generalBrand.FooterButtonText);
        Assert.False(generalBrand.AllowOperatorSelection);
        Assert.Equal(
            EntityTestFactory.CurrentUserId,
            generalBrand.CreatedByApplicationUserId);
        Assert.Equal(modifier, generalBrand.LastModifiedByApplicationUserId);
    }
}
