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
                "CK_Terminal_Number_Positive",
                "[Number] > 0");

            table.HasCheckConstraint(
                "CK_Terminal_IPAddress_NotBlank",
                "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SerialNo)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.Number)
            .IsRequired();

        builder.Property(x => x.IPAddress)
            .IsRequired()
            .HasMaxLength(45)
            .IsUnicode(false);

        builder.Property(x => x.WindowId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired(false)
            .HasMaxLength(100);

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
            .IsUnique()
            .HasFilter("[SerialNo] IS NOT NULL");

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