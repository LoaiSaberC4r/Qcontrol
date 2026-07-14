using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BranchServiceWorkflowOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_ArabicName",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_EnglishName",
                table: "ServiceWorkflows");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "ServiceWorkflows",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "ServiceWorkflows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LeafServiceId",
                table: "ServiceWorkflows",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [ServiceWorkflows])
                BEGIN
                    THROW 51000,
                        'Existing global ServiceWorkflow rows require explicit BranchId and LeafServiceId migration.',
                        1;
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "ServiceWorkflows",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LeafServiceId",
                table: "ServiceWorkflows",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_ArabicName",
                table: "ServiceWorkflows",
                column: "ArabicName");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_BranchId",
                table: "ServiceWorkflows",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_BranchId_LeafServiceId",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_BranchId_LeafServiceId_IsActive",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_EnglishName",
                table: "ServiceWorkflows",
                column: "EnglishName");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWorkflows_LeafServiceId",
                table: "ServiceWorkflows",
                column: "LeafServiceId");

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_ArabicName",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId", "ArabicName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_Default",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId" },
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_EnglishName",
                table: "ServiceWorkflows",
                columns: new[] { "BranchId", "LeafServiceId", "EnglishName" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServiceWorkflows_DefaultMustBeActive",
                table: "ServiceWorkflows",
                sql: "[IsDefault] = 0 OR [IsActive] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceWorkflows_Branch_BranchId",
                table: "ServiceWorkflows",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceWorkflows_Service_LeafServiceId",
                table: "ServiceWorkflows",
                column: "LeafServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceWorkflows_Branch_BranchId",
                table: "ServiceWorkflows");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceWorkflows_Service_LeafServiceId",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_ServiceWorkflows_ArabicName",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_ServiceWorkflows_BranchId",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_ServiceWorkflows_BranchId_LeafServiceId",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_ServiceWorkflows_BranchId_LeafServiceId_IsActive",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_ServiceWorkflows_EnglishName",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_ServiceWorkflows_LeafServiceId",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_ArabicName",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_Default",
                table: "ServiceWorkflows");

            migrationBuilder.DropIndex(
                name: "UX_ServiceWorkflows_BranchId_LeafServiceId_EnglishName",
                table: "ServiceWorkflows");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServiceWorkflows_DefaultMustBeActive",
                table: "ServiceWorkflows");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "ServiceWorkflows");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "ServiceWorkflows");

            migrationBuilder.DropColumn(
                name: "LeafServiceId",
                table: "ServiceWorkflows");

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
        }
    }
}
