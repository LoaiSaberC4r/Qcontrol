using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.ServiceSchedules;

public sealed class ServiceSchedulePersistenceModelTests
{
    [Fact]
    public void Model_configures_schedule_tables_indexes_and_constraints()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;

        var schedule = model.FindEntityType(typeof(ServiceSchedule))!;
        var timeSlot = model.FindEntityType(typeof(ServiceScheduleTimeSlot))!;

        Assert.Equal("ServiceSchedule", schedule.GetTableName());
        Assert.Equal("ServiceScheduleTimeSlot", timeSlot.GetTableName());
        Assert.Null(schedule.FindProperty("StartTime"));
        Assert.Null(schedule.FindProperty("EndTime"));
        Assert.False(
            schedule.FindProperty(nameof(ServiceSchedule.SlotCode))!
                .IsUnicode());
        Assert.Equal(
            100,
            schedule.FindProperty(nameof(ServiceSchedule.SlotCode))!
                .GetMaxLength());

        Assert.True(schedule.GetIndexes().Single(index =>
            index.GetDatabaseName() ==
                "UX_ServiceSchedule_BranchId_ServiceId").IsUnique);

        var slotCodeIndex = schedule.GetIndexes().Single(index =>
            index.GetDatabaseName() ==
                "UX_ServiceSchedule_BranchId_SlotCode");
        Assert.True(slotCodeIndex.IsUnique);
        Assert.Equal("[SlotCode] IS NOT NULL", slotCodeIndex.GetFilter());
        Assert.DoesNotContain(
            schedule.GetCheckConstraints(),
            constraint => constraint.Name == "CK_ServiceSchedule_TimeRange");
        Assert.Contains(
            schedule.GetCheckConstraints(),
            constraint => constraint.Name ==
                "CK_ServiceSchedule_RequiredSlotCode");

        Assert.Equal(
            "tinyint",
            timeSlot.FindProperty(nameof(ServiceScheduleTimeSlot.DayOfWeek))!
                .GetColumnType());
        Assert.Equal(
            "time(0)",
            timeSlot.FindProperty(nameof(ServiceScheduleTimeSlot.StartTime))!
                .GetColumnType());
        Assert.Equal(
            "time(0)",
            timeSlot.FindProperty(nameof(ServiceScheduleTimeSlot.EndTime))!
                .GetColumnType());

        Assert.True(timeSlot.GetIndexes().Single(index =>
            index.GetDatabaseName() ==
                "UX_ServiceScheduleTimeSlot_ExactRange").IsUnique);
        Assert.Contains(timeSlot.GetIndexes(), index =>
            index.GetDatabaseName() ==
                "IX_ServiceScheduleTimeSlot_Schedule_Day_StartTime");
        Assert.Contains(timeSlot.GetCheckConstraints(), constraint =>
            constraint.Name == "CK_ServiceScheduleTimeSlot_DayOfWeek");
        Assert.Contains(timeSlot.GetCheckConstraints(), constraint =>
            constraint.Name == "CK_ServiceScheduleTimeSlot_TimeRange");
    }

    private static PlatformWriteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=QControlModelTests;Trusted_Connection=True;")
            .Options;

        return new PlatformWriteDbContext(
            options,
            new TestTenantContext(),
            new TestCurrentBranchContext());
    }
}
