using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class DisplayWindowConfiguration
    : IEntityTypeConfiguration<DisplayWindow>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<DisplayWindow> builder)
    {
        builder.ToTable("DisplayWindow");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DisplayId)
            .IsRequired();

        builder.Property(x => x.WindowId)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.HasIndex(x => x.BranchId);

        builder.HasIndex(x => x.DisplayId);

        builder.HasIndex(x => x.WindowId);

        builder.HasIndex(x => new
        {
            x.DisplayId,
            x.WindowId
        })
            .IsUnique();

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasOne(x => x.Display)
            .WithMany(x => x.DisplayWindows)
            .HasForeignKey(x => new
            {
                x.DisplayId,
                x.BranchId
            })
            .HasPrincipalKey(x => new
            {
                x.Id,
                x.BranchId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Window)
            .WithMany(x => x.DisplayWindows)
            .HasForeignKey(x => new
            {
                x.WindowId,
                x.BranchId
            })
            .HasPrincipalKey(x => new
            {
                x.Id,
                x.BranchId
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
