using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreserveOriginalCustomInputIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderSnapshot",
                table: "TicketCustomInputValueArchive",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalServiceCustomInputId",
                table: "TicketCustomInputValueArchive",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalServiceCustomInputId",
                table: "TicketCustomInputValue",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderSnapshot",
                table: "ReservationCustomInputValueArchive",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalServiceCustomInputId",
                table: "ReservationCustomInputValueArchive",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalServiceCustomInputId",
                table: "ReservationCustomInputValue",
                type: "int",
                nullable: true);

            // Defer compilation until the new columns exist in the idempotent script's batch.
            migrationBuilder.Sql("""
                EXEC(N'
                UPDATE [ReservationCustomInputValue]
                SET [OriginalServiceCustomInputId] = [ServiceCustomInputId]
                WHERE [ServiceCustomInputId] IS NOT NULL;

                UPDATE [TicketCustomInputValue]
                SET [OriginalServiceCustomInputId] = [ServiceCustomInputId]
                WHERE [ServiceCustomInputId] IS NOT NULL;

                UPDATE [ReservationCustomInputValueArchive]
                SET [OriginalServiceCustomInputId] = [ServiceCustomInputId]
                WHERE [ServiceCustomInputId] IS NOT NULL;

                UPDATE [TicketCustomInputValueArchive]
                SET [OriginalServiceCustomInputId] = [ServiceCustomInputId]
                WHERE [ServiceCustomInputId] IS NOT NULL;
                ');
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCustomInputValue_Reservation_OriginalInput",
                table: "ReservationCustomInputValue",
                columns: new[] { "ReservationId", "OriginalServiceCustomInputId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReservationCustomInputValue_Reservation_OriginalInput",
                table: "ReservationCustomInputValue");

            migrationBuilder.DropColumn(
                name: "OrderSnapshot",
                table: "TicketCustomInputValueArchive");

            migrationBuilder.DropColumn(
                name: "OriginalServiceCustomInputId",
                table: "TicketCustomInputValueArchive");

            migrationBuilder.DropColumn(
                name: "OriginalServiceCustomInputId",
                table: "TicketCustomInputValue");

            migrationBuilder.DropColumn(
                name: "OrderSnapshot",
                table: "ReservationCustomInputValueArchive");

            migrationBuilder.DropColumn(
                name: "OriginalServiceCustomInputId",
                table: "ReservationCustomInputValueArchive");

            migrationBuilder.DropColumn(
                name: "OriginalServiceCustomInputId",
                table: "ReservationCustomInputValue");
        }
    }
}
