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

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.HasIndex(x => x.DisplayId);

        builder.HasIndex(x => x.WindowId);

        builder.HasIndex(x => new
        {
            x.DisplayId,
            x.WindowId
        })
            .IsUnique();

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasOne(x => x.Display)
            .WithMany(x => x.DisplayWindows)
            .HasForeignKey(x => x.DisplayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Window)
            .WithMany(x => x.DisplayWindows)
            .HasForeignKey(x => x.WindowId)
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