using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class TerminalConfiguration
    : IEntityTypeConfiguration<Terminal>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Terminal> builder)
    {
        builder.ToTable("Terminal", table =>
        {
            table.HasCheckConstraint(
                "CK_Terminal_Number_NotBlank",
                "NULLIF(LTRIM(RTRIM([Number])), '') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Terminal_IPAddress_NotBlank",
                "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Terminal_SerialNo_NotBlank",
                "NULLIF(LTRIM(RTRIM([SerialNo])), '') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Terminal_Type_NotBlank",
                "NULLIF(LTRIM(RTRIM([Type])), '') IS NOT NULL");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

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

        builder.Property(x => x.WindowId)
            .IsRequired();

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

        builder.HasIndex(x => x.WindowId);

        builder.HasIndex(x => new
        {
            x.WindowId,
            x.Number
        })
            .IsUnique();

        builder.HasIndex(x => x.SerialNo)
            .IsUnique(false);

        builder.HasIndex(x => x.IPAddress);

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasOne(x => x.Window)
            .WithMany(x => x.Terminals)
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
