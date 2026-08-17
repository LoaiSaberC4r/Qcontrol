using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchVideoConfiguration
    : IEntityTypeConfiguration<BranchVideo>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchVideo> builder)
    {
        builder.ToTable("BranchVideo", table =>
        {
            table.HasCheckConstraint(
                "CK_BranchVideo_DisplayOrder_Positive",
                "[DisplayOrder] > 0");
            table.HasCheckConstraint(
                "CK_BranchVideo_OriginalFileName_NotBlank",
                "NULLIF(LTRIM(RTRIM([OriginalFileName])), '') IS NOT NULL");
            table.HasCheckConstraint(
                "CK_BranchVideo_OriginalPath_NotBlank",
                "NULLIF(LTRIM(RTRIM([OriginalPath])), '') IS NOT NULL");
            table.HasCheckConstraint(
                "CK_BranchVideo_DeactivationAudit_Pair",
                "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR " +
                "([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");
            table.HasCheckConstraint(
                "CK_BranchVideo_ReactivationAudit_Pair",
                "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR " +
                "([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BranchId).IsRequired();

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);
        builder.Property(x => x.OriginalPath)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode(false);
        builder.Property(x => x.HlsManifestPath)
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ProcessingStatus).IsRequired();
        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired()
            .HasColumnType("datetime2(3)");
        builder.Property(x => x.ModifiedOnUtc)
            .HasColumnType("datetime2(3)");
        builder.Property(x => x.CreatedByApplicationUserId).IsRequired();
        builder.Property(x => x.LastModifiedByApplicationUserId);
        builder.Property(x => x.DeactivatedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.DeactivatedByApplicationUserId);
        builder.Property(x => x.ReactivatedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.ReactivatedByApplicationUserId);

        builder.HasIndex(x => new { x.BranchId, x.DisplayOrder })
            .IsUnique();
        builder.HasIndex(x => new
        {
            x.BranchId,
            x.IsActive,
            x.ProcessingStatus,
            x.DisplayOrder
        });
        builder.HasIndex(x => x.CreatedByApplicationUserId);
        builder.HasIndex(x => x.LastModifiedByApplicationUserId);
        builder.HasIndex(x => x.DeactivatedByApplicationUserId);
        builder.HasIndex(x => x.ReactivatedByApplicationUserId);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Videos)
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
