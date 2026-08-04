using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendBranchBrandingLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowOperatorSelection",
                table: "BranchBranding",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowRequestMoreServices",
                table: "BranchBranding",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AlwaysRequireUserInput",
                table: "BranchBranding",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DefaultLanguageIsArabic",
                table: "BranchBranding",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterButtonBackgroundColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FooterButtonHeight",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterButtonText",
                table: "BranchBranding",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterButtonTextColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FooterButtonWidth",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeypadButtonBackgroundColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KeypadButtonHeight",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeypadButtonText",
                table: "BranchBranding",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeypadButtonTextColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KeypadButtonWidth",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageButtonBackgroundColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LanguageButtonHeight",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageButtonText",
                table: "BranchBranding",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageButtonTextColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LanguageButtonWidth",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainTextColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceButtonBackgroundColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceButtonFontSize",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceButtonHeight",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceButtonSpace",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceButtonText",
                table: "BranchBranding",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceButtonTextColor",
                table: "BranchBranding",
                type: "varchar(7)",
                unicode: false,
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceButtonWidth",
                table: "BranchBranding",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowLanguagePage",
                table: "BranchBranding",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowServiceNavigationPath",
                table: "BranchBranding",
                type: "bit",
                nullable: true);

            // Existing records predate all six behavior options. A staged,
            // explicit neutral backfill preserves those records without
            // creating persistent database defaults for future writes.
            migrationBuilder.Sql(
                """
                UPDATE [BranchBranding]
                SET [AllowOperatorSelection] = 0,
                    [AllowRequestMoreServices] = 0,
                    [AlwaysRequireUserInput] = 0,
                    [DefaultLanguageIsArabic] = 0,
                    [ShowLanguagePage] = 0,
                    [ShowServiceNavigationPath] = 0
                WHERE [AllowOperatorSelection] IS NULL
                   OR [AllowRequestMoreServices] IS NULL
                   OR [AlwaysRequireUserInput] IS NULL
                   OR [DefaultLanguageIsArabic] IS NULL
                   OR [ShowLanguagePage] IS NULL
                   OR [ShowServiceNavigationPath] IS NULL;
                """);

            MakeBehaviorColumnRequired(
                migrationBuilder,
                "AllowOperatorSelection");
            MakeBehaviorColumnRequired(
                migrationBuilder,
                "AllowRequestMoreServices");
            MakeBehaviorColumnRequired(
                migrationBuilder,
                "AlwaysRequireUserInput");
            MakeBehaviorColumnRequired(
                migrationBuilder,
                "DefaultLanguageIsArabic");
            MakeBehaviorColumnRequired(
                migrationBuilder,
                "ShowLanguagePage");
            MakeBehaviorColumnRequired(
                migrationBuilder,
                "ShowServiceNavigationPath");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_FooterButtonBackgroundColor_Format",
                table: "BranchBranding",
                sql: "[FooterButtonBackgroundColor] IS NULL OR ([FooterButtonBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([FooterButtonBackgroundColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_FooterButtonHeight_Range",
                table: "BranchBranding",
                sql: "[FooterButtonHeight] IS NULL OR ([FooterButtonHeight] > 0 AND [FooterButtonHeight] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_FooterButtonTextColor_Format",
                table: "BranchBranding",
                sql: "[FooterButtonTextColor] IS NULL OR ([FooterButtonTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([FooterButtonTextColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_FooterButtonWidth_Range",
                table: "BranchBranding",
                sql: "[FooterButtonWidth] IS NULL OR ([FooterButtonWidth] > 0 AND [FooterButtonWidth] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_FooterColor_Format",
                table: "BranchBranding",
                sql: "[FooterColor] IS NULL OR ([FooterColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([FooterColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_HeaderColor_Format",
                table: "BranchBranding",
                sql: "[HeaderColor] IS NULL OR ([HeaderColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([HeaderColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_KeypadButtonBackgroundColor_Format",
                table: "BranchBranding",
                sql: "[KeypadButtonBackgroundColor] IS NULL OR ([KeypadButtonBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([KeypadButtonBackgroundColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_KeypadButtonHeight_Range",
                table: "BranchBranding",
                sql: "[KeypadButtonHeight] IS NULL OR ([KeypadButtonHeight] > 0 AND [KeypadButtonHeight] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_KeypadButtonTextColor_Format",
                table: "BranchBranding",
                sql: "[KeypadButtonTextColor] IS NULL OR ([KeypadButtonTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([KeypadButtonTextColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_KeypadButtonWidth_Range",
                table: "BranchBranding",
                sql: "[KeypadButtonWidth] IS NULL OR ([KeypadButtonWidth] > 0 AND [KeypadButtonWidth] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_LanguageButtonBackgroundColor_Format",
                table: "BranchBranding",
                sql: "[LanguageButtonBackgroundColor] IS NULL OR ([LanguageButtonBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([LanguageButtonBackgroundColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_LanguageButtonHeight_Range",
                table: "BranchBranding",
                sql: "[LanguageButtonHeight] IS NULL OR ([LanguageButtonHeight] > 0 AND [LanguageButtonHeight] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_LanguageButtonTextColor_Format",
                table: "BranchBranding",
                sql: "[LanguageButtonTextColor] IS NULL OR ([LanguageButtonTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([LanguageButtonTextColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_LanguageButtonWidth_Range",
                table: "BranchBranding",
                sql: "[LanguageButtonWidth] IS NULL OR ([LanguageButtonWidth] > 0 AND [LanguageButtonWidth] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_MainTextColor_Format",
                table: "BranchBranding",
                sql: "[MainTextColor] IS NULL OR ([MainTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([MainTextColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonBackgroundColor_Format",
                table: "BranchBranding",
                sql: "[ServiceButtonBackgroundColor] IS NULL OR ([ServiceButtonBackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([ServiceButtonBackgroundColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonFontSize_Range",
                table: "BranchBranding",
                sql: "[ServiceButtonFontSize] IS NULL OR ([ServiceButtonFontSize] > 0 AND [ServiceButtonFontSize] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonHeight_Range",
                table: "BranchBranding",
                sql: "[ServiceButtonHeight] IS NULL OR ([ServiceButtonHeight] > 0 AND [ServiceButtonHeight] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonSpace_Range",
                table: "BranchBranding",
                sql: "[ServiceButtonSpace] IS NULL OR ([ServiceButtonSpace] >= 0 AND [ServiceButtonSpace] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonTextColor_Format",
                table: "BranchBranding",
                sql: "[ServiceButtonTextColor] IS NULL OR ([ServiceButtonTextColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([ServiceButtonTextColor]) = 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonWidth_Range",
                table: "BranchBranding",
                sql: "[ServiceButtonWidth] IS NULL OR ([ServiceButtonWidth] > 0 AND [ServiceButtonWidth] <= 100)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_FooterButtonBackgroundColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_FooterButtonHeight_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_FooterButtonTextColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_FooterButtonWidth_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_FooterColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_HeaderColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_KeypadButtonBackgroundColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_KeypadButtonHeight_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_KeypadButtonTextColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_KeypadButtonWidth_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_LanguageButtonBackgroundColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_LanguageButtonHeight_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_LanguageButtonTextColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_LanguageButtonWidth_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_MainTextColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonBackgroundColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonFontSize_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonHeight_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonSpace_Range",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonTextColor_Format",
                table: "BranchBranding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchBranding_ServiceButtonWidth_Range",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "AllowOperatorSelection",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "AllowRequestMoreServices",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "AlwaysRequireUserInput",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "DefaultLanguageIsArabic",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "FooterButtonBackgroundColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "FooterButtonHeight",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "FooterButtonText",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "FooterButtonTextColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "FooterButtonWidth",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "FooterColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "HeaderColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "KeypadButtonBackgroundColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "KeypadButtonHeight",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "KeypadButtonText",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "KeypadButtonTextColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "KeypadButtonWidth",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "LanguageButtonBackgroundColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "LanguageButtonHeight",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "LanguageButtonText",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "LanguageButtonTextColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "LanguageButtonWidth",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "MainTextColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ServiceButtonBackgroundColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ServiceButtonFontSize",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ServiceButtonHeight",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ServiceButtonSpace",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ServiceButtonText",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ServiceButtonTextColor",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ServiceButtonWidth",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ShowLanguagePage",
                table: "BranchBranding");

            migrationBuilder.DropColumn(
                name: "ShowServiceNavigationPath",
                table: "BranchBranding");
        }

        private static void MakeBehaviorColumnRequired(
            MigrationBuilder migrationBuilder,
            string columnName)
        {
            migrationBuilder.AlterColumn<bool>(
                name: columnName,
                table: "BranchBranding",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }
    }
}
