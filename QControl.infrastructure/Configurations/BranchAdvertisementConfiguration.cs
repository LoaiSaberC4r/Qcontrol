using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchAdvertisementConfiguration
    : IEntityTypeConfiguration<BranchAdvertisement>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchAdvertisement> builder)
    {
        builder.ToTable("BranchAdvertisement", table =>
        {
            table.HasCheckConstraint(
                "CK_BranchAdvertisement_ImagePath_NotBlank",
                "NULLIF(LTRIM(RTRIM([ImagePath])), '') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_BranchAdvertisement_DisplayOrder_Range",
                "(([DisplayOrder] >= 1 AND [DisplayOrder] <= 20) OR ([DisplayOrder] >= 1001 AND [DisplayOrder] <= 1020))");

            table.HasCheckConstraint(
                "CK_BranchAdvertisement_DeactivationAudit_Pair",
                "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR " +
                "([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");

            table.HasCheckConstraint(
                "CK_BranchAdvertisement_ReactivationAudit_Pair",
                "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR " +
                "([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ImagePath)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

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

        builder.Property(x => x.DeactivatedOnUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(3)");

        builder.Property(x => x.DeactivatedByApplicationUserId)
            .IsRequired(false);

        builder.Property(x => x.ReactivatedOnUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(3)");

        builder.Property(x => x.ReactivatedByApplicationUserId)
            .IsRequired(false);

        builder.HasIndex(x => x.BranchId);

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.DisplayOrder
        })
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.IsActive,
            x.DisplayOrder
        });

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasIndex(x => x.DeactivatedByApplicationUserId);

        builder.HasIndex(x => x.ReactivatedByApplicationUserId);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Advertisements)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastModifiedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DeactivatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.DeactivatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReactivatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ReactivatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
