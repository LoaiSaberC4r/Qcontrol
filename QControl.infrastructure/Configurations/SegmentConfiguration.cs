using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class SegmentConfiguration
    : IEntityTypeConfiguration<Segment>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Segment> builder)
    {
        builder.ToTable("Segment", table =>
        {
            table.HasCheckConstraint(
                "CK_Segment_Priority_NonNegative",
                "[Priority] >= 0");
            table.HasCheckConstraint(
                "CK_Segment_Scope_Owner",
                "([Scope] = 1 AND [OwnerBranchId] IS NULL) OR ([Scope] = 2 AND [OwnerBranchId] IS NOT NULL)");
            table.HasCheckConstraint(
                "CK_Segment_SystemDefault",
                "[IsSystemDefault] = 0 OR ([Scope] = 1 AND [OwnerBranchId] IS NULL AND [Priority] = 0)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ArabicName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EnglishName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.Scope).HasConversion<int>().IsRequired();
        builder.Property(x => x.OwnerBranchId).IsRequired(false);
        builder.Property(x => x.IsSystemDefault).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedByApplicationUserId).IsRequired();
        builder.Property(x => x.LastModifiedByApplicationUserId).IsRequired(false);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasOne(x => x.OwnerBranch)
            .WithMany()
            .HasForeignKey(x => x.OwnerBranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LastModifiedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Scope, x.OwnerBranchId })
            .HasDatabaseName("IX_Segment_Scope_OwnerBranchId");
        builder.HasIndex(x => x.Priority)
            .HasDatabaseName("IX_Segment_Priority");
        builder.HasIndex(x => x.ArabicName)
            .HasDatabaseName("IX_Segment_ArabicName");
        builder.HasIndex(x => x.EnglishName)
            .HasDatabaseName("IX_Segment_EnglishName");
        builder.HasIndex(x => x.CreatedByApplicationUserId);
        builder.HasIndex(x => x.LastModifiedByApplicationUserId);
        builder.HasIndex(x => x.IsSystemDefault)
            .IsUnique()
            .HasFilter("[IsSystemDefault] = 1")
            .HasDatabaseName("UX_Segment_SystemDefault");
    }
}
