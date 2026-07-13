using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceGlobalizationRequestConfiguration
    : IEntityTypeConfiguration<ServiceGlobalizationRequest>,
      IWriteEntityConfiguration
{
    public void Configure(
        EntityTypeBuilder<ServiceGlobalizationRequest> builder)
    {
        builder.ToTable("ServiceGlobalizationRequest", table =>
        {
            table.HasCheckConstraint(
                "CK_ServiceGlobalizationRequest_Status",
                "[Status] IN (1, 2, 3)");

            table.HasCheckConstraint(
                "CK_ServiceGlobalizationRequest_RequestType",
                "[RequestType] IN (1, 2)");

            table.HasCheckConstraint(
                "CK_ServiceGlobalizationRequest_ReviewAudit",
                "(([Status] = 1 AND [ReviewedByApplicationUserId] IS NULL AND [ReviewedOnUtc] IS NULL) OR " +
                "([Status] IN (2, 3) AND [ReviewedByApplicationUserId] IS NOT NULL AND [ReviewedOnUtc] IS NOT NULL))");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.RootServiceId)
            .IsRequired();

        builder.Property(x => x.RequestType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.RequestedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.RequestedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.ReviewedByApplicationUserId)
            .IsRequired(false);

        builder.Property(x => x.ReviewedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RootService)
            .WithMany()
            .HasForeignKey(x => x.RootServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Request)
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new
            {
                x.Status,
                x.RequestedOnUtc
            })
            .HasDatabaseName(
                "IX_ServiceGlobalizationRequest_Status_RequestedOnUtc");

        builder.HasIndex(x => new
            {
                x.BranchId,
                x.Status
            })
            .HasDatabaseName(
                "IX_ServiceGlobalizationRequest_BranchId_Status");

        builder.HasIndex(x => x.RootServiceId)
            .HasDatabaseName("IX_ServiceGlobalizationRequest_RootServiceId");

        builder.HasIndex(x => x.RequestedByApplicationUserId)
            .HasDatabaseName(
                "IX_ServiceGlobalizationRequest_RequestedByApplicationUserId");

        builder.HasIndex(x => x.ReviewedByApplicationUserId)
            .HasDatabaseName(
                "IX_ServiceGlobalizationRequest_ReviewedByApplicationUserId");

        builder.HasIndex(x => x.RootServiceId)
            .IsUnique()
            .HasFilter("[Status] = 1")
            .HasDatabaseName(
                "UX_ServiceGlobalizationRequest_RootServiceId_Pending");
    }
}
