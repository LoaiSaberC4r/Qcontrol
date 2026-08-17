using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchDisplayVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchVideo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    OriginalPath = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    HlsManifestPath = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_BranchVideo", x => x.Id);
                    table.CheckConstraint("CK_BranchVideo_DeactivationAudit_Pair", "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");
                    table.CheckConstraint("CK_BranchVideo_DisplayOrder_Positive", "[DisplayOrder] > 0");
                    table.CheckConstraint("CK_BranchVideo_OriginalFileName_NotBlank", "NULLIF(LTRIM(RTRIM([OriginalFileName])), '') IS NOT NULL");
                    table.CheckConstraint("CK_BranchVideo_OriginalPath_NotBlank", "NULLIF(LTRIM(RTRIM([OriginalPath])), '') IS NOT NULL");
                    table.CheckConstraint("CK_BranchVideo_ReactivationAudit_Pair", "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_BranchVideo_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchVideo_ApplicationUser_DeactivatedByApplicationUserId",
                        column: x => x.DeactivatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchVideo_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchVideo_ApplicationUser_ReactivatedByApplicationUserId",
                        column: x => x.ReactivatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchVideo_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchVideo_BranchId_DisplayOrder",
                table: "BranchVideo",
                columns: new[] { "BranchId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchVideo_BranchId_IsActive_ProcessingStatus_DisplayOrder",
                table: "BranchVideo",
                columns: new[] { "BranchId", "IsActive", "ProcessingStatus", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchVideo_CreatedByApplicationUserId",
                table: "BranchVideo",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchVideo_DeactivatedByApplicationUserId",
                table: "BranchVideo",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchVideo_LastModifiedByApplicationUserId",
                table: "BranchVideo",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchVideo_ReactivatedByApplicationUserId",
                table: "BranchVideo",
                column: "ReactivatedByApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchVideo");
        }
    }
}
