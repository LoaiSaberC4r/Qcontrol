using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qcontrol.Domain.Identity;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Ticket", table =>
        {
            table.HasCheckConstraint("CK_Ticket_Status_Valid", "[Status] IN (1,2,3,4,5,6)");
            table.HasCheckConstraint("CK_Ticket_Number_NotBlank", "LEN(LTRIM(RTRIM([TicketNumber]))) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.TicketNumber).HasMaxLength(50).IsUnicode(false).IsRequired();
        builder.Property(x => x.LookupValue).HasMaxLength(3000);
        builder.Property(x => x.BusinessDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CurrentQueueEnteredOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.CurrentServiceStartedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.CompletedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.CancelledOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.IssuingService).WithMany().HasForeignKey(x => x.IssuingServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CurrentService).WithMany().HasForeignKey(x => x.CurrentServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Segment).WithMany().HasForeignKey(x => x.SegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Reservation).WithMany().HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CurrentWindow).WithMany().HasForeignKey(x => x.CurrentWindowId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CurrentAssignedApplicationUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ServiceJourneys).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.History).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.CallAttempts).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.CustomInputValues).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.WorkflowSteps).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        foreach (var navigation in new[] { nameof(Ticket.ServiceJourneys), nameof(Ticket.History), nameof(Ticket.CallAttempts), nameof(Ticket.CustomInputValues), nameof(Ticket.WorkflowSteps) })
            builder.Navigation(navigation).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.BranchId, x.BusinessDate, x.IssuingServiceId, x.TicketNumber }).IsUnique().HasDatabaseName("UX_Ticket_Branch_BusinessDate_IssuingService_Number");
        builder.HasIndex(x => x.ReservationId).IsUnique().HasFilter("[ReservationId] IS NOT NULL").HasDatabaseName("UX_Ticket_ReservationId_NotNull");
        builder.HasIndex(x => new { x.BranchId, x.CurrentServiceId, x.Status, x.CurrentQueueEnteredOnUtc }).HasDatabaseName("IX_Ticket_QueueRead");
        builder.HasIndex(x => new { x.BranchId, x.Status });
        builder.HasIndex(x => new { x.BranchId, x.BusinessDate });
        builder.HasIndex(x => x.TicketNumber);
        builder.HasIndex(x => x.SegmentId);
    }
}

internal sealed class TicketServiceJourneyConfiguration : IEntityTypeConfiguration<TicketServiceJourney>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketServiceJourney> builder)
    {
        builder.ToTable("TicketServiceJourney");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EntryType).HasConversion<int>().IsRequired();
        builder.Property(x => x.Outcome).HasConversion<int?>();
        builder.Property(x => x.EnteredWaitingOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.ServiceStartedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.ServiceEndedOnUtc).HasColumnType("datetime2(3)");
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => new { x.TicketId, x.Id });
        builder.HasIndex(x => x.ServiceId);
        builder.HasIndex(x => new { x.TicketId, x.ServiceEndedOnUtc }).HasFilter("[ServiceEndedOnUtc] IS NULL");
    }
}

internal sealed class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.ToTable("TicketHistory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EventType).HasConversion<int>().IsRequired();
        builder.Property(x => x.FromStatus).HasConversion<int?>();
        builder.Property(x => x.ToStatus).HasConversion<int?>();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.OccurredOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.HasOne<Service>().WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(x => x.FromServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(x => x.ToServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Window>().WithMany().HasForeignKey(x => x.WindowId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.PerformerApplicationUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TicketId, x.OccurredOnUtc });
        builder.HasIndex(x => new { x.TicketId, x.EventType });
    }
}

internal sealed class TicketCallAttemptConfiguration : IEntityTypeConfiguration<TicketCallAttempt>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketCallAttempt> builder)
    {
        builder.ToTable("TicketCallAttempt");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CalledOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.HasOne(x => x.Window).WithMany().HasForeignKey(x => x.WindowId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.PerformerApplicationUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TicketId, x.CallCycleNumber, x.AttemptNumber }).IsUnique();
        builder.HasIndex(x => new { x.TicketId, x.CalledOnUtc });
    }
}

internal sealed class TicketWorkflowStepSnapshotConfiguration : IEntityTypeConfiguration<TicketWorkflowStepSnapshot>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketWorkflowStepSnapshot> builder)
    {
        builder.ToTable("TicketWorkflowStepSnapshot");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");
        builder.HasOne<ServiceWorkflow>().WithMany().HasForeignKey(x => x.SourceWorkflowId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TicketId, x.StepOrder }).IsUnique();
        builder.HasIndex(x => x.TicketId);
    }
}

internal sealed class TicketCustomInputValueConfiguration : IEntityTypeConfiguration<TicketCustomInputValue>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketCustomInputValue> builder)
    {
        builder.ToTable("TicketCustomInputValue");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.NameSnapshot).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LabelEnSnapshot).HasMaxLength(200);
        builder.Property(x => x.LabelArSnapshot).HasMaxLength(200);
        builder.Property(x => x.OrderSnapshot);
        builder.Property(x => x.TypeSnapshot).HasConversion<int>().IsRequired();
        builder.Property(x => x.Value).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.HasOne<ServiceCustomInput>().WithMany().HasForeignKey(x => x.ServiceCustomInputId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.TicketId);
    }
}
