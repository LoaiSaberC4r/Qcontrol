using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceWorkflowStepConfiguration
    : IEntityTypeConfiguration<ServiceWorkflowStep>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ServiceWorkflowStep> builder)
    {
        builder.ToTable("ServiceWorkflowSteps", table =>
        {
            table.HasCheckConstraint(
                "CK_ServiceWorkflowSteps_StepOrder_Positive",
                "[StepOrder] > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ServiceWorkflowId)
            .IsRequired();

        builder.Property(x => x.ServiceId)
            .IsRequired();

        builder.Property(x => x.StepOrder)
            .IsRequired();

        builder.HasOne(x => x.ServiceWorkflow)
            .WithMany(x => x.Steps)
            .HasForeignKey(x => x.ServiceWorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Service)
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ServiceWorkflowId)
            .HasDatabaseName("IX_ServiceWorkflowSteps_ServiceWorkflowId");

        builder.HasIndex(x => x.ServiceId)
            .HasDatabaseName("IX_ServiceWorkflowSteps_ServiceId");

        builder.HasIndex(x => new
        {
            x.ServiceWorkflowId,
            x.StepOrder
        })
            .HasDatabaseName("IX_ServiceWorkflowSteps_ServiceWorkflowId_StepOrder");

        builder.HasIndex(x => new
        {
            x.ServiceWorkflowId,
            x.StepOrder
        })
            .IsUnique()
            .HasDatabaseName("UX_ServiceWorkflowSteps_ServiceWorkflowId_StepOrder");
    }
}
