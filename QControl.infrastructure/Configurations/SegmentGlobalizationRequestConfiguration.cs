using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class SegmentGlobalizationRequestConfiguration
    : IEntityTypeConfiguration<SegmentGlobalizationRequest>,
      IWriteEntityConfiguration
{
    public void Configure(
        EntityTypeBuilder<SegmentGlobalizationRequest> builder)
    {
        builder.ToTable("SegmentGlobalizationRequest", table =>
        {
            table.HasCheckConstraint(
                "CK_SegmentGlobalizationRequest_Status",
                "[Status] IN (1, 2, 3)");
            table.HasCheckConstraint(
                "CK_SegmentGlobalizationRequest_ReviewAudit",
                "(([Status] = 1 AND [ReviewedByApplicationUserId] IS NULL AND [ReviewedOnUtc] IS NULL) OR ([Status] IN (2, 3) AND [ReviewedByApplicationUserId] IS NOT NULL AND [ReviewedOnUtc] IS NOT NULL))");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.SegmentId).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.RequestedByApplicationUserId).IsRequired();
        builder.Property(x => x.RequestedOnUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.ReviewedByApplicationUserId).IsRequired(false);
        builder.Property(x => x.ReviewedOnUtc).HasColumnType("datetime2").IsRequired(false);
        builder.Property(x => x.RejectionReason).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Segment)
            .WithMany()
            .HasForeignKey(x => x.SegmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RequestedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReviewedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BranchId, x.Status })
            .HasDatabaseName(
                "IX_SegmentGlobalizationRequest_BranchId_Status");
        builder.HasIndex(x => new { x.Status, x.RequestedOnUtc })
            .HasDatabaseName(
                "IX_SegmentGlobalizationRequest_Status_RequestedOnUtc");
        builder.HasIndex(
                x => x.SegmentId,
                "IX_SegmentGlobalizationRequest_SegmentId");
        builder.HasIndex(x => x.RequestedByApplicationUserId);
        builder.HasIndex(x => x.ReviewedByApplicationUserId);
        builder.HasIndex(
                x => x.SegmentId,
                "UX_SegmentGlobalizationRequest_SegmentId_Pending")
            .IsUnique()
            .HasFilter("[Status] = 1");
    }
}
