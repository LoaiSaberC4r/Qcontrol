using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKioskReservationLookupValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LookupValue",
                table: "TicketArchive",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LookupValue",
                table: "Ticket",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LookupValue",
                table: "ReservationArchive",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LookupValue",
                table: "Reservation",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCustomInputValue_Reservation_Input",
                table: "ReservationCustomInputValue",
                columns: new[] { "ReservationId", "ServiceCustomInputId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReservationCustomInputValue_Reservation_Input",
                table: "ReservationCustomInputValue");

            migrationBuilder.DropColumn(
                name: "LookupValue",
                table: "TicketArchive");

            migrationBuilder.DropColumn(
                name: "LookupValue",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "LookupValue",
                table: "ReservationArchive");

            migrationBuilder.DropColumn(
                name: "LookupValue",
                table: "Reservation");
        }
    }
}
