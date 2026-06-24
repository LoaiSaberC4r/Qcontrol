using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class DisplayConfiguration
    : IEntityTypeConfiguration<Display>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Display> builder)
    {
        builder.ToTable("Display", table =>
        {
            table.HasCheckConstraint(
                "CK_Display_Number_Positive",
                "[Number] > 0");

            table.HasCheckConstraint(
                "CK_Display_IPAddress_NotBlank",
                "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.SerialNo)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.Number)
            .IsRequired();

        builder.Property(x => x.IPAddress)
            .IsRequired()
            .HasMaxLength(45)
            .IsUnicode(false);

        builder.Property(x => x.Type)
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

        builder.HasIndex(x => x.SerialNo)
            .IsUnique()
            .HasFilter("[SerialNo] IS NOT NULL");

        builder.HasIndex(x => x.IPAddress);

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Displays)
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