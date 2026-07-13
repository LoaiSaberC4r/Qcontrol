using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceGlobalizationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceGlobalizationRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    RootServiceId = table.Column<int>(type: "int", nullable: false),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceGlobalizationRequest", x => x.Id);
                    table.CheckConstraint("CK_ServiceGlobalizationRequest_RequestType", "[RequestType] IN (1, 2)");
                    table.CheckConstraint("CK_ServiceGlobalizationRequest_ReviewAudit", "(([Status] = 1 AND [ReviewedByApplicationUserId] IS NULL AND [ReviewedOnUtc] IS NULL) OR ([Status] IN (2, 3) AND [ReviewedByApplicationUserId] IS NOT NULL AND [ReviewedOnUtc] IS NOT NULL))");
                    table.CheckConstraint("CK_ServiceGlobalizationRequest_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_ServiceGlobalizationRequest_ApplicationUser_RequestedByApplicationUserId",
                        column: x => x.RequestedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceGlobalizationRequest_ApplicationUser_ReviewedByApplicationUserId",
                        column: x => x.ReviewedByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceGlobalizationRequest_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceGlobalizationRequest_Service_RootServiceId",
                        column: x => x.RootServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceGlobalizationRequestItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceGlobalizationRequestItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceGlobalizationRequestItem_ServiceGlobalizationRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceGlobalizationRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceGlobalizationRequestItem_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGlobalizationRequest_BranchId_Status",
                table: "ServiceGlobalizationRequest",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGlobalizationRequest_RequestedByApplicationUserId",
                table: "ServiceGlobalizationRequest",
                column: "RequestedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGlobalizationRequest_ReviewedByApplicationUserId",
                table: "ServiceGlobalizationRequest",
                column: "ReviewedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGlobalizationRequest_RootServiceId",
                table: "ServiceGlobalizationRequest",
                column: "RootServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGlobalizationRequest_Status_RequestedOnUtc",
                table: "ServiceGlobalizationRequest",
                columns: new[] { "Status", "RequestedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_ServiceGlobalizationRequest_RootServiceId_Pending",
                table: "ServiceGlobalizationRequest",
                column: "RootServiceId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGlobalizationRequestItem_RequestId",
                table: "ServiceGlobalizationRequestItem",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGlobalizationRequestItem_ServiceId",
                table: "ServiceGlobalizationRequestItem",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "UX_ServiceGlobalizationRequestItem_RequestId_ServiceId",
                table: "ServiceGlobalizationRequestItem",
                columns: new[] { "RequestId", "ServiceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceGlobalizationRequestItem");

            migrationBuilder.DropTable(
                name: "ServiceGlobalizationRequest");
        }
    }
}
