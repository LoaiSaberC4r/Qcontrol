using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    IsSlotCodeRequired = table.Column<bool>(type: "bit", nullable: false),
                    SlotCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSchedule", x => x.Id);
                    table.CheckConstraint("CK_ServiceSchedule_RequiredSlotCode", "[IsSlotCodeRequired] = 0 OR [SlotCode] IS NOT NULL");
                    table.CheckConstraint("CK_ServiceSchedule_SlotCode_NotBlank", "[SlotCode] IS NULL OR LEN(LTRIM(RTRIM([SlotCode]))) > 0");
                    table.CheckConstraint("CK_ServiceSchedule_TimeRange", "[StartTime] < [EndTime]");
                    table.ForeignKey(
                        name: "FK_ServiceSchedule_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceSchedule_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceSchedule_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceSchedule_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceScheduleWorkDay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceScheduleId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceScheduleWorkDay", x => x.Id);
                    table.CheckConstraint("CK_ServiceScheduleWorkDay_DayOfWeek", "[DayOfWeek] BETWEEN 0 AND 6");
                    table.ForeignKey(
                        name: "FK_ServiceScheduleWorkDay_ServiceSchedule_ServiceScheduleId",
                        column: x => x.ServiceScheduleId,
                        principalTable: "ServiceSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedule_BranchId",
                table: "ServiceSchedule",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedule_CreatedByApplicationUserId",
                table: "ServiceSchedule",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedule_LastModifiedByApplicationUserId",
                table: "ServiceSchedule",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedule_ServiceId",
                table: "ServiceSchedule",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "UX_ServiceSchedule_BranchId_ServiceId",
                table: "ServiceSchedule",
                columns: new[] { "BranchId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ServiceSchedule_BranchId_SlotCode",
                table: "ServiceSchedule",
                columns: new[] { "BranchId", "SlotCode" },
                unique: true,
                filter: "[SlotCode] IS NOT NULL");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceScheduleWorkDay");

            migrationBuilder.DropTable(
                name: "ServiceSchedule");
        }
    }
}
