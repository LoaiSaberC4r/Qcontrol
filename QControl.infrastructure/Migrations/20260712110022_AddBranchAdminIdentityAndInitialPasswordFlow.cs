using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchAdminIdentityAndInitialPasswordFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_Display_DisplayId_BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_Window_WindowId_BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropIndex(
                name: "IX_DisplayWindow_DisplayId_BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropIndex(
                name: "IX_DisplayWindow_WindowId_BranchId",
                table: "DisplayWindow");

            migrationBuilder.RenameIndex(
                name: "IX_UserRole_ApplicationUserId_RoleId",
                table: "UserRole",
                newName: "UX_UserRole_ApplicationUserId_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationUser_UserName",
                table: "ApplicationUser",
                newName: "UX_ApplicationUser_UserName");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationUser_Email",
                table: "ApplicationUser",
                newName: "UX_ApplicationUser_Email");

            migrationBuilder.Sql("""
IF NOT EXISTS (
    SELECT 1
    FROM [Role]
    WHERE [Id] = '20000000-0000-0000-0000-000000000002'
       OR [Name] = N'Branch Administrator')
BEGIN
    INSERT INTO [Role]
        ([Id], [Name], [IsSystemRole], [CreatedByApplicationUserId], [CreatedOnUtc], [ModifiedOnUtc])
    VALUES
        ('20000000-0000-0000-0000-000000000002',
         N'Branch Administrator',
         CAST(1 AS bit),
         '10000000-0000-0000-0000-000000000001',
         CONVERT(datetime2, '2026-07-12T00:00:00'),
         NULL);
END
""");

            migrationBuilder.Sql("""
IF NOT EXISTS (
    SELECT 1
    FROM [Permission]
    WHERE [Id] = '30000000-0000-0000-0000-000000000062'
       OR [Name] = N'BranchAdmins.Create')
BEGIN
    INSERT INTO [Permission]
        ([Id], [Name], [IsSystemPermission], [CreatedByApplicationUserId], [CreatedOnUtc], [ModifiedOnUtc])
    VALUES
        ('30000000-0000-0000-0000-000000000062',
         N'BranchAdmins.Create',
         CAST(1 AS bit),
         '10000000-0000-0000-0000-000000000001',
         CONVERT(datetime2, '2026-07-12T00:00:00'),
         NULL);
END
""");

            migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM [Role] WHERE [Id] = '20000000-0000-0000-0000-000000000001')
   AND EXISTS (SELECT 1 FROM [Permission] WHERE [Id] = '30000000-0000-0000-0000-000000000062')
   AND NOT EXISTS (
        SELECT 1
        FROM [RolePermission]
        WHERE [RoleId] = '20000000-0000-0000-0000-000000000001'
          AND [PermissionId] = '30000000-0000-0000-0000-000000000062')
BEGIN
    INSERT INTO [RolePermission]
        ([Id], [RoleId], [PermissionId], [CreatedByApplicationUserId], [CreatedOnUtc], [ModifiedOnUtc])
    VALUES
        ('40000000-0000-0000-0000-000000000001',
         '20000000-0000-0000-0000-000000000001',
         '30000000-0000-0000-0000-000000000062',
         '10000000-0000-0000-0000-000000000001',
         CONVERT(datetime2, '2026-07-12T00:00:00'),
         NULL);
END
""");

            migrationBuilder.CreateTable(
                name: "ApplicationUserBranch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserBranch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserBranch_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationUserBranch_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchAdmin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchAdmin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchAdmin_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserBranch_ApplicationUserId",
                table: "ApplicationUserBranch",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserBranch_BranchId",
                table: "ApplicationUserBranch",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "UX_ApplicationUserBranch_ApplicationUserId_BranchId",
                table: "ApplicationUserBranch",
                columns: new[] { "ApplicationUserId", "BranchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BranchAdmin_ApplicationUserId",
                table: "BranchAdmin",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_Display_DisplayId",
                table: "DisplayWindow",
                column: "DisplayId",
                principalTable: "Display",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_Window_WindowId",
                table: "DisplayWindow",
                column: "WindowId",
                principalTable: "Window",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DELETE FROM [RolePermission]
WHERE [RoleId] = '20000000-0000-0000-0000-000000000001'
  AND [PermissionId] = '30000000-0000-0000-0000-000000000062';

DELETE FROM [Permission]
WHERE ([Id] = '30000000-0000-0000-0000-000000000062'
   OR [Name] = N'BranchAdmins.Create')
  AND NOT EXISTS (
      SELECT 1
      FROM [RolePermission]
      WHERE [PermissionId] = '30000000-0000-0000-0000-000000000062');

DELETE FROM [Role]
WHERE ([Id] = '20000000-0000-0000-0000-000000000002'
   OR [Name] = N'Branch Administrator')
  AND NOT EXISTS (
      SELECT 1
      FROM [UserRole]
      WHERE [RoleId] = '20000000-0000-0000-0000-000000000002');
""");

            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_Display_DisplayId",
                table: "DisplayWindow");

            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_Window_WindowId",
                table: "DisplayWindow");

            migrationBuilder.DropTable(
                name: "ApplicationUserBranch");

            migrationBuilder.DropTable(
                name: "BranchAdmin");

            migrationBuilder.RenameIndex(
                name: "UX_UserRole_ApplicationUserId_RoleId",
                table: "UserRole",
                newName: "IX_UserRole_ApplicationUserId_RoleId");

            migrationBuilder.RenameIndex(
                name: "UX_ApplicationUser_UserName",
                table: "ApplicationUser",
                newName: "IX_ApplicationUser_UserName");

            migrationBuilder.RenameIndex(
                name: "UX_ApplicationUser_Email",
                table: "ApplicationUser",
                newName: "IX_ApplicationUser_Email");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_DisplayId_BranchId",
                table: "DisplayWindow",
                columns: new[] { "DisplayId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_WindowId_BranchId",
                table: "DisplayWindow",
                columns: new[] { "WindowId", "BranchId" });

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_Display_DisplayId_BranchId",
                table: "DisplayWindow",
                columns: new[] { "DisplayId", "BranchId" },
                principalTable: "Display",
                principalColumns: new[] { "Id", "BranchId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_Window_WindowId_BranchId",
                table: "DisplayWindow",
                columns: new[] { "WindowId", "BranchId" },
                principalTable: "Window",
                principalColumns: new[] { "Id", "BranchId" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
