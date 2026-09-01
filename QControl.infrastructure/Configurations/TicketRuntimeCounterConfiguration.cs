using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class TicketNumberSequenceConfiguration : IEntityTypeConfiguration<TicketNumberSequence>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketNumberSequence> builder)
    {
        builder.ToTable("TicketNumberSequence");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BusinessDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");
        builder.HasOne<Branch>().WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BranchId, x.ServiceId, x.BusinessDate }).IsUnique();
    }
}

internal sealed class BranchServiceSegmentDailyUsageConfiguration : IEntityTypeConfiguration<BranchServiceSegmentDailyUsage>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchServiceSegmentDailyUsage> builder)
    {
        builder.ToTable("BranchServiceSegmentDailyUsage", table => table.HasCheckConstraint("CK_BranchServiceSegmentDailyUsage_Consumed_NonNegative", "[ConsumedCount] >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BusinessDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");
        builder.HasOne(x => x.BranchServiceSegment).WithMany().HasForeignKey(x => x.BranchServiceSegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BranchServiceSegmentId, x.BusinessDate }).IsUnique();
    }
}
