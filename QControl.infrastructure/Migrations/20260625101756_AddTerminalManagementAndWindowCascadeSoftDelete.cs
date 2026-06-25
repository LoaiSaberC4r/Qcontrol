using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qcontrol.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminalManagementAndWindowCascadeSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [Terminal]
                    WHERE [SerialNo] IS NULL
                       OR NULLIF(LTRIM(RTRIM([SerialNo])), N'') IS NULL)
                BEGIN
                    THROW 51000, 'Cannot apply Terminal management migration: Terminal.SerialNo contains null or blank values. Clean the data before migrating.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM [Terminal]
                    WHERE [Type] IS NULL
                       OR NULLIF(LTRIM(RTRIM([Type])), N'') IS NULL)
                BEGIN
                    THROW 51001, 'Cannot apply Terminal management migration: Terminal.Type contains null or blank values. Clean the data before migrating.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM (
                        SELECT
                            wa.[BranchId],
                            t.[IPAddress]
                        FROM [Terminal] AS t
                        INNER JOIN [Window] AS w
                            ON w.[Id] = t.[WindowId]
                        INNER JOIN [WaitingArea] AS wa
                            ON wa.[Id] = w.[WaitingAreaId]
                        GROUP BY
                            wa.[BranchId],
                            t.[IPAddress]
                        HAVING COUNT_BIG(*) > 1
                    ) AS duplicates)
                BEGIN
                    THROW 51002, 'Cannot apply Terminal management migration: duplicate Terminal.IPAddress values exist inside the same Branch. Clean the data before migrating.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM (
                        SELECT
                            wa.[BranchId],
                            t.[SerialNo]
                        FROM [Terminal] AS t
                        INNER JOIN [Window] AS w
                            ON w.[Id] = t.[WindowId]
                        INNER JOIN [WaitingArea] AS wa
                            ON wa.[Id] = w.[WaitingAreaId]
                        GROUP BY
                            wa.[BranchId],
                            t.[SerialNo]
                        HAVING COUNT_BIG(*) > 1
                    ) AS duplicates)
                BEGIN
                    THROW 51003, 'Cannot apply Terminal management migration: duplicate Terminal.SerialNo values exist inside the same Branch. Clean the data before migrating.', 1;
                END;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Terminal_SerialNo",
                table: "Terminal");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Terminal_Number_Positive",
                table: "Terminal");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Terminal",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "Terminal",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Terminal",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Terminal",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Terminal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredOnUtc",
                table: "Terminal",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_SerialNo",
                table: "Terminal",
                column: "SerialNo");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Terminal_Number_NotBlank",
                table: "Terminal",
                sql: "NULLIF(LTRIM(RTRIM([Number])), '') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Terminal_SerialNo_NotBlank",
                table: "Terminal",
                sql: "NULLIF(LTRIM(RTRIM([SerialNo])), '') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Terminal_Type_NotBlank",
                table: "Terminal",
                sql: "NULLIF(LTRIM(RTRIM([Type])), '') IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Terminal_SerialNo",
                table: "Terminal");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Terminal_Number_NotBlank",
                table: "Terminal");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Terminal_SerialNo_NotBlank",
                table: "Terminal");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Terminal_Type_NotBlank",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "RestoredOnUtc",
                table: "Terminal");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Terminal",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "Terminal",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Terminal",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_SerialNo",
                table: "Terminal",
                column: "SerialNo",
                unique: true,
                filter: "[SerialNo] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Terminal_Number_Positive",
                table: "Terminal",
                sql: "[Number] > 0");
        }
    }
}
