using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchServiceConfiguration
    : IEntityTypeConfiguration<BranchService>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchService> builder)
    {
        builder.ToTable("BranchService");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ServiceId)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.HasIndex(x => x.BranchId)
            .HasDatabaseName("IX_BranchService_BranchId");

        builder.HasIndex(x => x.ServiceId)
            .HasDatabaseName("IX_BranchService_ServiceId");

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.ServiceId
        })
        .IsUnique()
        .HasDatabaseName("UX_BranchService_BranchId_ServiceId");

        builder.HasIndex(x => x.CreatedByApplicationUserId)
            .HasDatabaseName("IX_BranchService_CreatedByApplicationUserId");

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.BranchServices)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.BranchServices)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
