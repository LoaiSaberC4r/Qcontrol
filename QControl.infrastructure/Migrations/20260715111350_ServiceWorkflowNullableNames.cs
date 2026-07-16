using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceWorkflowNullableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceWorkflows_ArabicName_NotBlank",
                table: "ServiceWorkflows");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceWorkflows_EnglishName_NotBlank",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_ArabicName",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_EnglishName",
                table: "ServiceWorkflows");

            migrationBuilder.AlterColumn<string>(
                name: "ArabicName",
                table: "ServiceWorkflows",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "ServiceWorkflows",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_ArabicName",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId", "ArabicName" },
                unique: true,
                filter: "[ArabicName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_EnglishName",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId", "EnglishName" },
                unique: true,
                filter: "[EnglishName] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceWorkflows_ArabicName_NotBlank",
                table: "ServiceWorkflows",
                sql: "[ArabicName] IS NULL OR LEN(LTRIM(RTRIM([ArabicName]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceWorkflows_EnglishName_NotBlank",
                table: "ServiceWorkflows",
                sql: "[EnglishName] IS NULL OR LEN(LTRIM(RTRIM([EnglishName]))) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM [ServiceWorkflows]
                    WHERE [ArabicName] IS NULL
                       OR [EnglishName] IS NULL
                )
                BEGIN
                    THROW 51001,
                        'Cannot revert nullable workflow names while ServiceWorkflow rows contain null names.',
                        1;
                END
                """);

            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_ArabicName",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_EnglishName",
                table: "ServiceWorkflows");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceWorkflows_ArabicName_NotBlank",
                table: "ServiceWorkflows");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceWorkflows_EnglishName_NotBlank",
                table: "ServiceWorkflows");

            migrationBuilder.AlterColumn<string>(
                name: "ArabicName",
                table: "ServiceWorkflows",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "ServiceWorkflows",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_ArabicName",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId", "ArabicName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_EnglishName",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId", "EnglishName" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceWorkflows_ArabicName_NotBlank",
                table: "ServiceWorkflows",
                sql: "LEN(LTRIM(RTRIM([ArabicName]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceWorkflows_EnglishName_NotBlank",
                table: "ServiceWorkflows",
                sql: "LEN(LTRIM(RTRIM([EnglishName]))) > 0");
        }
    }
}
