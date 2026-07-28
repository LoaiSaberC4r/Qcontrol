using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchServiceSegmentConfiguration
    : IEntityTypeConfiguration<BranchServiceSegment>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchServiceSegment> builder)
    {
        builder.ToTable("BranchServiceSegment", table =>
            table.HasCheckConstraint(
                "CK_BranchServiceSegment_Quota_NonNegative",
                "[Quota] >= 0"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BranchServiceId).IsRequired();
        builder.Property(x => x.SegmentId).IsRequired();
        builder.Property(x => x.Quota).IsRequired();
        builder.Property(x => x.CreatedByApplicationUserId).IsRequired();
        builder.Property(x => x.LastModifiedByApplicationUserId).IsRequired(false);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasOne(x => x.BranchService)
            .WithMany()
            .HasForeignKey(x => x.BranchServiceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Segment)
            .WithMany()
            .HasForeignKey(x => x.SegmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LastModifiedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SegmentId)
            .HasDatabaseName("IX_BranchServiceSegment_SegmentId");
        builder.HasIndex(x => x.BranchServiceId)
            .HasDatabaseName("IX_BranchServiceSegment_BranchServiceId");
        builder.HasIndex(x => new { x.BranchServiceId, x.SegmentId })
            .IsUnique()
            .HasDatabaseName(
                "UX_BranchServiceSegment_BranchServiceId_SegmentId");
        builder.HasIndex(x => x.CreatedByApplicationUserId);
        builder.HasIndex(x => x.LastModifiedByApplicationUserId);
    }
}
