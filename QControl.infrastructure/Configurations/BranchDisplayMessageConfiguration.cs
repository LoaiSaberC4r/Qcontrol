using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchDisplayMessageConfiguration
    : IEntityTypeConfiguration<BranchDisplayMessage>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchDisplayMessage> builder)
    {
        builder.ToTable("BranchDisplayMessage", table =>
        {
            table.HasCheckConstraint(
                "CK_BranchDisplayMessage_DisplayOrder_Positive",
                "[DisplayOrder] > 0");
            table.HasCheckConstraint(
                "CK_BranchDisplayMessage_TextAr_NotBlank",
                "NULLIF(LTRIM(RTRIM([TextAr])), N'') IS NOT NULL");
            table.HasCheckConstraint(
                "CK_BranchDisplayMessage_TextEn_NotBlank",
                "NULLIF(LTRIM(RTRIM([TextEn])), N'') IS NOT NULL");
            table.HasCheckConstraint(
                "CK_BranchDisplayMessage_DeactivationAudit_Pair",
                "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");
            table.HasCheckConstraint(
                "CK_BranchDisplayMessage_ReactivationAudit_Pair",
                "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.TextAr).IsRequired().HasMaxLength(500).IsUnicode();
        builder.Property(x => x.TextEn).IsRequired().HasMaxLength(500).IsUnicode();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.Property(x => x.CreatedOnUtc).IsRequired().HasColumnType("datetime2(3)");
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.CreatedByApplicationUserId).IsRequired();
        builder.Property(x => x.LastModifiedByApplicationUserId);
        builder.Property(x => x.DeactivatedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.DeactivatedByApplicationUserId);
        builder.Property(x => x.ReactivatedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.ReactivatedByApplicationUserId);

        builder.HasIndex(x => new { x.BranchId, x.DisplayOrder }).IsUnique();
        builder.HasIndex(x => new { x.BranchId, x.IsActive, x.DisplayOrder });
        builder.HasIndex(x => x.CreatedByApplicationUserId);
        builder.HasIndex(x => x.LastModifiedByApplicationUserId);
        builder.HasIndex(x => x.DeactivatedByApplicationUserId);
        builder.HasIndex(x => x.ReactivatedByApplicationUserId);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.DisplayMessages)
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
