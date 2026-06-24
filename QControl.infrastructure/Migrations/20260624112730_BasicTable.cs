using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qcontrol.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BasicTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    IsUpdatesAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    License = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch", x => x.Id);
                    table.CheckConstraint("CK_Branch_IPAddress_NotBlank", "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");
                    table.CheckConstraint("CK_Branch_NameRequired", "NULLIF(LTRIM(RTRIM([ArabicName])), N'') IS NOT NULL OR NULLIF(LTRIM(RTRIM([EnglishName])), N'') IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Branch_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branch_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Display",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Number = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Display", x => x.Id);
                    table.CheckConstraint("CK_Display_IPAddress_NotBlank", "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");
                    table.CheckConstraint("CK_Display_Number_Positive", "[Number] > 0");
                    table.ForeignKey(
                        name: "FK_Display_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Display_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Display_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Location_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Location_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WaitingArea",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    AudioDevice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ControlDevice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DescriptiveName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitingArea", x => x.Id);
                    table.CheckConstraint("CK_WaitingArea_Number_Positive", "[Number] > 0");
                    table.ForeignKey(
                        name: "FK_WaitingArea_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaitingArea_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaitingArea_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Window",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescriptiveName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WaitingAreaId = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    EnableTicketBooking = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EnableDirectCall = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Window", x => x.Id);
                    table.CheckConstraint("CK_Window_IPAddress_NotBlank", "[IPAddress] IS NULL OR NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");
                    table.CheckConstraint("CK_Window_Number_NotBlank", "NULLIF(LTRIM(RTRIM([Number])), N'') IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Window_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Window_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Window_WaitingArea_WaitingAreaId",
                        column: x => x.WaitingAreaId,
                        principalTable: "WaitingArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DisplayWindow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayId = table.Column<int>(type: "int", nullable: false),
                    WindowId = table.Column<int>(type: "int", nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayWindow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayWindow_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisplayWindow_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisplayWindow_Display_DisplayId",
                        column: x => x.DisplayId,
                        principalTable: "Display",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisplayWindow_Window_WindowId",
                        column: x => x.WindowId,
                        principalTable: "Window",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Terminal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Number = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    WindowId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terminal", x => x.Id);
                    table.CheckConstraint("CK_Terminal_IPAddress_NotBlank", "NULLIF(LTRIM(RTRIM([IPAddress])), '') IS NOT NULL");
                    table.CheckConstraint("CK_Terminal_Number_Positive", "[Number] > 0");
                    table.ForeignKey(
                        name: "FK_Terminal_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Terminal_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Terminal_Window_WindowId",
                        column: x => x.WindowId,
                        principalTable: "Window",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branch_CreatedByApplicationUserId",
                table: "Branch",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_IPAddress",
                table: "Branch",
                column: "IPAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branch_LastModifiedByApplicationUserId",
                table: "Branch",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Display_BranchId",
                table: "Display",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Display_BranchId_Number",
                table: "Display",
                columns: new[] { "BranchId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Display_CreatedByApplicationUserId",
                table: "Display",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Display_IPAddress",
                table: "Display",
                column: "IPAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Display_LastModifiedByApplicationUserId",
                table: "Display",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Display_SerialNo",
                table: "Display",
                column: "SerialNo",
                unique: true,
                filter: "[SerialNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_CreatedByApplicationUserId",
                table: "DisplayWindow",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_DisplayId",
                table: "DisplayWindow",
                column: "DisplayId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_DisplayId_WindowId",
                table: "DisplayWindow",
                columns: new[] { "DisplayId", "WindowId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_LastModifiedByApplicationUserId",
                table: "DisplayWindow",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_WindowId",
                table: "DisplayWindow",
                column: "WindowId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_BranchId",
                table: "Location",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_CreatedByApplicationUserId",
                table: "Location",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LastModifiedByApplicationUserId",
                table: "Location",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_CreatedByApplicationUserId",
                table: "Terminal",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_IPAddress",
                table: "Terminal",
                column: "IPAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_LastModifiedByApplicationUserId",
                table: "Terminal",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_SerialNo",
                table: "Terminal",
                column: "SerialNo",
                unique: true,
                filter: "[SerialNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_WindowId",
                table: "Terminal",
                column: "WindowId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_WindowId_Number",
                table: "Terminal",
                columns: new[] { "WindowId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaitingArea_BranchId",
                table: "WaitingArea",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitingArea_BranchId_Number",
                table: "WaitingArea",
                columns: new[] { "BranchId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaitingArea_CreatedByApplicationUserId",
                table: "WaitingArea",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitingArea_LastModifiedByApplicationUserId",
                table: "WaitingArea",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Window_CreatedByApplicationUserId",
                table: "Window",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Window_IPAddress",
                table: "Window",
                column: "IPAddress",
                unique: true,
                filter: "[IPAddress] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Window_LastModifiedByApplicationUserId",
                table: "Window",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Window_WaitingAreaId",
                table: "Window",
                column: "WaitingAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Window_WaitingAreaId_Number",
                table: "Window",
                columns: new[] { "WaitingAreaId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisplayWindow");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Terminal");

            migrationBuilder.DropTable(
                name: "Display");

            migrationBuilder.DropTable(
                name: "Window");

            migrationBuilder.DropTable(
                name: "WaitingArea");

            migrationBuilder.DropTable(
                name: "Branch");
        }
    }
}
