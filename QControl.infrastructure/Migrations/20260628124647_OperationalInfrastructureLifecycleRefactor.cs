using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OperationalInfrastructureLifecycleRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_ApplicationUser_LastModifiedByApplicationUserId",
                table: "DisplayWindow");

            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_Display_DisplayId",
                table: "DisplayWindow");

            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_Window_WindowId",
                table: "DisplayWindow");

            migrationBuilder.DropForeignKey(
                name: "FK_Location_ApplicationUser_CreatedByApplicationUserId",
                table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_Location_ApplicationUser_LastModifiedByApplicationUserId",
                table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_Terminal_Window_WindowId",
                table: "Terminal");

            migrationBuilder.DropForeignKey(
                name: "FK_Window_WaitingArea_WaitingAreaId",
                table: "Window");

            migrationBuilder.DropIndex(
                name: "IX_Terminal_IPAddress",
                table: "Terminal");

            migrationBuilder.DropIndex(
                name: "IX_Terminal_SerialNo",
                table: "Terminal");

            migrationBuilder.DropIndex(
                name: "IX_Location_CreatedByApplicationUserId",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_LastModifiedByApplicationUserId",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_DisplayWindow_LastModifiedByApplicationUserId",
                table: "DisplayWindow");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Branch_NameRequired",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "RestoredOnUtc",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "RestoredOnUtc",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "CreatedByApplicationUserId",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "LastModifiedByApplicationUserId",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "LastModifiedByApplicationUserId",
                table: "DisplayWindow");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "RestoredOnUtc",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "IsUpdatesAvailable",
                table: "Branch");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Branch",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Window",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE [w]
SET [BranchId] = [wa].[BranchId]
FROM [Window] AS [w]
INNER JOIN [WaitingArea] AS [wa] ON [w].[WaitingAreaId] = [wa].[Id];
");

            migrationBuilder.AddColumn<Guid>(
                name: "DeactivatedByApplicationUserId",
                table: "Window",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOnUtc",
                table: "Window",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Window",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReactivatedByApplicationUserId",
                table: "Window",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivatedOnUtc",
                table: "Window",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Window",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "DeactivatedByApplicationUserId",
                table: "WaitingArea",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOnUtc",
                table: "WaitingArea",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WaitingArea",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReactivatedByApplicationUserId",
                table: "WaitingArea",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivatedOnUtc",
                table: "WaitingArea",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WaitingArea",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Terminal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE [t]
SET [BranchId] = [w].[BranchId]
FROM [Terminal] AS [t]
INNER JOIN [Window] AS [w] ON [t].[WindowId] = [w].[Id];
");

            migrationBuilder.AddColumn<Guid>(
                name: "DeactivatedByApplicationUserId",
                table: "Terminal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOnUtc",
                table: "Terminal",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Terminal",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReactivatedByApplicationUserId",
                table: "Terminal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivatedOnUtc",
                table: "Terminal",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Terminal",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.Sql(@"
UPDATE [Location]
SET [Governorate] = N'Unknown'
WHERE NULLIF(LTRIM(RTRIM([Governorate])), N'') IS NULL;

UPDATE [Location]
SET [City] = N'Unknown'
WHERE NULLIF(LTRIM(RTRIM([City])), N'') IS NULL;

UPDATE [Location]
SET [Area] = N'Unknown'
WHERE NULLIF(LTRIM(RTRIM([Area])), N'') IS NULL;

UPDATE [Location]
SET [Address] = N'Unknown'
WHERE NULLIF(LTRIM(RTRIM([Address])), N'') IS NULL;

UPDATE [Location]
SET [Latitude] = N'0'
WHERE TRY_CONVERT(decimal(9,6), [Latitude]) IS NULL
   OR TRY_CONVERT(decimal(9,6), [Latitude]) < -90
   OR TRY_CONVERT(decimal(9,6), [Latitude]) > 90;

UPDATE [Location]
SET [Longitude] = N'0'
WHERE TRY_CONVERT(decimal(9,6), [Longitude]) IS NULL
   OR TRY_CONVERT(decimal(9,6), [Longitude]) < -180
   OR TRY_CONVERT(decimal(9,6), [Longitude]) > 180;
");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Location",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Location",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Governorate",
                table: "Location",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Location",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                table: "Location",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Location",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "DisplayWindow",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
DELETE [dw]
FROM [DisplayWindow] AS [dw]
INNER JOIN [Display] AS [d] ON [dw].[DisplayId] = [d].[Id]
INNER JOIN [Window] AS [w] ON [dw].[WindowId] = [w].[Id]
WHERE [d].[BranchId] <> [w].[BranchId];

UPDATE [dw]
SET [BranchId] = [d].[BranchId]
FROM [DisplayWindow] AS [dw]
INNER JOIN [Display] AS [d] ON [dw].[DisplayId] = [d].[Id];
");

            migrationBuilder.AddColumn<Guid>(
                name: "DeactivatedByApplicationUserId",
                table: "Display",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOnUtc",
                table: "Display",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Display",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReactivatedByApplicationUserId",
                table: "Display",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivatedOnUtc",
                table: "Display",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Display",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.Sql(@"
UPDATE [Branch]
SET [ArabicName] = CONCAT(N'Arabic Branch ', [Id])
WHERE NULLIF(LTRIM(RTRIM([ArabicName])), N'') IS NULL;

UPDATE [Branch]
SET [EnglishName] = CONCAT(N'English Branch ', [Id])
WHERE NULLIF(LTRIM(RTRIM([EnglishName])), N'') IS NULL;
");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "Branch",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArabicName",
                table: "Branch",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeactivatedByApplicationUserId",
                table: "Branch",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOnUtc",
                table: "Branch",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReactivatedByApplicationUserId",
                table: "Branch",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReactivatedOnUtc",
                table: "Branch",
                type: "datetime2(3)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Branch",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Window_Id_BranchId",
                table: "Window",
                columns: new[] { "Id", "BranchId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_WaitingArea_Id_BranchId",
                table: "WaitingArea",
                columns: new[] { "Id", "BranchId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Display_Id_BranchId",
                table: "Display",
                columns: new[] { "Id", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Window_BranchId",
                table: "Window",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Window_DeactivatedByApplicationUserId",
                table: "Window",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Window_ReactivatedByApplicationUserId",
                table: "Window",
                column: "ReactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Window_WaitingAreaId_BranchId",
                table: "Window",
                columns: new[] { "WaitingAreaId", "BranchId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Window_DeactivationAudit_Pair",
                table: "Window",
                sql: "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Window_ReactivationAudit_Pair",
                table: "Window",
                sql: "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_WaitingArea_DeactivatedByApplicationUserId",
                table: "WaitingArea",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitingArea_ReactivatedByApplicationUserId",
                table: "WaitingArea",
                column: "ReactivatedByApplicationUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WaitingArea_DeactivationAudit_Pair",
                table: "WaitingArea",
                sql: "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WaitingArea_ReactivationAudit_Pair",
                table: "WaitingArea",
                sql: "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_BranchId",
                table: "Terminal",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_BranchId_IPAddress",
                table: "Terminal",
                columns: new[] { "BranchId", "IPAddress" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_BranchId_SerialNo",
                table: "Terminal",
                columns: new[] { "BranchId", "SerialNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_DeactivatedByApplicationUserId",
                table: "Terminal",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_ReactivatedByApplicationUserId",
                table: "Terminal",
                column: "ReactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_WindowId_BranchId",
                table: "Terminal",
                columns: new[] { "WindowId", "BranchId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Terminal_DeactivationAudit_Pair",
                table: "Terminal",
                sql: "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Terminal_ReactivationAudit_Pair",
                table: "Terminal",
                sql: "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Location_Address_Required",
                table: "Location",
                sql: "NULLIF(LTRIM(RTRIM([Address])), N'') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Location_Area_Required",
                table: "Location",
                sql: "NULLIF(LTRIM(RTRIM([Area])), N'') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Location_City_Required",
                table: "Location",
                sql: "NULLIF(LTRIM(RTRIM([City])), N'') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Location_Governorate_Required",
                table: "Location",
                sql: "NULLIF(LTRIM(RTRIM([Governorate])), N'') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Location_Latitude_Range",
                table: "Location",
                sql: "[Latitude] >= -90 AND [Latitude] <= 90");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Location_Longitude_Range",
                table: "Location",
                sql: "[Longitude] >= -180 AND [Longitude] <= 180");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_BranchId",
                table: "DisplayWindow",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_DisplayId_BranchId",
                table: "DisplayWindow",
                columns: new[] { "DisplayId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_WindowId_BranchId",
                table: "DisplayWindow",
                columns: new[] { "WindowId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Display_DeactivatedByApplicationUserId",
                table: "Display",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Display_ReactivatedByApplicationUserId",
                table: "Display",
                column: "ReactivatedByApplicationUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Display_DeactivationAudit_Pair",
                table: "Display",
                sql: "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Display_ReactivationAudit_Pair",
                table: "Display",
                sql: "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_DeactivatedByApplicationUserId",
                table: "Branch",
                column: "DeactivatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_ReactivatedByApplicationUserId",
                table: "Branch",
                column: "ReactivatedByApplicationUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Branch_ArabicName_Required",
                table: "Branch",
                sql: "NULLIF(LTRIM(RTRIM([ArabicName])), N'') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Branch_DeactivationAudit_Pair",
                table: "Branch",
                sql: "(([DeactivatedOnUtc] IS NULL AND [DeactivatedByApplicationUserId] IS NULL) OR ([DeactivatedOnUtc] IS NOT NULL AND [DeactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Branch_EnglishName_Required",
                table: "Branch",
                sql: "NULLIF(LTRIM(RTRIM([EnglishName])), N'') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Branch_ReactivationAudit_Pair",
                table: "Branch",
                sql: "(([ReactivatedOnUtc] IS NULL AND [ReactivatedByApplicationUserId] IS NULL) OR ([ReactivatedOnUtc] IS NOT NULL AND [ReactivatedByApplicationUserId] IS NOT NULL))");

            migrationBuilder.AddForeignKey(
                name: "FK_Branch_ApplicationUser_DeactivatedByApplicationUserId",
                table: "Branch",
                column: "DeactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branch_ApplicationUser_ReactivatedByApplicationUserId",
                table: "Branch",
                column: "ReactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Display_ApplicationUser_DeactivatedByApplicationUserId",
                table: "Display",
                column: "DeactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Display_ApplicationUser_ReactivatedByApplicationUserId",
                table: "Display",
                column: "ReactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_Display_DisplayId_BranchId",
                table: "DisplayWindow",
                columns: new[] { "DisplayId", "BranchId" },
                principalTable: "Display",
                principalColumns: new[] { "Id", "BranchId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_Window_WindowId_BranchId",
                table: "DisplayWindow",
                columns: new[] { "WindowId", "BranchId" },
                principalTable: "Window",
                principalColumns: new[] { "Id", "BranchId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Terminal_ApplicationUser_DeactivatedByApplicationUserId",
                table: "Terminal",
                column: "DeactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Terminal_ApplicationUser_ReactivatedByApplicationUserId",
                table: "Terminal",
                column: "ReactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Terminal_Window_WindowId_BranchId",
                table: "Terminal",
                columns: new[] { "WindowId", "BranchId" },
                principalTable: "Window",
                principalColumns: new[] { "Id", "BranchId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WaitingArea_ApplicationUser_DeactivatedByApplicationUserId",
                table: "WaitingArea",
                column: "DeactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WaitingArea_ApplicationUser_ReactivatedByApplicationUserId",
                table: "WaitingArea",
                column: "ReactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Window_ApplicationUser_DeactivatedByApplicationUserId",
                table: "Window",
                column: "DeactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Window_ApplicationUser_ReactivatedByApplicationUserId",
                table: "Window",
                column: "ReactivatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Window_WaitingArea_WaitingAreaId_BranchId",
                table: "Window",
                columns: new[] { "WaitingAreaId", "BranchId" },
                principalTable: "WaitingArea",
                principalColumns: new[] { "Id", "BranchId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branch_ApplicationUser_DeactivatedByApplicationUserId",
                table: "Branch");

            migrationBuilder.DropForeignKey(
                name: "FK_Branch_ApplicationUser_ReactivatedByApplicationUserId",
                table: "Branch");

            migrationBuilder.DropForeignKey(
                name: "FK_Display_ApplicationUser_DeactivatedByApplicationUserId",
                table: "Display");

            migrationBuilder.DropForeignKey(
                name: "FK_Display_ApplicationUser_ReactivatedByApplicationUserId",
                table: "Display");

            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_Display_DisplayId_BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropForeignKey(
                name: "FK_DisplayWindow_Window_WindowId_BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropForeignKey(
                name: "FK_Terminal_ApplicationUser_DeactivatedByApplicationUserId",
                table: "Terminal");

            migrationBuilder.DropForeignKey(
                name: "FK_Terminal_ApplicationUser_ReactivatedByApplicationUserId",
                table: "Terminal");

            migrationBuilder.DropForeignKey(
                name: "FK_Terminal_Window_WindowId_BranchId",
                table: "Terminal");

            migrationBuilder.DropForeignKey(
                name: "FK_WaitingArea_ApplicationUser_DeactivatedByApplicationUserId",
                table: "WaitingArea");

            migrationBuilder.DropForeignKey(
                name: "FK_WaitingArea_ApplicationUser_ReactivatedByApplicationUserId",
                table: "WaitingArea");

            migrationBuilder.DropForeignKey(
                name: "FK_Window_ApplicationUser_DeactivatedByApplicationUserId",
                table: "Window");

            migrationBuilder.DropForeignKey(
                name: "FK_Window_ApplicationUser_ReactivatedByApplicationUserId",
                table: "Window");

            migrationBuilder.DropForeignKey(
                name: "FK_Window_WaitingArea_WaitingAreaId_BranchId",
                table: "Window");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Window_Id_BranchId",
                table: "Window");

            migrationBuilder.DropIndex(
                name: "IX_Window_BranchId",
                table: "Window");

            migrationBuilder.DropIndex(
                name: "IX_Window_DeactivatedByApplicationUserId",
                table: "Window");

            migrationBuilder.DropIndex(
                name: "IX_Window_ReactivatedByApplicationUserId",
                table: "Window");

            migrationBuilder.DropIndex(
                name: "IX_Window_WaitingAreaId_BranchId",
                table: "Window");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Window_DeactivationAudit_Pair",
                table: "Window");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Window_ReactivationAudit_Pair",
                table: "Window");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_WaitingArea_Id_BranchId",
                table: "WaitingArea");

            migrationBuilder.DropIndex(
                name: "IX_WaitingArea_DeactivatedByApplicationUserId",
                table: "WaitingArea");

            migrationBuilder.DropIndex(
                name: "IX_WaitingArea_ReactivatedByApplicationUserId",
                table: "WaitingArea");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WaitingArea_DeactivationAudit_Pair",
                table: "WaitingArea");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WaitingArea_ReactivationAudit_Pair",
                table: "WaitingArea");

            migrationBuilder.DropIndex(
                name: "IX_Terminal_BranchId",
                table: "Terminal");

            migrationBuilder.DropIndex(
                name: "IX_Terminal_BranchId_IPAddress",
                table: "Terminal");

            migrationBuilder.DropIndex(
                name: "IX_Terminal_BranchId_SerialNo",
                table: "Terminal");

            migrationBuilder.DropIndex(
                name: "IX_Terminal_DeactivatedByApplicationUserId",
                table: "Terminal");

            migrationBuilder.DropIndex(
                name: "IX_Terminal_ReactivatedByApplicationUserId",
                table: "Terminal");

            migrationBuilder.DropIndex(
                name: "IX_Terminal_WindowId_BranchId",
                table: "Terminal");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Terminal_DeactivationAudit_Pair",
                table: "Terminal");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Terminal_ReactivationAudit_Pair",
                table: "Terminal");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Location_Address_Required",
                table: "Location");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Location_Area_Required",
                table: "Location");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Location_City_Required",
                table: "Location");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Location_Governorate_Required",
                table: "Location");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Location_Latitude_Range",
                table: "Location");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Location_Longitude_Range",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_DisplayWindow_BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropIndex(
                name: "IX_DisplayWindow_DisplayId_BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropIndex(
                name: "IX_DisplayWindow_WindowId_BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Display_Id_BranchId",
                table: "Display");

            migrationBuilder.DropIndex(
                name: "IX_Display_DeactivatedByApplicationUserId",
                table: "Display");

            migrationBuilder.DropIndex(
                name: "IX_Display_ReactivatedByApplicationUserId",
                table: "Display");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Display_DeactivationAudit_Pair",
                table: "Display");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Display_ReactivationAudit_Pair",
                table: "Display");

            migrationBuilder.DropIndex(
                name: "IX_Branch_DeactivatedByApplicationUserId",
                table: "Branch");

            migrationBuilder.DropIndex(
                name: "IX_Branch_ReactivatedByApplicationUserId",
                table: "Branch");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Branch_ArabicName_Required",
                table: "Branch");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Branch_DeactivationAudit_Pair",
                table: "Branch");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Branch_EnglishName_Required",
                table: "Branch");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Branch_ReactivationAudit_Pair",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "DeactivatedByApplicationUserId",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "DeactivatedOnUtc",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "ReactivatedByApplicationUserId",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "ReactivatedOnUtc",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Window");

            migrationBuilder.DropColumn(
                name: "DeactivatedByApplicationUserId",
                table: "WaitingArea");

            migrationBuilder.DropColumn(
                name: "DeactivatedOnUtc",
                table: "WaitingArea");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WaitingArea");

            migrationBuilder.DropColumn(
                name: "ReactivatedByApplicationUserId",
                table: "WaitingArea");

            migrationBuilder.DropColumn(
                name: "ReactivatedOnUtc",
                table: "WaitingArea");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WaitingArea");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "DeactivatedByApplicationUserId",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "DeactivatedOnUtc",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "ReactivatedByApplicationUserId",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "ReactivatedOnUtc",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "DisplayWindow");

            migrationBuilder.DropColumn(
                name: "DeactivatedByApplicationUserId",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "DeactivatedOnUtc",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "ReactivatedByApplicationUserId",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "ReactivatedOnUtc",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Display");

            migrationBuilder.DropColumn(
                name: "DeactivatedByApplicationUserId",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "DeactivatedOnUtc",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "ReactivatedByApplicationUserId",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "ReactivatedOnUtc",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Branch");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Branch");

            migrationBuilder.AddColumn<bool>(
                name: "IsUpdatesAvailable",
                table: "Branch",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Window",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Window",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredOnUtc",
                table: "Window",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Terminal",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Terminal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredOnUtc",
                table: "Terminal",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Longitude",
                table: "Location",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)");

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                table: "Location",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)");

            migrationBuilder.AlterColumn<string>(
                name: "Governorate",
                table: "Location",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Location",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                table: "Location",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Location",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByApplicationUserId",
                table: "Location",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByApplicationUserId",
                table: "Location",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedByApplicationUserId",
                table: "DisplayWindow",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Display",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Display",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredOnUtc",
                table: "Display",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "Branch",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ArabicName",
                table: "Branch",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Branch",
                type: "datetime2(3)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_IPAddress",
                table: "Terminal",
                column: "IPAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_SerialNo",
                table: "Terminal",
                column: "SerialNo");

            migrationBuilder.CreateIndex(
                name: "IX_Location_CreatedByApplicationUserId",
                table: "Location",
                column: "CreatedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LastModifiedByApplicationUserId",
                table: "Location",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayWindow_LastModifiedByApplicationUserId",
                table: "DisplayWindow",
                column: "LastModifiedByApplicationUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Branch_NameRequired",
                table: "Branch",
                sql: "NULLIF(LTRIM(RTRIM([ArabicName])), N'') IS NOT NULL OR NULLIF(LTRIM(RTRIM([EnglishName])), N'') IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_ApplicationUser_LastModifiedByApplicationUserId",
                table: "DisplayWindow",
                column: "LastModifiedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_Display_DisplayId",
                table: "DisplayWindow",
                column: "DisplayId",
                principalTable: "Display",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayWindow_Window_WindowId",
                table: "DisplayWindow",
                column: "WindowId",
                principalTable: "Window",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_ApplicationUser_CreatedByApplicationUserId",
                table: "Location",
                column: "CreatedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_ApplicationUser_LastModifiedByApplicationUserId",
                table: "Location",
                column: "LastModifiedByApplicationUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Terminal_Window_WindowId",
                table: "Terminal",
                column: "WindowId",
                principalTable: "Window",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Window_WaitingArea_WaitingAreaId",
                table: "Window",
                column: "WaitingAreaId",
                principalTable: "WaitingArea",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
