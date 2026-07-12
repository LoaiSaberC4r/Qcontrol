using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qcontrol.Domain.Identity;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ApplicationUserBranchConfiguration
    : IEntityTypeConfiguration<ApplicationUserBranch>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ApplicationUserBranch> builder)
    {
        builder.ToTable("ApplicationUserBranch");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.HasIndex(x => x.ApplicationUserId)
            .HasDatabaseName("IX_ApplicationUserBranch_ApplicationUserId");

        builder.HasIndex(x => x.BranchId)
            .HasDatabaseName("IX_ApplicationUserBranch_BranchId");

        builder.HasIndex(x => new
        {
            x.ApplicationUserId,
            x.BranchId
        })
            .IsUnique()
            .HasDatabaseName("UX_ApplicationUserBranch_ApplicationUserId_BranchId");

        builder.HasOne(x => x.ApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
