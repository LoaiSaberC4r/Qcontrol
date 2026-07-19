using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeServiceScheduleTimeSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceScheduleWorkDay_ServiceSchedule_ServiceScheduleId",
                table: "ServiceScheduleWorkDay");

            migrationBuilder.DropIndex(
                name: "IX_ServiceScheduleWorkDay_ServiceScheduleId",
                table: "ServiceScheduleWorkDay");

            migrationBuilder.DropIndex(
                name: "UX_ServiceScheduleWorkDay_ServiceScheduleId_DayOfWeek",
                table: "ServiceScheduleWorkDay");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceScheduleWorkDay_DayOfWeek",
                table: "ServiceScheduleWorkDay");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceScheduleWorkDay",
                table: "ServiceScheduleWorkDay");

            migrationBuilder.RenameTable(
                name: "ServiceScheduleWorkDay",
                newName: "ServiceScheduleTimeSlot");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "ServiceScheduleTimeSlot",
                type: "time(0)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "ServiceScheduleTimeSlot",
                type: "time(0)",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE timeSlot
                SET
                    timeSlot.[StartTime] = schedule.[StartTime],
                    timeSlot.[EndTime] = schedule.[EndTime]
                FROM [ServiceScheduleTimeSlot] AS timeSlot
                INNER JOIN [ServiceSchedule] AS schedule
                    ON schedule.[Id] = timeSlot.[ServiceScheduleId];

                IF EXISTS
                (
                    SELECT 1
                    FROM [ServiceScheduleTimeSlot]
                    WHERE [StartTime] IS NULL OR [EndTime] IS NULL
                )
                BEGIN
                    THROW 51000, N'Unable to migrate all service schedule time slots.', 1;
                END;
                """);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "ServiceScheduleTimeSlot",
                type: "time(0)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "ServiceScheduleTimeSlot",
                type: "time(0)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(0)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceScheduleTimeSlot",
                table: "ServiceScheduleTimeSlot",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceScheduleTimeSlot_DayOfWeek",
                table: "ServiceScheduleTimeSlot",
                sql: "[DayOfWeek] BETWEEN 0 AND 6");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceScheduleTimeSlot_TimeRange",
                table: "ServiceScheduleTimeSlot",
                sql: "[StartTime] < [EndTime]");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceScheduleTimeSlot_ServiceSchedule_ServiceScheduleId",
                table: "ServiceScheduleTimeSlot",
                column: "ServiceScheduleId",
                principalTable: "ServiceSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceScheduleTimeSlot_Schedule_Day_StartTime",
                table: "ServiceScheduleTimeSlot",
                columns: new[]
                {
                    "ServiceScheduleId",
                    "DayOfWeek",
                    "StartTime"
                });

            migrationBuilder.CreateIndex(
                name: "UX_ServiceScheduleTimeSlot_ExactRange",
                table: "ServiceScheduleTimeSlot",
                columns: new[]
                {
                    "ServiceScheduleId",
                    "DayOfWeek",
                    "StartTime",
                    "EndTime"
                },
                unique: true);

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceSchedule_TimeRange",
                table: "ServiceSchedule");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ServiceSchedule");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ServiceSchedule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "ServiceSchedule",
                type: "time(0)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "ServiceSchedule",
                type: "time(0)",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE schedule
                SET
                    schedule.[StartTime] = slotRange.[StartTime],
                    schedule.[EndTime] = slotRange.[EndTime]
                FROM [ServiceSchedule] AS schedule
                INNER JOIN
                (
                    SELECT
                        [ServiceScheduleId],
                        MIN([StartTime]) AS [StartTime],
                        MAX([EndTime]) AS [EndTime]
                    FROM [ServiceScheduleTimeSlot]
                    GROUP BY [ServiceScheduleId]
                ) AS slotRange
                    ON slotRange.[ServiceScheduleId] = schedule.[Id];

                IF EXISTS
                (
                    SELECT 1
                    FROM [ServiceSchedule]
                    WHERE [StartTime] IS NULL OR [EndTime] IS NULL
                )
                BEGIN
                    THROW 51001, N'Unable to restore the legacy service schedule time range.', 1;
                END;
                """);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "ServiceSchedule",
                type: "time(0)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "ServiceSchedule",
                type: "time(0)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(0)",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_ServiceScheduleTimeSlot_Schedule_Day_StartTime",
                table: "ServiceScheduleTimeSlot");

            migrationBuilder.DropIndex(
                name: "UX_ServiceScheduleTimeSlot_ExactRange",
                table: "ServiceScheduleTimeSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceScheduleTimeSlot_ServiceSchedule_ServiceScheduleId",
                table: "ServiceScheduleTimeSlot");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceScheduleTimeSlot_DayOfWeek",
                table: "ServiceScheduleTimeSlot");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceScheduleTimeSlot_TimeRange",
                table: "ServiceScheduleTimeSlot");

            migrationBuilder.Sql(
                """
                ;WITH RankedTimeSlots AS
                (
                    SELECT
                        [Id],
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY [ServiceScheduleId], [DayOfWeek]
                            ORDER BY [StartTime], [EndTime], [Id]
                        ) AS [RowNumber]
                    FROM [ServiceScheduleTimeSlot]
                )
                DELETE FROM RankedTimeSlots
                WHERE [RowNumber] > 1;
                """);

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ServiceScheduleTimeSlot");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ServiceScheduleTimeSlot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceScheduleTimeSlot",
                table: "ServiceScheduleTimeSlot");

            migrationBuilder.RenameTable(
                name: "ServiceScheduleTimeSlot",
                newName: "ServiceScheduleWorkDay");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceScheduleWorkDay",
                table: "ServiceScheduleWorkDay",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceScheduleWorkDay_DayOfWeek",
                table: "ServiceScheduleWorkDay",
                sql: "[DayOfWeek] BETWEEN 0 AND 6");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceScheduleWorkDay_ServiceSchedule_ServiceScheduleId",
                table: "ServiceScheduleWorkDay",
                column: "ServiceScheduleId",
                principalTable: "ServiceSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceSchedule_TimeRange",
                table: "ServiceSchedule",
                sql: "[StartTime] < [EndTime]");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceScheduleWorkDay_ServiceScheduleId",
                table: "ServiceScheduleWorkDay",
                column: "ServiceScheduleId");

            migrationBuilder.CreateIndex(
                name: "UX_ServiceScheduleWorkDay_ServiceScheduleId_DayOfWeek",
                table: "ServiceScheduleWorkDay",
                columns: new[] { "ServiceScheduleId", "DayOfWeek" },
                unique: true);
        }
    }
}
