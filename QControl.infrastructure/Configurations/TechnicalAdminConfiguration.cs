using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qcontrol.Domain.Identity;
using QControl.Domain.Identity;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class TechnicalAdminConfiguration
    : IEntityTypeConfiguration<TechnicalAdmin>,
      IWriteEntityConfiguration
{
    public void Configure(
        EntityTypeBuilder<TechnicalAdmin> builder)
    {
        builder.ToTable("TechnicalAdmin");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.HasIndex(x => x.ApplicationUserId)
            .IsUnique();

        builder.HasOne(x => x.ApplicationUser)
            .WithOne()
            .HasForeignKey<TechnicalAdmin>(
                x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}