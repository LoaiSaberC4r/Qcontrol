using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class WaitingAreaConfiguration
    : IEntityTypeConfiguration<WaitingArea>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<WaitingArea> builder)
    {
        builder.ToTable("WaitingArea", table =>
        {
            table.HasCheckConstraint(
                "CK_WaitingArea_Number_Positive",
                "[Number] > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Number)
            .IsRequired();

        builder.Property(x => x.AudioDevice)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.ControlDevice)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.DescriptiveName)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.HasIndex(x => x.BranchId);

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.Number
        })
            .IsUnique();

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.WaitingAreas)
            .HasForeignKey(x => x.BranchId)
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