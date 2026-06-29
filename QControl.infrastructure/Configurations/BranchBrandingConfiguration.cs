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
}
