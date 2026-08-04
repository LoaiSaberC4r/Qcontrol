using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.GeneralBranding;

public sealed class GeneralBrandPersistenceModelTests
{
    [Fact]
    public void Model_enforces_singleton_layout_audit_and_concurrency()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var generalBrand = model.FindEntityType(
            typeof(QControl.Domain.Entities.GeneralBrand))!;

        Assert.Equal("GeneralBrand", generalBrand.GetTableName());
        Assert.Equal(ValueGenerated.OnAdd,
            generalBrand.FindProperty(nameof(QControl.Domain.Entities.GeneralBrand.Id))!
                .ValueGenerated);

        var singletonKey = generalBrand.FindProperty(
            nameof(QControl.Domain.Entities.GeneralBrand.SingletonKey))!;
        Assert.False(singletonKey.IsNullable);
        Assert.Equal("tinyint", singletonKey.GetColumnType());
        Assert.Equal((byte)1, singletonKey.GetDefaultValue());
        Assert.True(generalBrand.GetIndexes().Single(index =>
            index.GetDatabaseName() == "UX_GeneralBrand_SingletonKey")
            .IsUnique);
        Assert.Contains(generalBrand.GetCheckConstraints(), constraint =>
            constraint.Name == "CK_GeneralBrand_SingletonKey" &&
            constraint.Sql == "[SingletonKey] = 1");

        foreach (var propertyName in ColorProperties)
        {
            var property = generalBrand.FindProperty(propertyName)!;
            Assert.False(property.IsUnicode());
            Assert.Equal(7, property.GetMaxLength());
        }

        foreach (var propertyName in TextProperties)
        {
            var property = generalBrand.FindProperty(propertyName)!;
            Assert.True(property.IsUnicode());
            Assert.Equal(200, property.GetMaxLength());
        }

        foreach (var propertyName in DecimalProperties)
        {
            var property = generalBrand.FindProperty(propertyName)!;
            Assert.Equal(5, property.GetPrecision());
            Assert.Equal(2, property.GetScale());
        }

        var rowVersion = generalBrand.FindProperty(
            nameof(QControl.Domain.Entities.GeneralBrand.RowVersion))!;
        Assert.True(rowVersion.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);

        Assert.All(generalBrand.GetForeignKeys(), foreignKey =>
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior));

        Assert.Null(generalBrand.FindProperty("BranchId"));
        Assert.Null(generalBrand.FindProperty("CenterId"));
        Assert.Null(generalBrand.FindProperty("LogoPath"));

        var branchBranding = model.FindEntityType(
            typeof(QControl.Domain.Entities.BranchBranding))!;
        Assert.Null(branchBranding.FindProperty("GeneralBrandId"));
        var advertisement = model.FindEntityType(typeof(BranchAdvertisement))!;
        Assert.Null(advertisement.FindProperty("GeneralBrandId"));
    }

    private static readonly string[] ColorProperties =
    {
        nameof(QControl.Domain.Entities.GeneralBrand.MainColor),
        nameof(QControl.Domain.Entities.GeneralBrand.SecondaryColor),
        nameof(QControl.Domain.Entities.GeneralBrand.BackgroundColor),
        nameof(QControl.Domain.Entities.GeneralBrand.HeaderColor),
        nameof(QControl.Domain.Entities.GeneralBrand.FooterColor),
        nameof(QControl.Domain.Entities.GeneralBrand.MainTextColor),
        nameof(QControl.Domain.Entities.GeneralBrand.LanguageButtonBackgroundColor),
        nameof(QControl.Domain.Entities.GeneralBrand.LanguageButtonTextColor),
        nameof(QControl.Domain.Entities.GeneralBrand.ServiceButtonBackgroundColor),
        nameof(QControl.Domain.Entities.GeneralBrand.ServiceButtonTextColor),
        nameof(QControl.Domain.Entities.GeneralBrand.KeypadButtonBackgroundColor),
        nameof(QControl.Domain.Entities.GeneralBrand.KeypadButtonTextColor),
        nameof(QControl.Domain.Entities.GeneralBrand.FooterButtonBackgroundColor),
        nameof(QControl.Domain.Entities.GeneralBrand.FooterButtonTextColor)
    };

    private static readonly string[] TextProperties =
    {
        nameof(QControl.Domain.Entities.GeneralBrand.LanguageButtonText),
        nameof(QControl.Domain.Entities.GeneralBrand.ServiceButtonText),
        nameof(QControl.Domain.Entities.GeneralBrand.KeypadButtonText),
        nameof(QControl.Domain.Entities.GeneralBrand.FooterButtonText)
    };

    private static readonly string[] DecimalProperties =
    {
        nameof(QControl.Domain.Entities.GeneralBrand.LanguageButtonWidth),
        nameof(QControl.Domain.Entities.GeneralBrand.LanguageButtonHeight),
        nameof(QControl.Domain.Entities.GeneralBrand.ServiceButtonWidth),
        nameof(QControl.Domain.Entities.GeneralBrand.ServiceButtonHeight),
        nameof(QControl.Domain.Entities.GeneralBrand.ServiceButtonSpace),
        nameof(QControl.Domain.Entities.GeneralBrand.ServiceButtonFontSize),
        nameof(QControl.Domain.Entities.GeneralBrand.KeypadButtonWidth),
        nameof(QControl.Domain.Entities.GeneralBrand.KeypadButtonHeight),
        nameof(QControl.Domain.Entities.GeneralBrand.FooterButtonWidth),
        nameof(QControl.Domain.Entities.GeneralBrand.FooterButtonHeight)
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
