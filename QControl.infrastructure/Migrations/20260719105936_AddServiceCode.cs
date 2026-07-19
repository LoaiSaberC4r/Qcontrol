using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsServiceCodeRequired",
                table: "Service",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                table: "Service",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Service_ServiceCode",
                table: "Service",
                column: "ServiceCode",
                unique: true,
                filter: "[ServiceCode] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_ServiceCode_Requirement",
                table: "Service",
                sql: "([IsServiceCodeRequired] = 1 AND [ServiceCode] IS NOT NULL AND LEN(LTRIM(RTRIM([ServiceCode]))) > 0) OR ([IsServiceCodeRequired] = 0 AND [ServiceCode] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Service_ServiceCode",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_ServiceCode_Requirement",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "IsServiceCodeRequired",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "ServiceCode",
                table: "Service");
        }
    }
}
