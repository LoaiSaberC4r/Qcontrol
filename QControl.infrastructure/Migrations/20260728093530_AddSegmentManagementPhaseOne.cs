using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSegmentManagementPhaseOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Segment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    OwnerBranchId = table.Column<int>(type: "int", nullable: true),
                    IsSystemDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Segment", x => x.Id);
                    table.CheckConstraint("CK_Segment_Priority_NonNegative", "[Priority] >= 0");
                    table.CheckConstraint("CK_Segment_Scope_Owner", "([Scope] = 1 AND [OwnerBranchId] IS NULL) OR ([Scope] = 2 AND [OwnerBranchId] IS NOT NULL)");
                    table.CheckConstraint("CK_Segment_SystemDefault", "[IsSystemDefault] = 0 OR ([Scope] = 1 AND [OwnerBranchId] IS NULL AND [Priority] = 0)");
                    table.ForeignKey(
                        name: "FK_Segment_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Segment_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Segment_Branch_OwnerBranchId",
                        column: x => x.OwnerBranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchServiceSegment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchServiceId = table.Column<int>(type: "int", nullable: false),
                    SegmentId = table.Column<int>(type: "int", nullable: false),
                    Quota = table.Column<int>(type: "int", nullable: false),
                    CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchServiceSegment", x => x.Id);
                    table.CheckConstraint("CK_BranchServiceSegment_Quota_NonNegative", "[Quota] >= 0");
                    table.ForeignKey(
                        name: "FK_BranchServiceSegment_ApplicationUser_CreatedByApplicationUserId",
                        column: x => x.CreatedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchServiceSegment_ApplicationUser_LastModifiedByApplicationUserId",
                        column: x => x.LastModifiedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchServiceSegment_BranchService_BranchServiceId",
                        column: x => x.BranchServiceId,
                        principalTable: "BranchService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchServiceSegment_Segment_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SegmentGlobalizationRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    SegmentId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentGlobalizationRequest", x => x.Id);
                    table.CheckConstraint("CK_SegmentGlobalizationRequest_ReviewAudit", "(([Status] = 1 AND [ReviewedByApplicationUserId] IS NULL AND [ReviewedOnUtc] IS NULL) OR ([Status] IN (2, 3) AND [ReviewedByApplicationUserId] IS NOT NULL AND [ReviewedOnUtc] IS NOT NULL))");
                    table.CheckConstraint("CK_SegmentGlobalizationRequest_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_SegmentGlobalizationRequest_ApplicationUser_RequestedByApplicationUserId",
                        column: x => x.RequestedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentGlobalizationRequest_ApplicationUser_ReviewedByApplicationUserId",
                        column: x => x.ReviewedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentGlobalizationRequest_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentGlobalizationRequest_Segment_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchServiceSegment_BranchServiceId",
                table: "BranchServiceSegment",
                column: "BranchServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchServiceSegment_CreatedByApplicationUserId",
                table: "BranchServiceSegment",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchServiceSegment_LastModifiedByApplicationUserId",
                table: "BranchServiceSegment",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchServiceSegment_SegmentId",
                table: "BranchServiceSegment",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "UX_BranchServiceSegment_BranchServiceId_SegmentId",
                table: "BranchServiceSegment",
                columns: new[] { "BranchServiceId", "SegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Segment_ArabicName",
                table: "Segment",
                column: "ArabicName");

            migrationBuilder.CreateIndex(
                name: "IX_Segment_CreatedByApplicationUserId",
                table: "Segment",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Segment_EnglishName",
                table: "Segment",
                column: "EnglishName");

            migrationBuilder.CreateIndex(
                name: "IX_Segment_LastModifiedByApplicationUserId",
                table: "Segment",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Segment_OwnerBranchId",
                table: "Segment",
                column: "OwnerBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Segment_Priority",
                table: "Segment",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Segment_Scope_OwnerBranchId",
                table: "Segment",
                columns: new[] { "Scope", "OwnerBranchId" });

            migrationBuilder.CreateIndex(
                name: "UX_Segment_SystemDefault",
                table: "Segment",
                column: "IsSystemDefault",
                unique: true,
                filter: "[IsSystemDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentGlobalizationRequest_BranchId_Status",
                table: "SegmentGlobalizationRequest",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SegmentGlobalizationRequest_RequestedByApplicationUserId",
                table: "SegmentGlobalizationRequest",
                column: "RequestedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentGlobalizationRequest_ReviewedByApplicationUserId",
                table: "SegmentGlobalizationRequest",
                column: "ReviewedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentGlobalizationRequest_SegmentId",
                table: "SegmentGlobalizationRequest",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentGlobalizationRequest_Status_RequestedOnUtc",
                table: "SegmentGlobalizationRequest",
                columns: new[] { "Status", "RequestedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_SegmentGlobalizationRequest_SegmentId_Pending",
                table: "SegmentGlobalizationRequest",
                column: "SegmentId",
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchServiceSegment");

            migrationBuilder.DropTable(
                name: "SegmentGlobalizationRequest");

            migrationBuilder.DropTable(
                name: "Segment");
        }
    }
}
