using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceTicketSettingsAndServiceImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_NoOfTicketCopies_Positive",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_RangeEnd_GreaterOrEqualStart",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_RangePrefix_NotBlank",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_RangeStart_NonNegative",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_WaitingDuration_NonNegative",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_Weight_NonNegative",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Service");

            migrationBuilder.AlterColumn<int>(
                name: "WaitingDuration",
                table: "Service",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "RangeStartNumber",
                table: "Service",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RangePrefix",
                table: "Service",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<int>(
                name: "RangeEndNumber",
                table: "Service",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "NoOfTicketCopies",
                table: "Service",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ServiceImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageType = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceImage", x => x.Id);
                    table.CheckConstraint("CK_ServiceImage_DisplayOrder_NonNegative", "[DisplayOrder] >= 0");
                    table.CheckConstraint("CK_ServiceImage_ImagePath_NotBlank", "LEN(LTRIM(RTRIM([ImagePath]))) > 0");
                    table.CheckConstraint("CK_ServiceImage_ImageType_Valid", "[ImageType] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_ServiceImage_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceImage_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceImage_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO [ServiceImage]
                    ([ServiceId],
                     [ImagePath],
                     [ImageType],
                     [DisplayOrder],
                     [IsActive],
                     [CreatedByApplicationUserId],
                     [LastModifiedByApplicationUserId],
                     [CreatedOnUtc],
                     [ModifiedOnUtc])
                SELECT
                    [Id],
                    [Logo],
                    1,
                    0,
                    CAST(1 AS bit),
                    [CreatedByApplicationUserId],
                    [LastModifiedByApplicationUserId],
                    [CreatedOnUtc],
                    [ModifiedOnUtc]
                FROM [Service]
                WHERE [Logo] IS NOT NULL
                    AND LEN(LTRIM(RTRIM([Logo]))) > 0;
                """);

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Service");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_NoOfTicketCopies_NullOrPositive",
                table: "Service",
                sql: "[NoOfTicketCopies] IS NULL OR [NoOfTicketCopies] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_RangeEnd_NullOrGreaterOrEqualStart",
                table: "Service",
                sql: "[RangeStartNumber] IS NULL OR [RangeEndNumber] IS NULL OR [RangeEndNumber] >= [RangeStartNumber]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_RangeEnd_NullOrNonNegative",
                table: "Service",
                sql: "[RangeEndNumber] IS NULL OR [RangeEndNumber] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_RangePrefix_NullOrNotBlank",
                table: "Service",
                sql: "[RangePrefix] IS NULL OR LEN(LTRIM(RTRIM([RangePrefix]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_RangeStart_NullOrNonNegative",
                table: "Service",
                sql: "[RangeStartNumber] IS NULL OR [RangeStartNumber] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_TicketIssuable_Settings_Required",
                table: "Service",
                sql: "[IsTicketIssuable] = 0 OR ([RangePrefix] IS NOT NULL AND LEN(LTRIM(RTRIM([RangePrefix]))) > 0 AND [RangeStartNumber] IS NOT NULL AND [RangeEndNumber] IS NOT NULL AND [WaitingDuration] IS NOT NULL AND [NoOfTicketCopies] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_WaitingDuration_NullOrNonNegative",
                table: "Service",
                sql: "[WaitingDuration] IS NULL OR [WaitingDuration] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceImage_CreatedByApplicationUserId",
                table: "ServiceImage",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceImage_LastModifiedByApplicationUserId",
                table: "ServiceImage",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceImage_ServiceId",
                table: "ServiceImage",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceImage_ServiceId_ImageType_DisplayOrder",
                table: "ServiceImage",
                columns: new[] { "ServiceId", "ImageType", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "UX_ServiceImage_ServiceId_ImageType_LogoIcon",
                table: "ServiceImage",
                columns: new[] { "ServiceId", "ImageType" },
                unique: true,
                filter: "[ImageType] IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_NoOfTicketCopies_NullOrPositive",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_RangeEnd_NullOrGreaterOrEqualStart",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_RangeEnd_NullOrNonNegative",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_RangePrefix_NullOrNotBlank",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_RangeStart_NullOrNonNegative",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_TicketIssuable_Settings_Required",
                table: "Service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Service_WaitingDuration_NullOrNonNegative",
                table: "Service");

            migrationBuilder.AlterColumn<int>(
                name: "WaitingDuration",
                table: "Service",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RangeStartNumber",
                table: "Service",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RangePrefix",
                table: "Service",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RangeEndNumber",
                table: "Service",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NoOfTicketCopies",
                table: "Service",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Service",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [Service]
                SET [Logo] = [ServiceImage].[ImagePath]
                FROM [Service]
                INNER JOIN [ServiceImage]
                    ON [ServiceImage].[ServiceId] = [Service].[Id]
                    AND [ServiceImage].[ImageType] = 1
                    AND [ServiceImage].[IsActive] = CAST(1 AS bit);
                """);

            migrationBuilder.DropTable(
                name: "ServiceImage");

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "Service",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_NoOfTicketCopies_Positive",
                table: "Service",
                sql: "[NoOfTicketCopies] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_RangeEnd_GreaterOrEqualStart",
                table: "Service",
                sql: "[RangeEndNumber] >= [RangeStartNumber]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_RangePrefix_NotBlank",
                table: "Service",
                sql: "LEN(LTRIM(RTRIM([RangePrefix]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_RangeStart_NonNegative",
                table: "Service",
                sql: "[RangeStartNumber] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_WaitingDuration_NonNegative",
                table: "Service",
                sql: "[WaitingDuration] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Service_Weight_NonNegative",
                table: "Service",
                sql: "[Weight] >= 0");
        }
    }
}
