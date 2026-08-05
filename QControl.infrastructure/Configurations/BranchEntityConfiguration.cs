using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchEntityConfiguration
    : IEntityTypeConfiguration<Branch>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branch", table =>
        {
            table.HasCheckConstraint(
                "CK_Branch_ArabicName_Required",
                "NULLIF(LTRIM(RTRIM([ArabicName])), N'') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Branch_EnglishName_Required",
                "NULLIF(LTRIM(RTRIM([EnglishName])), N'') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Branch_IPAddress_NotBlank",
                "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");

            table.HasCheckConstraint(
                "CK_Branch_DeactivationAudit_Pair",
                "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR " +
                "([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");

            table.HasCheckConstraint(
                "CK_Branch_ReactivationAudit_Pair",
                "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR " +
                "([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IPAddress)
            .IsRequired()
            .HasMaxLength(45)
            .IsUnicode(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(x => x.License)
            .IsRequired(false)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.Property(x => x.DeactivatedOnUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(3)");

        builder.Property(x => x.DeactivatedByApplicationUserId)
            .IsRequired(false);

        builder.Property(x => x.ReactivatedOnUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(3)");

        builder.Property(x => x.ReactivatedByApplicationUserId)
            .IsRequired(false);

        builder.HasIndex(x => x.IPAddress)
            .IsUnique();

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasIndex(x => x.DeactivatedByApplicationUserId);

        builder.HasIndex(x => x.ReactivatedByApplicationUserId);

        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastModifiedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DeactivatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.DeactivatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReactivatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ReactivatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
