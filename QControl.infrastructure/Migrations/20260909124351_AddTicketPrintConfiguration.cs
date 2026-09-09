using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketPrintConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderSnapshot",
                table: "TicketCustomInputValue",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderSnapshot",
                table: "ReservationCustomInputValue",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TicketPrintConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    TicketWidthMm = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    TicketHeightMm = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketPrintConfiguration", x => x.Id);
                    table.CheckConstraint("CK_TicketPrintConfiguration_Height_Positive", "[TicketHeightMm] > 0");
                    table.CheckConstraint("CK_TicketPrintConfiguration_Width_Positive", "[TicketWidthMm] > 0");
                    table.ForeignKey(
                        name: "FK_TicketPrintConfiguration_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketPrintElement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketPrintConfigurationId = table.Column<int>(type: "int", nullable: false),
                    ElementType = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    XMm = table.Column<decimal>(type: "decimal(9,2)", nullable: true),
                    YMm = table.Column<decimal>(type: "decimal(9,2)", nullable: true),
                    WidthMm = table.Column<decimal>(type: "decimal(9,2)", nullable: true),
                    HeightMm = table.Column<decimal>(type: "decimal(9,2)", nullable: true),
                    FontSizePt = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    FontWeight = table.Column<int>(type: "int", nullable: true),
                    TextAlign = table.Column<int>(type: "int", nullable: true),
                    Language = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketPrintElement", x => x.Id);
                    table.CheckConstraint("CK_TicketPrintElement_ElementType_Valid", "[ElementType] IN (1,2,3,4,5,6,7,8)");
                    table.CheckConstraint("CK_TicketPrintElement_VisibleLayout", "[IsVisible] = 0 OR ([XMm] IS NOT NULL AND [YMm] IS NOT NULL AND [WidthMm] IS NOT NULL AND [HeightMm] IS NOT NULL AND [XMm] >= 0 AND [YMm] >= 0 AND [WidthMm] > 0 AND [HeightMm] > 0)");
                    table.CheckConstraint("CK_TicketPrintElement_VisibleTypography", "([ElementType] = 1 AND [FontSizePt] IS NULL AND [FontWeight] IS NULL AND [TextAlign] IS NULL AND [Language] IS NULL) OR ([ElementType] <> 1 AND ([IsVisible] = 0 OR ([FontSizePt] IS NOT NULL AND [FontSizePt] > 0 AND [FontWeight] IN (1,2) AND [TextAlign] IN (1,2,3) AND [Language] IN (1,2))))");
                    table.ForeignKey(
                        name: "FK_TicketPrintElement_TicketPrintConfiguration_TicketPrintConfigurationId",
                        column: x => x.TicketPrintConfigurationId,
                        principalTable: "TicketPrintConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_TicketPrintConfiguration_BranchId",
                table: "TicketPrintConfiguration",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TicketPrintElement_Configuration_ElementType",
                table: "TicketPrintElement",
                columns: new[] { "TicketPrintConfigurationId", "ElementType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketPrintElement");

            migrationBuilder.DropTable(
                name: "TicketPrintConfiguration");

            migrationBuilder.DropColumn(
                name: "OrderSnapshot",
                table: "TicketCustomInputValue");

            migrationBuilder.DropColumn(
                name: "OrderSnapshot",
                table: "ReservationCustomInputValue");
        }
    }
}
