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
                "CK_Display_Number_NotBlank",
                "NULLIF(LTRIM(RTRIM([Number])), '') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Display_IPAddress_NotBlank",
                "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Display_SerialNo_NotBlank",
                "NULLIF(LTRIM(RTRIM([SerialNo])), '') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Display_Type_NotBlank",
                "NULLIF(LTRIM(RTRIM([Type])), '') IS NOT NULL");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.SerialNo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.IPAddress)
            .IsRequired()
            .HasMaxLength(45)
            .IsUnicode(false);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.DeletedOnUtc)
            .IsRequired(false);

        builder.Property(x => x.RestoredOnUtc)
            .IsRequired(false);

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

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.IPAddress
        })
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.SerialNo
        })
            .IsUnique();

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
