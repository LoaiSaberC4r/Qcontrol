using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceScheduleTimeSlotConfiguration
    : IEntityTypeConfiguration<ServiceScheduleTimeSlot>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ServiceScheduleTimeSlot> builder)
    {
        builder.ToTable("ServiceScheduleTimeSlot", table =>
        {
            table.HasCheckConstraint(
                "CK_ServiceScheduleTimeSlot_DayOfWeek",
                "[DayOfWeek] BETWEEN 0 AND 6");

            table.HasCheckConstraint(
                "CK_ServiceScheduleTimeSlot_TimeRange",
                "[StartTime] < [EndTime]");
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

        builder.Property(x => x.StartTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.HasOne(x => x.ServiceSchedule)
            .WithMany(x => x.TimeSlots)
            .HasForeignKey(x => x.ServiceScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.ServiceScheduleId,
            x.DayOfWeek,
            x.StartTime
        })
            .HasDatabaseName(
                "IX_ServiceScheduleTimeSlot_Schedule_Day_StartTime");

        builder.HasIndex(x => new
        {
            x.ServiceScheduleId,
            x.DayOfWeek,
            x.StartTime,
            x.EndTime
        })
            .IsUnique()
            .HasDatabaseName("UX_ServiceScheduleTimeSlot_ExactRange");
    }
}
