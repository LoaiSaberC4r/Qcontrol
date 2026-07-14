using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceWorkflowConfiguration
    : IEntityTypeConfiguration<ServiceWorkflow>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ServiceWorkflow> builder)
    {
        builder.ToTable("ServiceWorkflows", table =>
        {
            table.HasCheckConstraint(
                "CK_ServiceWorkflows_ArabicName_NotBlank",
                "LEN(LTRIM(RTRIM([ArabicName]))) > 0");

            table.HasCheckConstraint(
                "CK_ServiceWorkflows_EnglishName_NotBlank",
                "LEN(LTRIM(RTRIM([EnglishName]))) > 0");

            table.HasCheckConstraint(
                "CK_ServiceWorkflows_DefaultMustBeActive",
                "[IsDefault] = 0 OR [IsActive] = 1");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ArabicName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.LeafServiceId)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.ModifiedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(x => x.DeactivatedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(x => x.ReactivatedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.Property(x => x.DeactivatedByApplicationUserId)
            .IsRequired(false);

        builder.Property(x => x.ReactivatedByApplicationUserId)
            .IsRequired(false);

        builder.HasMany(x => x.Steps)
            .WithOne(x => x.ServiceWorkflow)
            .HasForeignKey(x => x.ServiceWorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeafService)
            .WithMany()
            .HasForeignKey(x => x.LeafServiceId)
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

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_ServiceWorkflows_IsActive");

        builder.HasIndex(x => x.BranchId)
            .HasDatabaseName("IX_ServiceWorkflows_BranchId");

        builder.HasIndex(x => x.LeafServiceId)
            .HasDatabaseName("IX_ServiceWorkflows_LeafServiceId");

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.LeafServiceId
        })
            .HasDatabaseName("IX_ServiceWorkflows_BranchId_LeafServiceId");

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.LeafServiceId,
            x.IsActive
        })
            .HasDatabaseName(
                "IX_ServiceWorkflows_BranchId_LeafServiceId_IsActive");

        builder.HasIndex(x => x.ArabicName)
            .HasDatabaseName("IX_ServiceWorkflows_ArabicName");

        builder.HasIndex(x => x.EnglishName)
            .HasDatabaseName("IX_ServiceWorkflows_EnglishName");

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.LeafServiceId,
            x.ArabicName
        })
            .IsUnique()
            .HasDatabaseName(
                "UX_ServiceWorkflows_BranchId_LeafServiceId_ArabicName");

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.LeafServiceId,
            x.EnglishName
        })
            .IsUnique()
            .HasDatabaseName(
                "UX_ServiceWorkflows_BranchId_LeafServiceId_EnglishName");

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.LeafServiceId
        })
            .IsUnique()
            .HasFilter("[IsDefault] = 1")
            .HasDatabaseName(
                "UX_ServiceWorkflows_BranchId_LeafServiceId_Default");

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasIndex(x => x.DeactivatedByApplicationUserId);

        builder.HasIndex(x => x.ReactivatedByApplicationUserId);
    }
}
