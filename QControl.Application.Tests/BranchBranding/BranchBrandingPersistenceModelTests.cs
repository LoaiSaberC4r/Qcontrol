using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.Branding;

public sealed class BranchBrandingPersistenceModelTests
{
    [Fact]
    public void Model_configures_layout_columns_constraints_and_relationships()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var branding = model.FindEntityType(
            typeof(QControl.Domain.Entities.BranchBranding))!;

        Assert.Equal("BranchBranding", branding.GetTableName());
        Assert.True(branding.GetIndexes().Single(index =>
            index.Properties.Single().Name ==
                nameof(QControl.Domain.Entities.BranchBranding.BranchId))
            .IsUnique);

        var rowVersion = branding.FindProperty(
            nameof(QControl.Domain.Entities.BranchBranding.RowVersion))!;
        Assert.True(rowVersion.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);

        foreach (var propertyName in ColorProperties)
        {
            var property = branding.FindProperty(propertyName)!;
            Assert.False(property.IsUnicode());
            Assert.Equal(7, property.GetMaxLength());
        }

        foreach (var propertyName in TextProperties)
        {
            var property = branding.FindProperty(propertyName)!;
            Assert.True(property.IsUnicode());
            Assert.Equal(200, property.GetMaxLength());
        }

        foreach (var propertyName in DecimalProperties)
        {
            var property = branding.FindProperty(propertyName)!;
            Assert.Equal(5, property.GetPrecision());
            Assert.Equal(2, property.GetScale());
        }

        foreach (var propertyName in BehaviorProperties)
        {
            Assert.False(branding.FindProperty(propertyName)!.IsNullable);
        }

        var branchForeignKey = branding.GetForeignKeys().Single(foreignKey =>
            foreignKey.Properties.Single().Name ==
                nameof(QControl.Domain.Entities.BranchBranding.BranchId));
        Assert.Equal(DeleteBehavior.Restrict, branchForeignKey.DeleteBehavior);
        Assert.Contains(branding.GetCheckConstraints(), constraint =>
            constraint.Name ==
                "CK_BranchBranding_ServiceButtonSpace_Range");
        Assert.Contains(branding.GetCheckConstraints(), constraint =>
            constraint.Name ==
                "CK_BranchBranding_HeaderColor_Format");

        var advertisement = model.FindEntityType(typeof(BranchAdvertisement))!;
        Assert.Null(advertisement.FindProperty("CenterId"));
        Assert.Null(advertisement.FindProperty("GeneralBrandingId"));
    }

    private static readonly string[] ColorProperties =
    {
        nameof(QControl.Domain.Entities.BranchBranding.HeaderColor),
        nameof(QControl.Domain.Entities.BranchBranding.FooterColor),
        nameof(QControl.Domain.Entities.BranchBranding.MainTextColor),
        nameof(QControl.Domain.Entities.BranchBranding.LanguageButtonBackgroundColor),
        nameof(QControl.Domain.Entities.BranchBranding.LanguageButtonTextColor),
        nameof(QControl.Domain.Entities.BranchBranding.ServiceButtonBackgroundColor),
        nameof(QControl.Domain.Entities.BranchBranding.ServiceButtonTextColor),
        nameof(QControl.Domain.Entities.BranchBranding.KeypadButtonBackgroundColor),
        nameof(QControl.Domain.Entities.BranchBranding.KeypadButtonTextColor),
        nameof(QControl.Domain.Entities.BranchBranding.FooterButtonBackgroundColor),
        nameof(QControl.Domain.Entities.BranchBranding.FooterButtonTextColor)
    };

    private static readonly string[] TextProperties =
    {
        nameof(QControl.Domain.Entities.BranchBranding.LanguageButtonText),
        nameof(QControl.Domain.Entities.BranchBranding.ServiceButtonText),
        nameof(QControl.Domain.Entities.BranchBranding.KeypadButtonText),
        nameof(QControl.Domain.Entities.BranchBranding.FooterButtonText)
    };

    private static readonly string[] DecimalProperties =
    {
        nameof(QControl.Domain.Entities.BranchBranding.LanguageButtonWidth),
        nameof(QControl.Domain.Entities.BranchBranding.LanguageButtonHeight),
        nameof(QControl.Domain.Entities.BranchBranding.ServiceButtonWidth),
        nameof(QControl.Domain.Entities.BranchBranding.ServiceButtonHeight),
        nameof(QControl.Domain.Entities.BranchBranding.ServiceButtonSpace),
        nameof(QControl.Domain.Entities.BranchBranding.ServiceButtonFontSize),
        nameof(QControl.Domain.Entities.BranchBranding.KeypadButtonWidth),
        nameof(QControl.Domain.Entities.BranchBranding.KeypadButtonHeight),
        nameof(QControl.Domain.Entities.BranchBranding.FooterButtonWidth),
        nameof(QControl.Domain.Entities.BranchBranding.FooterButtonHeight)
    };

    private static readonly string[] BehaviorProperties =
    {
        nameof(QControl.Domain.Entities.BranchBranding.ShowLanguagePage),
        nameof(QControl.Domain.Entities.BranchBranding.DefaultLanguageIsArabic),
        nameof(QControl.Domain.Entities.BranchBranding.AlwaysRequireUserInput),
        nameof(QControl.Domain.Entities.BranchBranding.ShowServiceNavigationPath),
        nameof(QControl.Domain.Entities.BranchBranding.AllowOperatorSelection),
        nameof(QControl.Domain.Entities.BranchBranding.AllowRequestMoreServices)
    };

    private static PlatformWriteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=QControlModelTests;Trusted_Connection=True;")
            .Options;

        return new PlatformWriteDbContext(
            options,
            new TestTenantContext(),
            new TestCurrentBranchContext());
    }
}
