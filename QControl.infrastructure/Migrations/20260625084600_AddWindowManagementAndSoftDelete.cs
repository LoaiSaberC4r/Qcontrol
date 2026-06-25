using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qcontrol.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWindowManagementAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Window_IPAddress",
                table: "Window");

            migrationBuilder.RenameIndex(
                name: "IX_Window_WaitingAreaId_Number",
                table: "Window",
                newName: "UX_Window_WaitingAreaId_Number");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Window",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Window",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredOnUtc",
                table: "Window",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Window_WaitingAreaId_IPAddress",
                table: "Window",
                columns: new[] { "WaitingAreaId", "IPAddress" },
                unique: true,
                filter: "[IPAddress] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Window_WaitingAreaId_IPAddress",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "RestoredOnUtc",
                table: "Window");

            migrationBuilder.RenameIndex(
                name: "UX_Window_WaitingAreaId_Number",
                table: "Window",
                newName: "IX_Window_WaitingAreaId_Number");

            migrationBuilder.CreateIndex(
                name: "IX_Window_IPAddress",
                table: "Window",
                column: "IPAddress",
                unique: true,
                filter: "[IPAddress] IS NOT NULL");
        }
    }
}
