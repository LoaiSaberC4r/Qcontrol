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
        var workDay = model.FindEntityType(typeof(ServiceScheduleWorkDay))!;

        Assert.Equal("ServiceSchedule", schedule.GetTableName());
        Assert.Equal("ServiceScheduleWorkDay", workDay.GetTableName());

        Assert.Equal(
            "time(0)",
            schedule.FindProperty(nameof(ServiceSchedule.StartTime))!
                .GetColumnType());
        Assert.Equal(
            "time(0)",
            schedule.FindProperty(nameof(ServiceSchedule.EndTime))!
                .GetColumnType());
        Assert.False(
            schedule.FindProperty(nameof(ServiceSchedule.SlotCode))!
                .IsUnicode());
        Assert.Equal(
            100,
            schedule.FindProperty(nameof(ServiceSchedule.SlotCode))!
                .GetMaxLength());

        var branchServiceIndex = schedule.GetIndexes()
            .Single(index =>
                index.GetDatabaseName() ==
                "UX_ServiceSchedule_BranchId_ServiceId");
        Assert.True(branchServiceIndex.IsUnique);

        var slotCodeIndex = schedule.GetIndexes()
            .Single(index =>
                index.GetDatabaseName() ==
                "UX_ServiceSchedule_BranchId_SlotCode");
        Assert.True(slotCodeIndex.IsUnique);
        Assert.Equal("[SlotCode] IS NOT NULL", slotCodeIndex.GetFilter());

        Assert.Contains(
            schedule.GetCheckConstraints(),
            constraint =>
                constraint.Name == "CK_ServiceSchedule_TimeRange");
        Assert.Contains(
            schedule.GetCheckConstraints(),
            constraint =>
                constraint.Name == "CK_ServiceSchedule_RequiredSlotCode");
        Assert.Contains(
            schedule.GetCheckConstraints(),
            constraint =>
                constraint.Name == "CK_ServiceSchedule_SlotCode_NotBlank");

        Assert.Equal(
            "tinyint",
            workDay.FindProperty(nameof(ServiceScheduleWorkDay.DayOfWeek))!
                .GetColumnType());

        var workDayIndex = workDay.GetIndexes()
            .Single(index =>
                index.GetDatabaseName() ==
                "UX_ServiceScheduleWorkDay_ServiceScheduleId_DayOfWeek");
        Assert.True(workDayIndex.IsUnique);

        Assert.Contains(
            workDay.GetCheckConstraints(),
            constraint =>
                constraint.Name == "CK_ServiceScheduleWorkDay_DayOfWeek");
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
