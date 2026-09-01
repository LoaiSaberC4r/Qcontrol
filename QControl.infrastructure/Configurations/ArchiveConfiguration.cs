using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.infrastructure.Persistence;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class TicketArchiveConfiguration : IEntityTypeConfiguration<TicketArchive>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketArchive> b)
    {
        b.ToTable("TicketArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.TicketNumber).HasMaxLength(50).IsUnicode(false).IsRequired(); b.Property(x => x.LookupValue).HasMaxLength(3000); b.Property(x => x.BusinessDate).HasColumnType("date"); b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.CurrentQueueEnteredOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.CurrentServiceStartedOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.CompletedOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.CancelledOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.ArchivedOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.CancellationReason).HasMaxLength(500);
        b.HasIndex(x => x.OriginalTicketId).IsUnique(); b.HasIndex(x => new { x.BranchId, x.BusinessDate }); b.HasIndex(x => new { x.BranchId, x.Status }); b.HasIndex(x => new { x.BranchId, x.IssuingServiceId }); b.HasIndex(x => x.CurrentServiceId); b.HasIndex(x => x.SegmentId); b.HasIndex(x => x.TicketNumber);
        b.HasMany(x => x.Journeys).WithOne(x => x.TicketArchive).HasForeignKey(x => x.TicketArchiveId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.History).WithOne(x => x.TicketArchive).HasForeignKey(x => x.TicketArchiveId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.CallAttempts).WithOne(x => x.TicketArchive).HasForeignKey(x => x.TicketArchiveId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.CustomInputs).WithOne(x => x.TicketArchive).HasForeignKey(x => x.TicketArchiveId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.WorkflowSteps).WithOne(x => x.TicketArchive).HasForeignKey(x => x.TicketArchiveId).OnDelete(DeleteBehavior.Cascade);
    }
}
internal sealed class TicketServiceJourneyArchiveConfiguration : IEntityTypeConfiguration<TicketServiceJourneyArchive>, IWriteEntityConfiguration
{ public void Configure(EntityTypeBuilder<TicketServiceJourneyArchive> b) { b.ToTable("TicketServiceJourneyArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd(); b.Property(x => x.EntryType).HasConversion<int>(); b.Property(x => x.Outcome).HasConversion<int?>(); b.Property(x => x.EnteredWaitingOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.ServiceStartedOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.ServiceEndedOnUtc).HasColumnType("datetime2(3)"); b.HasIndex(x => new { x.TicketArchiveId, x.OriginalJourneyId }).IsUnique(); b.HasIndex(x => x.ServiceId); } }
internal sealed class TicketHistoryArchiveConfiguration : IEntityTypeConfiguration<TicketHistoryArchive>, IWriteEntityConfiguration
{ public void Configure(EntityTypeBuilder<TicketHistoryArchive> b) { b.ToTable("TicketHistoryArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd(); b.Property(x => x.EventType).HasConversion<int>(); b.Property(x => x.FromStatus).HasConversion<int?>(); b.Property(x => x.ToStatus).HasConversion<int?>(); b.Property(x => x.Reason).HasMaxLength(500); b.Property(x => x.OccurredOnUtc).HasColumnType("datetime2(3)"); b.HasIndex(x => new { x.TicketArchiveId, x.OccurredOnUtc }); b.HasIndex(x => x.ServiceId); b.HasIndex(x => x.FromServiceId); b.HasIndex(x => x.ToServiceId); } }
internal sealed class TicketCallAttemptArchiveConfiguration : IEntityTypeConfiguration<TicketCallAttemptArchive>, IWriteEntityConfiguration
{ public void Configure(EntityTypeBuilder<TicketCallAttemptArchive> b) { b.ToTable("TicketCallAttemptArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd(); b.Property(x => x.CalledOnUtc).HasColumnType("datetime2(3)"); b.HasIndex(x => new { x.TicketArchiveId, x.CallCycleNumber, x.AttemptNumber }).IsUnique(); } }
internal sealed class TicketCustomInputValueArchiveConfiguration : IEntityTypeConfiguration<TicketCustomInputValueArchive>, IWriteEntityConfiguration
{ public void Configure(EntityTypeBuilder<TicketCustomInputValueArchive> b) { b.ToTable("TicketCustomInputValueArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd(); b.Property(x => x.NameSnapshot).HasMaxLength(100); b.Property(x => x.LabelEnSnapshot).HasMaxLength(200); b.Property(x => x.LabelArSnapshot).HasMaxLength(200); b.Property(x => x.TypeSnapshot).HasConversion<int>(); b.Property(x => x.Value).HasMaxLength(3000); b.HasIndex(x => x.TicketArchiveId); } }
internal sealed class TicketWorkflowStepSnapshotArchiveConfiguration : IEntityTypeConfiguration<TicketWorkflowStepSnapshotArchive>, IWriteEntityConfiguration
{ public void Configure(EntityTypeBuilder<TicketWorkflowStepSnapshotArchive> b) { b.ToTable("TicketWorkflowStepSnapshotArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd(); b.HasIndex(x => new { x.TicketArchiveId, x.StepOrder }).IsUnique(); b.HasIndex(x => x.ServiceId); } }

internal sealed class ReservationArchiveConfiguration : IEntityTypeConfiguration<ReservationArchive>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ReservationArchive> b)
    {
        b.ToTable("ReservationArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd(); b.Property(x => x.LookupValue).HasMaxLength(3000); b.Property(x => x.ScheduledOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.BusinessDate).HasColumnType("date"); b.Property(x => x.Status).HasConversion<int>(); b.Property(x => x.CancellationReason).HasMaxLength(500); b.Property(x => x.CancelledOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.ConvertedToTicketOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.ExpiredOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)"); b.Property(x => x.ArchivedOnUtc).HasColumnType("datetime2(3)");
        b.HasIndex(x => x.OriginalReservationId).IsUnique(); b.HasIndex(x => new { x.BranchId, x.BusinessDate }); b.HasIndex(x => new { x.BranchId, x.ServiceId, x.Status }); b.HasIndex(x => x.SegmentId);
        b.HasMany(x => x.History).WithOne(x => x.ReservationArchive).HasForeignKey(x => x.ReservationArchiveId).OnDelete(DeleteBehavior.Cascade); b.HasMany(x => x.CustomInputs).WithOne(x => x.ReservationArchive).HasForeignKey(x => x.ReservationArchiveId).OnDelete(DeleteBehavior.Cascade);
    }
}
internal sealed class ReservationHistoryArchiveConfiguration : IEntityTypeConfiguration<ReservationHistoryArchive>, IWriteEntityConfiguration
{ public void Configure(EntityTypeBuilder<ReservationHistoryArchive> b) { b.ToTable("ReservationHistoryArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd(); b.Property(x => x.EventType).HasConversion<int>(); b.Property(x => x.FromStatus).HasConversion<int?>(); b.Property(x => x.ToStatus).HasConversion<int?>(); b.Property(x => x.Reason).HasMaxLength(500); b.Property(x => x.OccurredOnUtc).HasColumnType("datetime2(3)"); b.HasIndex(x => new { x.ReservationArchiveId, x.OccurredOnUtc }); } }
internal sealed class ReservationCustomInputValueArchiveConfiguration : IEntityTypeConfiguration<ReservationCustomInputValueArchive>, IWriteEntityConfiguration
{ public void Configure(EntityTypeBuilder<ReservationCustomInputValueArchive> b) { b.ToTable("ReservationCustomInputValueArchive"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd(); b.Property(x => x.NameSnapshot).HasMaxLength(100); b.Property(x => x.LabelEnSnapshot).HasMaxLength(200); b.Property(x => x.LabelArSnapshot).HasMaxLength(200); b.Property(x => x.TypeSnapshot).HasConversion<int>(); b.Property(x => x.Value).HasMaxLength(3000); b.HasIndex(x => x.ReservationArchiveId); } }
