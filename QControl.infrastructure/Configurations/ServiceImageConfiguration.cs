using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceImageConfiguration
    : IEntityTypeConfiguration<ServiceImage>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ServiceImage> builder)
    {
        builder.ToTable("ServiceImage", table =>
        {
            table.HasCheckConstraint(
                "CK_ServiceImage_ImagePath_NotBlank",
                "LEN(LTRIM(RTRIM([ImagePath]))) > 0");

            table.HasCheckConstraint(
                "CK_ServiceImage_DisplayOrder_NonNegative",
                "[DisplayOrder] >= 0");

            table.HasCheckConstraint(
                "CK_ServiceImage_ImageType_Valid",
                "[ImageType] IN (1, 2, 3)");
        });

        builder.HasKey(x => x.Id);

        builder.HasQueryFilter(x => !x.Service.IsDeleted);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ServiceId)
            .IsRequired();

        builder.Property(x => x.ImagePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ImageType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastModifiedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ServiceId);

        builder.HasIndex(x => new
        {
            x.ServiceId,
            x.ImageType
        });

        builder.HasIndex(x => new
        {
            x.ServiceId,
            x.ImageType,
            x.DisplayOrder
        });

        builder.HasIndex(x => new
        {
            x.ServiceId,
            x.ImageType
        })
        .IsUnique()
        .HasFilter("[ImageType] IN (1, 2)")
        .HasDatabaseName("UX_ServiceImage_ServiceId_ImageType_LogoIcon");
    }
}
