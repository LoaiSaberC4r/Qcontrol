using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentServiceId = table.Column<int>(type: "int", nullable: true),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ArabicUserMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnglishUserMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsTicketIssuable = table.Column<bool>(type: "bit", nullable: false),
                    IsClientInputRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasReservation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Weight = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RangePrefix = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    RangeStartNumber = table.Column<int>(type: "int", nullable: false),
                    RangeEndNumber = table.Column<int>(type: "int", nullable: false),
                    WaitingDuration = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NoOfTicketCopies = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                    table.CheckConstraint("CK_Service_ArabicName_NotBlank", "LEN(LTRIM(RTRIM([ArabicName]))) > 0");
                    table.CheckConstraint("CK_Service_EnglishName_NotBlank", "LEN(LTRIM(RTRIM([EnglishName]))) > 0");
                    table.CheckConstraint("CK_Service_NoOfTicketCopies_Positive", "[NoOfTicketCopies] > 0");
                    table.CheckConstraint("CK_Service_OrderNo_NonNegative", "[OrderNo] >= 0");
                    table.CheckConstraint("CK_Service_Priority_NonNegative", "[Priority] >= 0");
                    table.CheckConstraint("CK_Service_RangeEnd_GreaterOrEqualStart", "[RangeEndNumber] >= [RangeStartNumber]");
                    table.CheckConstraint("CK_Service_RangePrefix_NotBlank", "LEN(LTRIM(RTRIM([RangePrefix]))) > 0");
                    table.CheckConstraint("CK_Service_RangeStart_NonNegative", "[RangeStartNumber] >= 0");
                    table.CheckConstraint("CK_Service_WaitingDuration_NonNegative", "[WaitingDuration] >= 0");
                    table.CheckConstraint("CK_Service_Weight_NonNegative", "[Weight] >= 0");
                    table.ForeignKey(
                        name: "FK_Service_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Service_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Service_Service_ParentServiceId",
                        column: x => x.ParentServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Service_CreatedByApplicationUserId",
                table: "Service",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_IsActive",
                table: "Service",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Service_IsDeleted",
                table: "Service",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Service_LastModifiedByApplicationUserId",
                table: "Service",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_ParentServiceId",
                table: "Service",
                column: "ParentServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_ParentServiceId_OrderNo",
                table: "Service",
                columns: new[] { "ParentServiceId", "OrderNo" });

            migrationBuilder.CreateIndex(
                name: "UX_Service_ArabicName_ParentServiceId",
                table: "Service",
                columns: new[] { "ArabicName", "ParentServiceId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Service_EnglishName_ParentServiceId",
                table: "Service",
                columns: new[] { "EnglishName", "ParentServiceId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Service");
        }
    }
}
