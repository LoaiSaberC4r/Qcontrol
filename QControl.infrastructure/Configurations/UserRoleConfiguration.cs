using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qcontrol.Domain.Identity;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class UserRoleConfiguration
    : IEntityTypeConfiguration<UserRole>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.HasIndex(x => x.ApplicationUserId);

        builder.HasIndex(x => x.RoleId);

        builder.HasIndex(x => new
        {
            x.ApplicationUserId,
            x.RoleId
        })
        .IsUnique();

        builder.HasOne(x => x.ApplicationUser)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}