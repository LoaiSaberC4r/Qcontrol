using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qcontrol.Domain.Identity;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class RolePermissionConfiguration
    : IEntityTypeConfiguration<RolePermission>,
      IWriteEntityConfiguration
{
    public void Configure(
        EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermission");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.PermissionId)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.HasIndex(x => x.RoleId);

        builder.HasIndex(x => x.PermissionId);

        builder.HasIndex(x => new
        {
            x.RoleId,
            x.PermissionId
        })
            .IsUnique();
    }
}