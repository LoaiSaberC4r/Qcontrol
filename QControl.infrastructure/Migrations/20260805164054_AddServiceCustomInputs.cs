using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCustomInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceCustomInput",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LabelEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LabelAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    MinLength = table.Column<int>(type: "int", nullable: true),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    MinValue = table.Column<int>(type: "int", nullable: true),
                    MaxValue = table.Column<int>(type: "int", nullable: true),
                    StartWith = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCustomInput", x => x.Id);
                    table.CheckConstraint("CK_ServiceCustomInput_Integer_Restrictions", "[Type] <> 2 OR ([MinLength] IS NULL AND [MaxLength] IS NULL AND [StartWith] IS NULL)");
                    table.CheckConstraint("CK_ServiceCustomInput_Length_Range", "[MinLength] IS NULL OR [MaxLength] IS NULL OR [MaxLength] >= [MinLength]");
                    table.CheckConstraint("CK_ServiceCustomInput_MaxLength_Valid", "[MaxLength] IS NULL OR ([MaxLength] > 0 AND [MaxLength] <= 3000)");
                    table.CheckConstraint("CK_ServiceCustomInput_MinLength_NonNegative", "[MinLength] IS NULL OR [MinLength] >= 0");
                    table.CheckConstraint("CK_ServiceCustomInput_Name_NotBlank", "LEN(LTRIM(RTRIM([Name]))) > 0");
                    table.CheckConstraint("CK_ServiceCustomInput_Order_Positive", "[Order] > 0");
                    table.CheckConstraint("CK_ServiceCustomInput_StartWith_NotBlank", "[StartWith] IS NULL OR LEN(LTRIM(RTRIM([StartWith]))) > 0");
                    table.CheckConstraint("CK_ServiceCustomInput_StartWith_StringOnly", "[StartWith] IS NULL OR [Type] = 1");
                    table.CheckConstraint("CK_ServiceCustomInput_String_Restrictions", "[Type] <> 1 OR ([MinValue] IS NULL AND [MaxValue] IS NULL)");
                    table.CheckConstraint("CK_ServiceCustomInput_Type_Valid", "[Type] IN (1, 2)");
                    table.CheckConstraint("CK_ServiceCustomInput_Value_Range", "[MinValue] IS NULL OR [MaxValue] IS NULL OR [MaxValue] >= [MinValue]");
                    table.ForeignKey(
                        name: "FK_ServiceCustomInput_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceCustomInput_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceCustomInput_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCustomInput_CreatedByApplicationUserId",
                table: "ServiceCustomInput",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCustomInput_LastModifiedByApplicationUserId",
                table: "ServiceCustomInput",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCustomInput_ServiceId",
                table: "ServiceCustomInput",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCustomInput_ServiceId_IsActive_Order",
                table: "ServiceCustomInput",
                columns: new[] { "ServiceId", "IsActive", "Order" });

            migrationBuilder.CreateIndex(
                name: "UX_ServiceCustomInput_ServiceId_Name_Active",
                table: "ServiceCustomInput",
                columns: new[] { "ServiceId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceCustomInput");
        }
    }
}
