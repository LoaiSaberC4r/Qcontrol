using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qcontrol.Domain.Identity;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchAdminConfiguration
    : IEntityTypeConfiguration<BranchAdmin>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchAdmin> builder)
    {
        builder.ToTable("BranchAdmin");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.HasIndex(x => x.ApplicationUserId)
            .IsUnique()
            .HasDatabaseName("UX_BranchAdmin_ApplicationUserId");

        builder.HasOne(x => x.ApplicationUser)
            .WithOne()
            .HasForeignKey<BranchAdmin>(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
