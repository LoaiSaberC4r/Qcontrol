using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qcontrol.Domain.Identity;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class PermissionConfiguration
    : IEntityTypeConfiguration<Permission>,
      IWriteEntityConfiguration
{
    public void Configure(
        EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permission");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsSystemPermission)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}