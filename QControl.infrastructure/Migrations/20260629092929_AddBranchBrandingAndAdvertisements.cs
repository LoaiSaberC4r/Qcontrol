using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchBrandingAndAdvertisements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchAdvertisement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeactivatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeactivatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReactivatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ReactivatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchAdvertisement", x => x.Id);
                    table.CheckConstraint("CK_BranchAdvertisement_DeactivationAudit_Pair", "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");
                    table.CheckConstraint("CK_BranchAdvertisement_DisplayOrder_Range", "(([DisplayOrder] >= 1 AND [DisplayOrder] <= 20) OR ([DisplayOrder] >= 1001 AND [DisplayOrder] <= 1020))");
                    table.CheckConstraint("CK_BranchAdvertisement_ImagePath_NotBlank", "NULLIF(LTRIM(RTRIM([ImagePath])), '') IS NOT NULL");
                    table.CheckConstraint("CK_BranchAdvertisement_ReactivationAudit_Pair", "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_BranchAdvertisement_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchAdvertisement_ApplicationUser_DeactivatedByApplicationUserId",
                        column: x => x.DeactivatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchAdvertisement_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchAdvertisement_ApplicationUser_ReactivatedByApplicationUserId",
                        column: x => x.ReactivatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchAdvertisement_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchBranding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    LogoPath = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    MainColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    SecondaryColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    BackgroundColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchBranding", x => x.Id);
                    table.CheckConstraint("CK_BranchBranding_BackgroundColor_Format", "[BackgroundColor] IS NULL OR ([BackgroundColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([BackgroundColor]) = 7)");
                    table.CheckConstraint("CK_BranchBranding_MainColor_Format", "[MainColor] IS NULL OR ([MainColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([MainColor]) = 7)");
                    table.CheckConstraint("CK_BranchBranding_SecondaryColor_Format", "[SecondaryColor] IS NULL OR ([SecondaryColor] COLLATE Latin1_General_BIN2 LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]' AND LEN([SecondaryColor]) = 7)");
                    table.ForeignKey(
                        name: "FK_BranchBranding_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchBranding_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchBranding_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchAdvertisement_BranchId",
                table: "BranchAdvertisement",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchAdvertisement_BranchId_DisplayOrder",
                table: "BranchAdvertisement",
                columns: new[] { "BranchId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchAdvertisement_BranchId_IsActive_DisplayOrder",
                table: "BranchAdvertisement",
                columns: new[] { "BranchId", "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchAdvertisement_CreatedByApplicationUserId",
                table: "BranchAdvertisement",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchAdvertisement_DeactivatedByApplicationUserId",
                table: "BranchAdvertisement",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchAdvertisement_LastModifiedByApplicationUserId",
                table: "BranchAdvertisement",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchAdvertisement_ReactivatedByApplicationUserId",
                table: "BranchAdvertisement",
                column: "ReactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchBranding_BranchId",
                table: "BranchBranding",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchBranding_CreatedByApplicationUserId",
                table: "BranchBranding",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchBranding_LastModifiedByApplicationUserId",
                table: "BranchBranding",
                column: "LastModifiedByApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchAdvertisement");

            migrationBuilder.DropTable(
                name: "BranchBranding");
        }
    }
}
