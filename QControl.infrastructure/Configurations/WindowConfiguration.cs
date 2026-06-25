using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class WindowConfiguration
    : IEntityTypeConfiguration<Window>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Window> builder)
    {
        builder.ToTable("Window", table =>
        {
            table.HasCheckConstraint(
                "CK_Window_Number_NotBlank",
                "NULLIF(LTRIM(RTRIM([Number])), N'') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Window_IPAddress_NotBlank",
                "[IPAddress] IS NULL OR " +
                "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DescriptiveName)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.WaitingAreaId)
            .IsRequired();

        builder.Property(x => x.IPAddress)
            .IsRequired(false)
            .HasMaxLength(45)
            .IsUnicode(false);

        builder.Property(x => x.EnableTicketBooking)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.EnableDirectCall)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasColumnType("bit")
            .HasDefaultValue(false);

        builder.Property(x => x.DeletedOnUtc)
            .IsRequired(false)
            .HasColumnType("datetime2");

        builder.Property(x => x.RestoredOnUtc)
            .IsRequired(false)
            .HasColumnType("datetime2");

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.HasIndex(x => x.WaitingAreaId);

        builder.HasIndex(x => new
        {
            x.WaitingAreaId,
            x.Number
        })
            .IsUnique()
            .HasDatabaseName("UX_Window_WaitingAreaId_Number");

        builder.HasIndex(x => new
        {
            x.WaitingAreaId,
            x.IPAddress
        })
            .IsUnique()
            .HasFilter("[IPAddress] IS NOT NULL")
            .HasDatabaseName("UX_Window_WaitingAreaId_IPAddress");

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasOne(x => x.WaitingArea)
            .WithMany(x => x.Windows)
            .HasForeignKey(x => x.WaitingAreaId)
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
