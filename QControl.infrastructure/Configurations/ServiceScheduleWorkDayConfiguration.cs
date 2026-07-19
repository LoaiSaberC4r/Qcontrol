using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceScheduleWorkDayConfiguration
    : IEntityTypeConfiguration<ServiceScheduleWorkDay>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ServiceScheduleWorkDay> builder)
    {
        builder.ToTable("ServiceScheduleWorkDay", table =>
        {
            table.HasCheckConstraint(
                "CK_ServiceScheduleWorkDay_DayOfWeek",
                "[DayOfWeek] BETWEEN 0 AND 6");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ServiceScheduleId)
            .IsRequired();

        builder.Property(x => x.DayOfWeek)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.HasOne(x => x.ServiceSchedule)
            .WithMany(x => x.WorkDays)
            .HasForeignKey(x => x.ServiceScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ServiceScheduleId)
            .HasDatabaseName("IX_ServiceScheduleWorkDay_ServiceScheduleId");

        builder.HasIndex(x => new
        {
            x.ServiceScheduleId,
            x.DayOfWeek
        })
            .IsUnique()
            .HasDatabaseName(
                "UX_ServiceScheduleWorkDay_ServiceScheduleId_DayOfWeek");
    }
}
