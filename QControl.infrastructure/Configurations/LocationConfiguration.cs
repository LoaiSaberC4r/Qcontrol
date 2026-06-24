using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class LocationConfiguration
    : IEntityTypeConfiguration<Location>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Location");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Governorate)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.Area)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.Longitude)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.Latitude)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.HasIndex(x => x.BranchId)
            .IsUnique();

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasOne(x => x.Branch)
            .WithOne(x => x.Location)
            .HasForeignKey<Location>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastModifiedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}