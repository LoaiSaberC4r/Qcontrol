using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceWorkflowManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceWorkflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeactivatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeactivatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReactivatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReactivatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceWorkflows", x => x.Id);
                    table.CheckConstraint("CK_ServiceWorkflows_ArabicName_NotBlank", "LEN(LTRIM(RTRIM([ArabicName]))) > 0");
                    table.CheckConstraint("CK_ServiceWorkflows_EnglishName_NotBlank", "LEN(LTRIM(RTRIM([EnglishName]))) > 0");
                    table.ForeignKey(
                        name: "FK_ServiceWorkflows_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceWorkflows_ApplicationUser_DeactivatedByApplicationUserId",
                        column: x => x.DeactivatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceWorkflows_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceWorkflows_ApplicationUser_ReactivatedByApplicationUserId",
                        column: x => x.ReactivatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceWorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceWorkflowId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceWorkflowSteps", x => x.Id);
                    table.CheckConstraint("CK_ServiceWorkflowSteps_StepOrder_Positive", "[StepOrder] > 0");
                    table.ForeignKey(
                        name: "FK_ServiceWorkflowSteps_ServiceWorkflows_ServiceWorkflowId",
                        column: x => x.ServiceWorkflowId,
                        principalTable: "ServiceWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceWorkflowSteps_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_CreatedByApplicationUserId",
                table: "ServiceWorkflows",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_DeactivatedByApplicationUserId",
                table: "ServiceWorkflows",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_IsActive",
                table: "ServiceWorkflows",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_LastModifiedByApplicationUserId",
                table: "ServiceWorkflows",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_ReactivatedByApplicationUserId",
                table: "ServiceWorkflows",
                column: "ReactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_ArabicName",
                table: "ServiceWorkflows",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_EnglishName",
                table: "ServiceWorkflows",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflowSteps_ServiceId",
                table: "ServiceWorkflowSteps",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflowSteps_ServiceWorkflowId",
                table: "ServiceWorkflowSteps",
                column: "ServiceWorkflowId");

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflowSteps_ServiceWorkflowId_StepOrder",
                table: "ServiceWorkflowSteps",
                columns: new[] { "ServiceWorkflowId", "StepOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceWorkflowSteps");

            migrationBuilder.DropTable(
                name: "ServiceWorkflows");
        }
    }
}
