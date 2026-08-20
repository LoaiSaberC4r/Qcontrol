using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchDisplayConfigurationConfiguration
    : IEntityTypeConfiguration<BranchDisplayConfiguration>,
      IWriteEntityConfiguration
{
    private static readonly string[] ColorProperties =
    [
        nameof(BranchDisplayConfiguration.DisplayBackgroundColor),
        nameof(BranchDisplayConfiguration.HeaderBackgroundColor),
        nameof(BranchDisplayConfiguration.MainTitleTextColor),
        nameof(BranchDisplayConfiguration.TableHeaderBackgroundColor),
        nameof(BranchDisplayConfiguration.TableHeaderTextColor),
        nameof(BranchDisplayConfiguration.TableRowBackgroundColor),
        nameof(BranchDisplayConfiguration.TableRowTextColor),
        nameof(BranchDisplayConfiguration.TicketNumberBackgroundColor),
        nameof(BranchDisplayConfiguration.TicketNumberTextColor),
        nameof(BranchDisplayConfiguration.TickerBackgroundColor),
        nameof(BranchDisplayConfiguration.TickerTextColor),
        nameof(BranchDisplayConfiguration.ClockBackgroundColor),
        nameof(BranchDisplayConfiguration.ClockTextColor)
    ];

    public void Configure(EntityTypeBuilder<BranchDisplayConfiguration> builder)
    {
        builder.ToTable("BranchDisplayConfiguration", table =>
        {
            foreach (var propertyName in ColorProperties)
            {
                table.HasCheckConstraint(
                    $"CK_BranchDisplayConfiguration_{propertyName}_Format",
                    $"[{propertyName}] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([{propertyName}]) = 7");
            }

            table.HasCheckConstraint(
                "CK_BranchDisplayConfiguration_MainTitleFontSize_Range",
                "[MainTitleFontSize] >= 1 AND [MainTitleFontSize] <= 100");
            table.HasCheckConstraint(
                "CK_BranchDisplayConfiguration_TickerFontSize_Range",
                "[TickerFontSize] >= 1 AND [TickerFontSize] <= 100");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BranchId).IsRequired();

        foreach (var propertyName in ColorProperties)
        {
            builder.Property<string>(propertyName)
                .IsRequired()
                .HasMaxLength(7)
                .IsUnicode(false);
        }

        ConfigureText(builder, x => x.MainTitleAr);
        ConfigureText(builder, x => x.MainTitleEn);
        ConfigureText(builder, x => x.TicketColumnTitleAr);
        ConfigureText(builder, x => x.TicketColumnTitleEn);
        ConfigureText(builder, x => x.ServiceColumnTitleAr);
        ConfigureText(builder, x => x.ServiceColumnTitleEn);
        ConfigureText(builder, x => x.WindowColumnTitleAr);
        ConfigureText(builder, x => x.WindowColumnTitleEn);

        builder.Property(x => x.MainTitleFontSize).IsRequired();
        builder.Property(x => x.TickerFontSize).IsRequired();
        builder.Property(x => x.ShowClock).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.Property(x => x.CreatedOnUtc).IsRequired().HasColumnType("datetime2(3)");
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.CreatedByApplicationUserId).IsRequired();
        builder.Property(x => x.LastModifiedByApplicationUserId);

        builder.HasIndex(x => x.BranchId).IsUnique();
        builder.HasIndex(x => x.CreatedByApplicationUserId);
        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasOne(x => x.Branch)
            .WithOne(x => x.DisplayConfiguration)
            .HasForeignKey<BranchDisplayConfiguration>(x => x.BranchId)
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

    private static void ConfigureText(
        EntityTypeBuilder<BranchDisplayConfiguration> builder,
        System.Linq.Expressions.Expression<Func<BranchDisplayConfiguration, string>> property)
    {
        builder.Property(property)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();
    }
}
