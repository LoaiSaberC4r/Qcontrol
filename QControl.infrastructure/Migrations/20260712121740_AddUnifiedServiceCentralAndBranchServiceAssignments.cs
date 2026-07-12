using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedServiceCentralAndBranchServiceAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Service_ArabicName_ParentServiceId",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "UX_Service_EnglishName_ParentServiceId",
                table: "Service");

            migrationBuilder.AddColumn<int>(
                name: "OwnerBranchId",
                table: "Service",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "Service",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [Service] SET [Scope] = 1, [OwnerBranchId] = NULL");

            migrationBuilder.AlterColumn<int>(
                name: "Scope",
                table: "Service",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BranchService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchService_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchService_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchService_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Service_OwnerBranchId",
                table: "Service",
                column: "OwnerBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_ParentServiceId_Scope_OwnerBranchId",
                table: "Service",
                columns: new[] { "ParentServiceId", "Scope", "OwnerBranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Service_Scope",
                table: "Service",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_Service_Scope_OwnerBranchId",
                table: "Service",
                columns: new[] { "Scope", "OwnerBranchId" });

            migrationBuilder.CreateIndex(
                name: "UX_Service_ArabicName_ParentServiceId_Scope_OwnerBranchId",
                table: "Service",
                columns: new[] { "ArabicName", "ParentServiceId", "Scope", "OwnerBranchId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Service_EnglishName_ParentServiceId_Scope_OwnerBranchId",
                table: "Service",
                columns: new[] { "EnglishName", "ParentServiceId", "Scope", "OwnerBranchId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_Scope_Owner",
                table: "Service",
                sql: "([Scope] = 1 AND [OwnerBranchId] IS NULL) OR ([Scope] = 2 AND [OwnerBranchId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_BranchService_BranchId",
                table: "BranchService",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchService_CreatedByApplicationUserId",
                table: "BranchService",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchService_ServiceId",
                table: "BranchService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "UX_BranchService_BranchId_ServiceId",
                table: "BranchService",
                columns: new[] { "BranchId", "ServiceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Service_Branch_OwnerBranchId",
                table: "Service",
                column: "OwnerBranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Service_Branch_OwnerBranchId",
                table: "Service");

            migrationBuilder.DropTable(
                name: "BranchService");

            migrationBuilder.DropIndex(
                name: "IX_Service_OwnerBranchId",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "IX_Service_ParentServiceId_Scope_OwnerBranchId",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "IX_Service_Scope",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "IX_Service_Scope_OwnerBranchId",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "UX_Service_ArabicName_ParentServiceId_Scope_OwnerBranchId",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "UX_Service_EnglishName_ParentServiceId_Scope_OwnerBranchId",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_Scope_Owner",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "OwnerBranchId",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Service");

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
    }
}
