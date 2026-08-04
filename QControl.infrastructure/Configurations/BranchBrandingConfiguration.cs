using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchBrandingConfiguration
    : IEntityTypeConfiguration<BranchBranding>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchBranding> builder)
    {
        builder.ToTable("BranchBranding", table =>
        {
            table.HasCheckConstraint(
                "CK_BranchBranding_MainColor_Format",
                "[MainColor] IS NULL OR ([MainColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([MainColor]) = 7)");

            table.HasCheckConstraint(
                "CK_BranchBranding_SecondaryColor_Format",
                "[SecondaryColor] IS NULL OR ([SecondaryColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([SecondaryColor]) = 7)");

            table.HasCheckConstraint(
                "CK_BranchBranding_BackgroundColor_Format",
                "[BackgroundColor] IS NULL OR ([BackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([BackgroundColor]) = 7)");

            AddColorCheck(table, "HeaderColor");
            AddColorCheck(table, "FooterColor");
            AddColorCheck(table, "MainTextColor");
            AddColorCheck(table, "LanguageButtonBackgroundColor");
            AddColorCheck(table, "LanguageButtonTextColor");
            AddColorCheck(table, "ServiceButtonBackgroundColor");
            AddColorCheck(table, "ServiceButtonTextColor");
            AddColorCheck(table, "KeypadButtonBackgroundColor");
            AddColorCheck(table, "KeypadButtonTextColor");
            AddColorCheck(table, "FooterButtonBackgroundColor");
            AddColorCheck(table, "FooterButtonTextColor");

            AddPositivePercentageCheck(table, "LanguageButtonWidth");
            AddPositivePercentageCheck(table, "LanguageButtonHeight");
            AddPositivePercentageCheck(table, "ServiceButtonWidth");
            AddPositivePercentageCheck(table, "ServiceButtonHeight");
            table.HasCheckConstraint(
                "CK_BranchBranding_ServiceButtonSpace_Range",
                "[ServiceButtonSpace] IS NULL OR ([ServiceButtonSpace] >= 0 AND [ServiceButtonSpace] <= 100)");
            AddPositivePercentageCheck(table, "ServiceButtonFontSize");
            AddPositivePercentageCheck(table, "KeypadButtonWidth");
            AddPositivePercentageCheck(table, "KeypadButtonHeight");
            AddPositivePercentageCheck(table, "FooterButtonWidth");
            AddPositivePercentageCheck(table, "FooterButtonHeight");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.LogoPath)
            .IsRequired(false)
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(x => x.MainColor)
            .IsRequired(false)
            .HasMaxLength(7)
            .IsUnicode(false);

        builder.Property(x => x.SecondaryColor)
            .IsRequired(false)
            .HasMaxLength(7)
            .IsUnicode(false);

        builder.Property(x => x.BackgroundColor)
            .IsRequired(false)
            .HasMaxLength(7)
            .IsUnicode(false);

        ConfigureColor(builder, x => x.HeaderColor);
        ConfigureColor(builder, x => x.FooterColor);
        ConfigureColor(builder, x => x.MainTextColor);
        ConfigureColor(builder, x => x.LanguageButtonBackgroundColor);
        ConfigureColor(builder, x => x.LanguageButtonTextColor);
        ConfigureColor(builder, x => x.ServiceButtonBackgroundColor);
        ConfigureColor(builder, x => x.ServiceButtonTextColor);
        ConfigureColor(builder, x => x.KeypadButtonBackgroundColor);
        ConfigureColor(builder, x => x.KeypadButtonTextColor);
        ConfigureColor(builder, x => x.FooterButtonBackgroundColor);
        ConfigureColor(builder, x => x.FooterButtonTextColor);

        builder.Property(x => x.ShowLanguagePage).IsRequired();
        builder.Property(x => x.DefaultLanguageIsArabic).IsRequired();
        builder.Property(x => x.AlwaysRequireUserInput).IsRequired();
        builder.Property(x => x.ShowServiceNavigationPath).IsRequired();
        builder.Property(x => x.AllowOperatorSelection).IsRequired();
        builder.Property(x => x.AllowRequestMoreServices).IsRequired();

        ConfigureDecimal(builder, x => x.LanguageButtonWidth);
        ConfigureDecimal(builder, x => x.LanguageButtonHeight);
        ConfigureText(builder, x => x.LanguageButtonText);
        ConfigureDecimal(builder, x => x.ServiceButtonWidth);
        ConfigureDecimal(builder, x => x.ServiceButtonHeight);
        ConfigureDecimal(builder, x => x.ServiceButtonSpace);
        ConfigureDecimal(builder, x => x.ServiceButtonFontSize);
        ConfigureText(builder, x => x.ServiceButtonText);
        ConfigureDecimal(builder, x => x.KeypadButtonWidth);
        ConfigureDecimal(builder, x => x.KeypadButtonHeight);
        ConfigureText(builder, x => x.KeypadButtonText);
        ConfigureDecimal(builder, x => x.FooterButtonWidth);
        ConfigureDecimal(builder, x => x.FooterButtonHeight);
        ConfigureText(builder, x => x.FooterButtonText);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired()
            .HasColumnType("datetime2(3)");

        builder.Property(x => x.ModifiedOnUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(3)");

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.HasIndex(x => x.BranchId)
            .IsUnique();

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasOne(x => x.Branch)
            .WithOne(x => x.Branding)
            .HasForeignKey<BranchBranding>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastModifiedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void AddColorCheck(
        TableBuilder<BranchBranding> table,
        string propertyName)
    {
        table.HasCheckConstraint(
            $"CK_BranchBranding_{propertyName}_Format",
            $"[{propertyName}] IS NULL OR ([{propertyName}] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([{propertyName}]) = 7)");
    }

    private static void AddPositivePercentageCheck(
        TableBuilder<BranchBranding> table,
        string propertyName)
    {
        table.HasCheckConstraint(
            $"CK_BranchBranding_{propertyName}_Range",
            $"[{propertyName}] IS NULL OR ([{propertyName}] > 0 AND [{propertyName}] <= 100)");
    }

    private static void ConfigureColor(
        EntityTypeBuilder<BranchBranding> builder,
        System.Linq.Expressions.Expression<Func<BranchBranding, string?>> property)
    {
        builder.Property(property)
            .IsRequired(false)
            .HasMaxLength(7)
            .IsUnicode(false);
    }

    private static void ConfigureDecimal(
        EntityTypeBuilder<BranchBranding> builder,
        System.Linq.Expressions.Expression<Func<BranchBranding, decimal?>> property)
    {
        builder.Property(property)
            .IsRequired(false)
            .HasPrecision(5, 2);
    }

    private static void ConfigureText(
        EntityTypeBuilder<BranchBranding> builder,
        System.Linq.Expressions.Expression<Func<BranchBranding, string?>> property)
    {
        builder.Property(property)
            .IsRequired(false)
            .HasMaxLength(200)
            .IsUnicode();
    }
}
