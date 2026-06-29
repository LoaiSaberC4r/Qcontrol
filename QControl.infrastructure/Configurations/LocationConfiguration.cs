using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class LocationConfiguration
    : IEntityTypeConfiguration<Location>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Location", table =>
        {
            table.HasCheckConstraint(
                "CK_Location_Governorate_Required",
                "NULLIF(LTRIM(RTRIM([Governorate])), N'') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Location_City_Required",
                "NULLIF(LTRIM(RTRIM([City])), N'') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Location_Area_Required",
                "NULLIF(LTRIM(RTRIM([Area])), N'') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Location_Address_Required",
                "NULLIF(LTRIM(RTRIM([Address])), N'') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Location_Latitude_Range",
                "[Latitude] >= -90 AND [Latitude] <= 90");

            table.HasCheckConstraint(
                "CK_Location_Longitude_Range",
                "[Longitude] >= -180 AND [Longitude] <= 180");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Governorate)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Area)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Latitude)
            .IsRequired()
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.Longitude)
            .IsRequired()
            .HasColumnType("decimal(9,6)");

        builder.HasIndex(x => x.BranchId)
            .IsUnique();

        builder.HasOne(x => x.Branch)
            .WithOne(x => x.Location)
            .HasForeignKey<Location>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
