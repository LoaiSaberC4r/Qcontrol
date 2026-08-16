using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceCustomInputConfiguration
    : IEntityTypeConfiguration<ServiceCustomInput>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ServiceCustomInput> builder)
    {
        builder.ToTable("ServiceCustomInput", table =>
        {
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_Name_NotBlank",
                "LEN(LTRIM(RTRIM([Name]))) > 0");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_Order_Positive",
                "[Order] > 0");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_Type_Valid",
                "[Type] IN (1, 2)");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_String_Restrictions",
                "[Type] <> 1 OR ([MinValue] IS NULL AND [MaxValue] IS NULL)");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_Integer_Restrictions",
                "[Type] <> 2 OR ([MinLength] IS NULL AND [MaxLength] IS NULL AND [StartWith] IS NULL)");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_Length_Range",
                "[MinLength] IS NULL OR [MaxLength] IS NULL OR [MaxLength] >= [MinLength]");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_Value_Range",
                "[MinValue] IS NULL OR [MaxValue] IS NULL OR [MaxValue] >= [MinValue]");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_MinLength_NonNegative",
                "[MinLength] IS NULL OR [MinLength] >= 0");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_MaxLength_Valid",
                "[MaxLength] IS NULL OR ([MaxLength] > 0 AND [MaxLength] <= 3000)");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_StartWith_StringOnly",
                "[StartWith] IS NULL OR [Type] = 1");
            table.HasCheckConstraint(
                "CK_ServiceCustomInput_StartWith_NotBlank",
                "[StartWith] IS NULL OR LEN(LTRIM(RTRIM([StartWith]))) > 0");
        });

        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.Service.IsDeleted);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ServiceId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LabelEn).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.LabelAr).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsRequired).IsRequired();
        builder.Property(x => x.MinLength).IsRequired(false);
        builder.Property(x => x.MaxLength).IsRequired(false);
        builder.Property(x => x.MinValue).IsRequired(false);
        builder.Property(x => x.MaxValue).IsRequired(false);
        builder.Property(x => x.StartWith).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2").IsRequired(false);
        builder.Property(x => x.CreatedByApplicationUserId).IsRequired();
        builder.Property(x => x.LastModifiedByApplicationUserId).IsRequired(false);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.CustomInputs)
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

        builder.HasIndex(x => x.ServiceId)
            .HasDatabaseName("IX_ServiceCustomInput_ServiceId");
        builder.HasIndex(x => new { x.ServiceId, x.IsActive, x.Order })
            .HasDatabaseName("IX_ServiceCustomInput_ServiceId_IsActive_Order");
        builder.HasIndex(x => new { x.ServiceId, x.Name })
            .IsUnique()
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("UX_ServiceCustomInput_ServiceId_Name_Active");
        builder.HasIndex(x => x.CreatedByApplicationUserId);
        builder.HasIndex(x => x.LastModifiedByApplicationUserId);
    }
}
