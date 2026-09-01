using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qcontrol.Domain.Identity;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservation", table => table.HasCheckConstraint("CK_Reservation_Status_Valid", "[Status] IN (1,2,3,4,5)"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ScheduledOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.BusinessDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.CancelledOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.ConvertedToTicketOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.ExpiredOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Segment).WithMany().HasForeignKey(x => x.SegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.BranchServiceSegment).WithMany().HasForeignKey(x => x.BranchServiceSegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CancelledByApplicationUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.History).WithOne(x => x.Reservation).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.CustomInputValues).WithOne(x => x.Reservation).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.History).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.CustomInputValues).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.BranchId, x.ScheduledOnUtc });
        builder.HasIndex(x => new { x.BranchId, x.ServiceId, x.Status });
        builder.HasIndex(x => x.SegmentId);
        builder.HasIndex(x => x.Status);
    }
}

internal sealed class ReservationHistoryConfiguration : IEntityTypeConfiguration<ReservationHistory>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ReservationHistory> builder)
    {
        builder.ToTable("ReservationHistory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EventType).HasConversion<int>().IsRequired();
        builder.Property(x => x.FromStatus).HasConversion<int?>();
        builder.Property(x => x.ToStatus).HasConversion<int?>();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.OccurredOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.PerformerApplicationUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ReservationId, x.OccurredOnUtc });
    }
}

internal sealed class ReservationCustomInputValueConfiguration : IEntityTypeConfiguration<ReservationCustomInputValue>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ReservationCustomInputValue> builder)
    {
        builder.ToTable("ReservationCustomInputValue");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.NameSnapshot).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LabelEnSnapshot).HasMaxLength(200);
        builder.Property(x => x.LabelArSnapshot).HasMaxLength(200);
        builder.Property(x => x.TypeSnapshot).HasConversion<int>().IsRequired();
        builder.Property(x => x.Value).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.HasOne<ServiceCustomInput>().WithMany().HasForeignKey(x => x.ServiceCustomInputId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.ReservationId);
    }
}
