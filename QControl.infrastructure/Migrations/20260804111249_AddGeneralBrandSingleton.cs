using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralBrandSingleton : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralBrand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SingletonKey = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    MainColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    SecondaryColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    BackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    HeaderColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    FooterColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    MainTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    ShowLanguagePage = table.Column<bool>(type: "bit", nullable: false),
                    DefaultLanguageIsArabic = table.Column<bool>(type: "bit", nullable: false),
                    AlwaysRequireUserInput = table.Column<bool>(type: "bit", nullable: false),
                    ShowServiceNavigationPath = table.Column<bool>(type: "bit", nullable: false),
                    AllowOperatorSelection = table.Column<bool>(type: "bit", nullable: false),
                    AllowRequestMoreServices = table.Column<bool>(type: "bit", nullable: false),
                    LanguageButtonBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    LanguageButtonTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    LanguageButtonWidth = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LanguageButtonHeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LanguageButtonText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ServiceButtonBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    ServiceButtonTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    ServiceButtonWidth = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ServiceButtonHeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ServiceButtonSpace = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ServiceButtonFontSize = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ServiceButtonText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    KeypadButtonBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    KeypadButtonTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    KeypadButtonWidth = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    KeypadButtonHeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    KeypadButtonText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FooterButtonBackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    FooterButtonTextColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    FooterButtonWidth = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FooterButtonHeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FooterButtonText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralBrand", x => x.Id);
                    table.CheckConstraint("CK_GeneralBrand_BackgroundColor_Format", "[BackgroundColor] IS NULL OR ([BackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([BackgroundColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_FooterButtonBackgroundColor_Format", "[FooterButtonBackgroundColor] IS NULL OR ([FooterButtonBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([FooterButtonBackgroundColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_FooterButtonHeight_Range", "[FooterButtonHeight] IS NULL OR ([FooterButtonHeight] > 0 AND [FooterButtonHeight] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_FooterButtonTextColor_Format", "[FooterButtonTextColor] IS NULL OR ([FooterButtonTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([FooterButtonTextColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_FooterButtonWidth_Range", "[FooterButtonWidth] IS NULL OR ([FooterButtonWidth] > 0 AND [FooterButtonWidth] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_FooterColor_Format", "[FooterColor] IS NULL OR ([FooterColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([FooterColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_HeaderColor_Format", "[HeaderColor] IS NULL OR ([HeaderColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([HeaderColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_KeypadButtonBackgroundColor_Format", "[KeypadButtonBackgroundColor] IS NULL OR ([KeypadButtonBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([KeypadButtonBackgroundColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_KeypadButtonHeight_Range", "[KeypadButtonHeight] IS NULL OR ([KeypadButtonHeight] > 0 AND [KeypadButtonHeight] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_KeypadButtonTextColor_Format", "[KeypadButtonTextColor] IS NULL OR ([KeypadButtonTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([KeypadButtonTextColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_KeypadButtonWidth_Range", "[KeypadButtonWidth] IS NULL OR ([KeypadButtonWidth] > 0 AND [KeypadButtonWidth] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_LanguageButtonBackgroundColor_Format", "[LanguageButtonBackgroundColor] IS NULL OR ([LanguageButtonBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([LanguageButtonBackgroundColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_LanguageButtonHeight_Range", "[LanguageButtonHeight] IS NULL OR ([LanguageButtonHeight] > 0 AND [LanguageButtonHeight] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_LanguageButtonTextColor_Format", "[LanguageButtonTextColor] IS NULL OR ([LanguageButtonTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([LanguageButtonTextColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_LanguageButtonWidth_Range", "[LanguageButtonWidth] IS NULL OR ([LanguageButtonWidth] > 0 AND [LanguageButtonWidth] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_MainColor_Format", "[MainColor] IS NULL OR ([MainColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([MainColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_MainTextColor_Format", "[MainTextColor] IS NULL OR ([MainTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([MainTextColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_SecondaryColor_Format", "[SecondaryColor] IS NULL OR ([SecondaryColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([SecondaryColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_ServiceButtonBackgroundColor_Format", "[ServiceButtonBackgroundColor] IS NULL OR ([ServiceButtonBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([ServiceButtonBackgroundColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_ServiceButtonFontSize_Range", "[ServiceButtonFontSize] IS NULL OR ([ServiceButtonFontSize] > 0 AND [ServiceButtonFontSize] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_ServiceButtonHeight_Range", "[ServiceButtonHeight] IS NULL OR ([ServiceButtonHeight] > 0 AND [ServiceButtonHeight] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_ServiceButtonSpace_Range", "[ServiceButtonSpace] IS NULL OR ([ServiceButtonSpace] >= 0 AND [ServiceButtonSpace] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_ServiceButtonTextColor_Format", "[ServiceButtonTextColor] IS NULL OR ([ServiceButtonTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([ServiceButtonTextColor]) = 7)");
                    table.CheckConstraint("CK_GeneralBrand_ServiceButtonWidth_Range", "[ServiceButtonWidth] IS NULL OR ([ServiceButtonWidth] > 0 AND [ServiceButtonWidth] <= 100)");
                    table.CheckConstraint("CK_GeneralBrand_SingletonKey", "[SingletonKey] = 1");
                    table.ForeignKey(
                        name: "FK_GeneralBrand_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneralBrand_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralBrand_CreatedByApplicationUserId",
                table: "GeneralBrand",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralBrand_LastModifiedByApplicationUserId",
                table: "GeneralBrand",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "UX_GeneralBrand_SingletonKey",
                table: "GeneralBrand",
                column: "SingletonKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralBrand");
        }
    }
}
