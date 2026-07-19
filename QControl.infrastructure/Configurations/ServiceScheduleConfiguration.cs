using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceScheduleConfiguration
    : IEntityTypeConfiguration<ServiceSchedule>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ServiceSchedule> builder)
    {
        builder.ToTable("ServiceSchedule", table =>
        {
            table.HasCheckConstraint(
                "CK_ServiceSchedule_SlotCode_NotBlank",
                "[SlotCode] IS NULL OR LEN(LTRIM(RTRIM([SlotCode]))) > 0");

            table.HasCheckConstraint(
                "CK_ServiceSchedule_RequiredSlotCode",
                "[IsSlotCodeRequired] = 0 OR [SlotCode] IS NOT NULL");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ServiceId)
            .IsRequired();

        builder.Property(x => x.IsSlotCodeRequired)
            .IsRequired();

        builder.Property(x => x.SlotCode)
            .HasMaxLength(ServiceScheduleSlotCodeNormalizer.MaxLength)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.ModifiedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.HasMany(x => x.TimeSlots)
            .WithOne(x => x.ServiceSchedule)
            .HasForeignKey(x => x.ServiceScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.TimeSlots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Service)
            .WithMany()
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

        builder.HasIndex(x => x.BranchId)
            .HasDatabaseName("IX_ServiceSchedule_BranchId");

        builder.HasIndex(x => x.ServiceId)
            .HasDatabaseName("IX_ServiceSchedule_ServiceId");

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.ServiceId
        })
            .IsUnique()
            .HasDatabaseName("UX_ServiceSchedule_BranchId_ServiceId");

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.SlotCode
        })
            .IsUnique()
            .HasFilter("[SlotCode] IS NOT NULL")
            .HasDatabaseName("UX_ServiceSchedule_BranchId_SlotCode");

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);
    }
}
