using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qcontrol.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayManagementAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [Display]
                    WHERE [SerialNo] IS NULL
                       OR NULLIF(LTRIM(RTRIM([SerialNo])), N'') IS NULL
                )
                BEGIN
                    THROW 51031, 'Display migration failed: SerialNo contains null or blank values.', 1;
                END

                IF EXISTS (
                    SELECT 1
                    FROM [Display]
                    WHERE [Type] IS NULL
                       OR NULLIF(LTRIM(RTRIM([Type])), N'') IS NULL
                )
                BEGIN
                    THROW 51032, 'Display migration failed: Type contains null or blank values.', 1;
                END

                IF EXISTS (
                    SELECT 1
                    FROM [Display]
                    WHERE [IPAddress] IS NULL
                       OR NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NULL
                )
                BEGIN
                    THROW 51033, 'Display migration failed: IPAddress contains null or blank values.', 1;
                END

                IF EXISTS (
                    SELECT 1
                    FROM [Display]
                    GROUP BY [BranchId], [Number]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51034, 'Display migration failed: duplicate Number values exist inside the same Branch.', 1;
                END

                IF EXISTS (
                    SELECT 1
                    FROM [Display]
                    GROUP BY [BranchId], [IPAddress]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51035, 'Display migration failed: duplicate IPAddress values exist inside the same Branch.', 1;
                END

                IF EXISTS (
                    SELECT 1
                    FROM [Display]
                    GROUP BY [BranchId], [SerialNo]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51036, 'Display migration failed: duplicate SerialNo values exist inside the same Branch.', 1;
                END
                """);

            migrationBuilder.DropIndex(
                name: "IX_Display_IPAddress",
                table: "Display");

            migrationBuilder.DropIndex(
                name: "IX_Display_SerialNo",
                table: "Display");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Display_Number_Positive",
                table: "Display");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Display",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "Display",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Display",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Display",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Display",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredOnUtc",
                table: "Display",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Display_BranchId_IPAddress",
                table: "Display",
                columns: new[] { "BranchId", "IPAddress" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Display_BranchId_SerialNo",
                table: "Display",
                columns: new[] { "BranchId", "SerialNo" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Display_Number_NotBlank",
                table: "Display",
                sql: "NULLIF(LTRIM(RTRIM([Number])), '') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Display_SerialNo_NotBlank",
                table: "Display",
                sql: "NULLIF(LTRIM(RTRIM([SerialNo])), '') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Display_Type_NotBlank",
                table: "Display",
                sql: "NULLIF(LTRIM(RTRIM([Type])), '') IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Display_BranchId_IPAddress",
                table: "Display");

            migrationBuilder.DropIndex(
                name: "IX_Display_BranchId_SerialNo",
                table: "Display");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Display_Number_NotBlank",
                table: "Display");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Display_SerialNo_NotBlank",
                table: "Display");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Display_Type_NotBlank",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "RestoredOnUtc",
                table: "Display");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Display",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "Display",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Display",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Display_IPAddress",
                table: "Display",
                column: "IPAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Display_SerialNo",
                table: "Display",
                column: "SerialNo",
                unique: true,
                filter: "[SerialNo] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Display_Number_Positive",
                table: "Display",
                sql: "[Number] > 0");
        }
    }
}
