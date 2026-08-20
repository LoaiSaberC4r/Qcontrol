using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchDisplayConfigurationAndMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchDisplayConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DisplayBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    MainTitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MainTitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeaderBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    MainTitleTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    MainTitleFontSize = table.Column<int>(type: "int", nullable: false),
                    TableHeaderBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    TableHeaderTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    TableRowBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    TableRowTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    TicketNumberBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    TicketNumberTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    TicketColumnTitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TicketColumnTitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ServiceColumnTitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ServiceColumnTitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WindowColumnTitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WindowColumnTitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TickerBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    TickerTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    TickerFontSize = table.Column<int>(type: "int", nullable: false),
                    ShowClock = table.Column<bool>(type: "bit", nullable: false),
                    ClockBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    ClockTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchDisplayConfiguration", x => x.Id);
                    table.CheckConstraint("CK_BranchDisplayConfiguration_ClockBackgroundColor_Format", "[ClockBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([ClockBackgroundColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_ClockTextColor_Format", "[ClockTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([ClockTextColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_DisplayBackgroundColor_Format", "[DisplayBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([DisplayBackgroundColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_HeaderBackgroundColor_Format", "[HeaderBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([HeaderBackgroundColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_MainTitleFontSize_Range", "[MainTitleFontSize] >= 1 AND [MainTitleFontSize] <= 100");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_MainTitleTextColor_Format", "[MainTitleTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([MainTitleTextColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TableHeaderBackgroundColor_Format", "[TableHeaderBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([TableHeaderBackgroundColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TableHeaderTextColor_Format", "[TableHeaderTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([TableHeaderTextColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TableRowBackgroundColor_Format", "[TableRowBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([TableRowBackgroundColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TableRowTextColor_Format", "[TableRowTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([TableRowTextColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TickerBackgroundColor_Format", "[TickerBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([TickerBackgroundColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TickerFontSize_Range", "[TickerFontSize] >= 1 AND [TickerFontSize] <= 100");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TickerTextColor_Format", "[TickerTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([TickerTextColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TicketNumberBackgroundColor_Format", "[TicketNumberBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([TicketNumberBackgroundColor]) = 7");
                    table.CheckConstraint("CK_BranchDisplayConfiguration_TicketNumberTextColor_Format", "[TicketNumberTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([TicketNumberTextColor]) = 7");
                    table.ForeignKey(
                        name: "FK_BranchDisplayConfiguration_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchDisplayConfiguration_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchDisplayConfiguration_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchDisplayMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    TextAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TextEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeactivatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeactivatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReactivatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ReactivatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchDisplayMessage", x => x.Id);
                    table.CheckConstraint("CK_BranchDisplayMessage_DeactivationAudit_Pair", "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");
                    table.CheckConstraint("CK_BranchDisplayMessage_DisplayOrder_Positive", "[DisplayOrder] > 0");
                    table.CheckConstraint("CK_BranchDisplayMessage_ReactivationAudit_Pair", "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");
                    table.CheckConstraint("CK_BranchDisplayMessage_TextAr_NotBlank", "NULLIF(LTRIM(RTRIM([TextAr])), N'') IS NOT NULL");
                    table.CheckConstraint("CK_BranchDisplayMessage_TextEn_NotBlank", "NULLIF(LTRIM(RTRIM([TextEn])), N'') IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_BranchDisplayMessage_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchDisplayMessage_ApplicationUser_DeactivatedByApplicationUserId",
                        column: x => x.DeactivatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchDisplayMessage_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchDisplayMessage_ApplicationUser_ReactivatedByApplicationUserId",
                        column: x => x.ReactivatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchDisplayMessage_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayConfiguration_BranchId",
                table: "BranchDisplayConfiguration",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayConfiguration_CreatedByApplicationUserId",
                table: "BranchDisplayConfiguration",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayConfiguration_LastModifiedByApplicationUserId",
                table: "BranchDisplayConfiguration",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayMessage_BranchId_DisplayOrder",
                table: "BranchDisplayMessage",
                columns: new[] { "BranchId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayMessage_BranchId_IsActive_DisplayOrder",
                table: "BranchDisplayMessage",
                columns: new[] { "BranchId", "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayMessage_CreatedByApplicationUserId",
                table: "BranchDisplayMessage",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayMessage_DeactivatedByApplicationUserId",
                table: "BranchDisplayMessage",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayMessage_LastModifiedByApplicationUserId",
                table: "BranchDisplayMessage",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDisplayMessage_ReactivatedByApplicationUserId",
                table: "BranchDisplayMessage",
                column: "ReactivatedByApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchDisplayConfiguration");

            migrationBuilder.DropTable(
                name: "BranchDisplayMessage");
        }
    }
}
