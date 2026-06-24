using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchConfiguration
    : IEntityTypeConfiguration<Branch>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branch", table =>
        {
            table.HasCheckConstraint(
                "CK_Branch_NameRequired",
                "NULLIF(LTRIM(RTRIM([ArabicName])), N'') IS NOT NULL OR " +
                "NULLIF(LTRIM(RTRIM([EnglishName])), N'') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Branch_IPAddress_NotBlank",
                "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ArabicName)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.EnglishName)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.IPAddress)
            .IsRequired()
            .HasMaxLength(45)
            .IsUnicode(false);

        builder.Property(x => x.IsUpdatesAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.LastUpdated)
            .IsRequired()
            .HasColumnType("datetime2(3)");

        builder.Property(x => x.License)
            .IsRequired(false)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.HasIndex(x => x.IPAddress)
            .IsUnique();

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

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