using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QControl.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketReservationRuntime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaximumTicketCallAttempts",
                table: "BranchConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "TicketArchiveRetentionDays",
                table: "BranchConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<int>(
                name: "TicketNoShowAutoCancellationMinutes",
                table: "BranchConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchConfiguration_ArchiveRetention_Positive",
                table: "BranchConfiguration",
                sql: "[TicketArchiveRetentionDays] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchConfiguration_MaxTicketCallAttempts_Positive",
                table: "BranchConfiguration",
                sql: "[MaximumTicketCallAttempts] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BranchConfiguration_NoShowTimeout_Positive",
                table: "BranchConfiguration",
                sql: "[TicketNoShowAutoCancellationMinutes] > 0");

            migrationBuilder.CreateTable(
                name: "BranchServiceSegmentDailyUsage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchServiceSegmentId = table.Column<int>(type: "int", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ConsumedCount = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchServiceSegmentDailyUsage", x => x.Id);
                    table.CheckConstraint("CK_BranchServiceSegmentDailyUsage_Consumed_NonNegative", "[ConsumedCount] >= 0");
                    table.ForeignKey(
                        name: "FK_BranchServiceSegmentDailyUsage_BranchServiceSegment_BranchServiceSegmentId",
                        column: x => x.BranchServiceSegmentId,
                        principalTable: "BranchServiceSegment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    SegmentId = table.Column<int>(type: "int", nullable: false),
                    BranchServiceSegmentId = table.Column<int>(type: "int", nullable: false),
                    ScheduledOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CancelledByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConvertedToTicketOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ExpiredOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.Id);
                    table.CheckConstraint("CK_Reservation_Status_Valid", "[Status] IN (1,2,3,4,5)");
                    table.ForeignKey(
                        name: "FK_Reservation_ApplicationUser_CancelledByApplicationUserId",
                        column: x => x.CancelledByApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservation_BranchServiceSegment_BranchServiceSegmentId",
                        column: x => x.BranchServiceSegmentId,
                        principalTable: "BranchServiceSegment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservation_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservation_Segment_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservation_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservationArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalReservationId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    SegmentId = table.Column<int>(type: "int", nullable: false),
                    BranchServiceSegmentId = table.Column<int>(type: "int", nullable: false),
                    ScheduledOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ConvertedToTicketOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ExpiredOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ArchivedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationArchive", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalTicketId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    IssuingServiceId = table.Column<int>(type: "int", nullable: false),
                    CurrentServiceId = table.Column<int>(type: "int", nullable: false),
                    SegmentId = table.Column<int>(type: "int", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    TicketNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentQueueEnteredOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CurrentServiceStartedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CancelledOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BoundWorkflowId = table.Column<int>(type: "int", nullable: true),
                    CurrentWorkflowStepOrder = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ArchivedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketArchive", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketNumberSequence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastIssuedNumber = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketNumberSequence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketNumberSequence_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketNumberSequence_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservationCustomInputValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    ServiceCustomInputId = table.Column<int>(type: "int", nullable: true),
                    NameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LabelEnSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LabelArSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TypeSnapshot = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationCustomInputValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationCustomInputValue_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservationCustomInputValue_ServiceCustomInput_ServiceCustomInputId",
                        column: x => x.ServiceCustomInputId,
                        principalTable: "ServiceCustomInput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReservationHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: true),
                    ToStatus = table.Column<int>(type: "int", nullable: true),
                    PerformerApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationHistory_ApplicationUser_PerformerApplicationUserId",
                        column: x => x.PerformerApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservationHistory_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    IssuingServiceId = table.Column<int>(type: "int", nullable: false),
                    CurrentServiceId = table.Column<int>(type: "int", nullable: false),
                    SegmentId = table.Column<int>(type: "int", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    TicketNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentWindowId = table.Column<int>(type: "int", nullable: true),
                    CurrentAssignedApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentQueueEnteredOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CurrentServiceStartedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CancelledOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BoundWorkflowId = table.Column<int>(type: "int", nullable: true),
                    CurrentWorkflowStepOrder = table.Column<int>(type: "int", nullable: true),
                    CurrentCallCycleNumber = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                    table.CheckConstraint("CK_Ticket_Number_NotBlank", "LEN(LTRIM(RTRIM([TicketNumber]))) > 0");
                    table.CheckConstraint("CK_Ticket_Status_Valid", "[Status] IN (1,2,3,4,5,6)");
                    table.ForeignKey(
                        name: "FK_Ticket_ApplicationUser_CurrentAssignedApplicationUserId",
                        column: x => x.CurrentAssignedApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Segment_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Service_CurrentServiceId",
                        column: x => x.CurrentServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Service_IssuingServiceId",
                        column: x => x.IssuingServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Window_CurrentWindowId",
                        column: x => x.CurrentWindowId,
                        principalTable: "Window",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservationCustomInputValueArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationArchiveId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceCustomInputId = table.Column<int>(type: "int", nullable: true),
                    NameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LabelEnSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LabelArSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TypeSnapshot = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationCustomInputValueArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationCustomInputValueArchive_ReservationArchive_ReservationArchiveId",
                        column: x => x.ReservationArchiveId,
                        principalTable: "ReservationArchive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservationHistoryArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationArchiveId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalHistoryId = table.Column<long>(type: "bigint", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: true),
                    ToStatus = table.Column<int>(type: "int", nullable: true),
                    PerformerApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationHistoryArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationHistoryArchive_ReservationArchive_ReservationArchiveId",
                        column: x => x.ReservationArchiveId,
                        principalTable: "ReservationArchive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketCallAttemptArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketArchiveId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalCallAttemptId = table.Column<long>(type: "bigint", nullable: false),
                    CallCycleNumber = table.Column<int>(type: "int", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    WindowId = table.Column<int>(type: "int", nullable: false),
                    PerformerApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CalledOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCallAttemptArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCallAttemptArchive_TicketArchive_TicketArchiveId",
                        column: x => x.TicketArchiveId,
                        principalTable: "TicketArchive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketCustomInputValueArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketArchiveId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceCustomInputId = table.Column<int>(type: "int", nullable: true),
                    NameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LabelEnSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LabelArSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TypeSnapshot = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCustomInputValueArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCustomInputValueArchive_TicketArchive_TicketArchiveId",
                        column: x => x.TicketArchiveId,
                        principalTable: "TicketArchive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketHistoryArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketArchiveId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalHistoryId = table.Column<long>(type: "bigint", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: true),
                    ToStatus = table.Column<int>(type: "int", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    FromServiceId = table.Column<int>(type: "int", nullable: true),
                    ToServiceId = table.Column<int>(type: "int", nullable: true),
                    WindowId = table.Column<int>(type: "int", nullable: true),
                    PerformerApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistoryArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketHistoryArchive_TicketArchive_TicketArchiveId",
                        column: x => x.TicketArchiveId,
                        principalTable: "TicketArchive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketServiceJourneyArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketArchiveId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalJourneyId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    EntryType = table.Column<int>(type: "int", nullable: false),
                    WorkflowStepOrder = table.Column<int>(type: "int", nullable: true),
                    EnteredWaitingOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ServiceStartedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ServiceEndedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    Outcome = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketServiceJourneyArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketServiceJourneyArchive_TicketArchive_TicketArchiveId",
                        column: x => x.TicketArchiveId,
                        principalTable: "TicketArchive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketWorkflowStepSnapshotArchive",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketArchiveId = table.Column<long>(type: "bigint", nullable: false),
                    SourceWorkflowId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketWorkflowStepSnapshotArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketWorkflowStepSnapshotArchive_TicketArchive_TicketArchiveId",
                        column: x => x.TicketArchiveId,
                        principalTable: "TicketArchive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketCallAttempt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    CallCycleNumber = table.Column<int>(type: "int", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    WindowId = table.Column<int>(type: "int", nullable: false),
                    PerformerApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CalledOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCallAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCallAttempt_ApplicationUser_PerformerApplicationUserId",
                        column: x => x.PerformerApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketCallAttempt_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketCallAttempt_Window_WindowId",
                        column: x => x.WindowId,
                        principalTable: "Window",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketCustomInputValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    ServiceCustomInputId = table.Column<int>(type: "int", nullable: true),
                    NameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LabelEnSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LabelArSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TypeSnapshot = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCustomInputValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCustomInputValue_ServiceCustomInput_ServiceCustomInputId",
                        column: x => x.ServiceCustomInputId,
                        principalTable: "ServiceCustomInput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TicketCustomInputValue_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: true),
                    ToStatus = table.Column<int>(type: "int", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    FromServiceId = table.Column<int>(type: "int", nullable: true),
                    ToServiceId = table.Column<int>(type: "int", nullable: true),
                    WindowId = table.Column<int>(type: "int", nullable: true),
                    PerformerApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketHistory_ApplicationUser_PerformerApplicationUserId",
                        column: x => x.PerformerApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketHistory_Service_FromServiceId",
                        column: x => x.FromServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketHistory_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketHistory_Service_ToServiceId",
                        column: x => x.ToServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketHistory_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketHistory_Window_WindowId",
                        column: x => x.WindowId,
                        principalTable: "Window",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketServiceJourney",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    EntryType = table.Column<int>(type: "int", nullable: false),
                    WorkflowStepOrder = table.Column<int>(type: "int", nullable: true),
                    EnteredWaitingOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ServiceStartedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ServiceEndedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    Outcome = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketServiceJourney", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketServiceJourney_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketServiceJourney_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketWorkflowStepSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    SourceWorkflowId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketWorkflowStepSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketWorkflowStepSnapshot_ServiceWorkflows_SourceWorkflowId",
                        column: x => x.SourceWorkflowId,
                        principalTable: "ServiceWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketWorkflowStepSnapshot_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketWorkflowStepSnapshot_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchServiceSegmentDailyUsage_BranchServiceSegmentId_BusinessDate",
                table: "BranchServiceSegmentDailyUsage",
                columns: new[] { "BranchServiceSegmentId", "BusinessDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_BranchId_ScheduledOnUtc",
                table: "Reservation",
                columns: new[] { "BranchId", "ScheduledOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_BranchId_ServiceId_Status",
                table: "Reservation",
                columns: new[] { "BranchId", "ServiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_BranchServiceSegmentId",
                table: "Reservation",
                column: "BranchServiceSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_CancelledByApplicationUserId",
                table: "Reservation",
                column: "CancelledByApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_SegmentId",
                table: "Reservation",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ServiceId",
                table: "Reservation",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_Status",
                table: "Reservation",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationArchive_BranchId_BusinessDate",
                table: "ReservationArchive",
                columns: new[] { "BranchId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationArchive_BranchId_ServiceId_Status",
                table: "ReservationArchive",
                columns: new[] { "BranchId", "ServiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationArchive_OriginalReservationId",
                table: "ReservationArchive",
                column: "OriginalReservationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationArchive_SegmentId",
                table: "ReservationArchive",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCustomInputValue_ReservationId",
                table: "ReservationCustomInputValue",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCustomInputValue_ServiceCustomInputId",
                table: "ReservationCustomInputValue",
                column: "ServiceCustomInputId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCustomInputValueArchive_ReservationArchiveId",
                table: "ReservationCustomInputValueArchive",
                column: "ReservationArchiveId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationHistory_PerformerApplicationUserId",
                table: "ReservationHistory",
                column: "PerformerApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationHistory_ReservationId_OccurredOnUtc",
                table: "ReservationHistory",
                columns: new[] { "ReservationId", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationHistoryArchive_ReservationArchiveId_OccurredOnUtc",
                table: "ReservationHistoryArchive",
                columns: new[] { "ReservationArchiveId", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_BranchId_BusinessDate",
                table: "Ticket",
                columns: new[] { "BranchId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_BranchId_Status",
                table: "Ticket",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CurrentAssignedApplicationUserId",
                table: "Ticket",
                column: "CurrentAssignedApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CurrentServiceId",
                table: "Ticket",
                column: "CurrentServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CurrentWindowId",
                table: "Ticket",
                column: "CurrentWindowId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_IssuingServiceId",
                table: "Ticket",
                column: "IssuingServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_QueueRead",
                table: "Ticket",
                columns: new[] { "BranchId", "CurrentServiceId", "Status", "CurrentQueueEnteredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_SegmentId",
                table: "Ticket",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TicketNumber",
                table: "Ticket",
                column: "TicketNumber");

            migrationBuilder.CreateIndex(
                name: "UX_Ticket_Branch_BusinessDate_IssuingService_Number",
                table: "Ticket",
                columns: new[] { "BranchId", "BusinessDate", "IssuingServiceId", "TicketNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Ticket_ReservationId_NotNull",
                table: "Ticket",
                column: "ReservationId",
                unique: true,
                filter: "[ReservationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TicketArchive_BranchId_BusinessDate",
                table: "TicketArchive",
                columns: new[] { "BranchId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketArchive_BranchId_IssuingServiceId",
                table: "TicketArchive",
                columns: new[] { "BranchId", "IssuingServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketArchive_BranchId_Status",
                table: "TicketArchive",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketArchive_CurrentServiceId",
                table: "TicketArchive",
                column: "CurrentServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketArchive_OriginalTicketId",
                table: "TicketArchive",
                column: "OriginalTicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketArchive_SegmentId",
                table: "TicketArchive",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketArchive_TicketNumber",
                table: "TicketArchive",
                column: "TicketNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCallAttempt_PerformerApplicationUserId",
                table: "TicketCallAttempt",
                column: "PerformerApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCallAttempt_TicketId_CallCycleNumber_AttemptNumber",
                table: "TicketCallAttempt",
                columns: new[] { "TicketId", "CallCycleNumber", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketCallAttempt_TicketId_CalledOnUtc",
                table: "TicketCallAttempt",
                columns: new[] { "TicketId", "CalledOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketCallAttempt_WindowId",
                table: "TicketCallAttempt",
                column: "WindowId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCallAttemptArchive_TicketArchiveId_CallCycleNumber_AttemptNumber",
                table: "TicketCallAttemptArchive",
                columns: new[] { "TicketArchiveId", "CallCycleNumber", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketCustomInputValue_ServiceCustomInputId",
                table: "TicketCustomInputValue",
                column: "ServiceCustomInputId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCustomInputValue_TicketId",
                table: "TicketCustomInputValue",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCustomInputValueArchive_TicketArchiveId",
                table: "TicketCustomInputValueArchive",
                column: "TicketArchiveId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_FromServiceId",
                table: "TicketHistory",
                column: "FromServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_PerformerApplicationUserId",
                table: "TicketHistory",
                column: "PerformerApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_ServiceId",
                table: "TicketHistory",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_TicketId_EventType",
                table: "TicketHistory",
                columns: new[] { "TicketId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_TicketId_OccurredOnUtc",
                table: "TicketHistory",
                columns: new[] { "TicketId", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_ToServiceId",
                table: "TicketHistory",
                column: "ToServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_WindowId",
                table: "TicketHistory",
                column: "WindowId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistoryArchive_FromServiceId",
                table: "TicketHistoryArchive",
                column: "FromServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistoryArchive_ServiceId",
                table: "TicketHistoryArchive",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistoryArchive_TicketArchiveId_OccurredOnUtc",
                table: "TicketHistoryArchive",
                columns: new[] { "TicketArchiveId", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistoryArchive_ToServiceId",
                table: "TicketHistoryArchive",
                column: "ToServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketNumberSequence_BranchId_ServiceId_BusinessDate",
                table: "TicketNumberSequence",
                columns: new[] { "BranchId", "ServiceId", "BusinessDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketNumberSequence_ServiceId",
                table: "TicketNumberSequence",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketServiceJourney_ServiceId",
                table: "TicketServiceJourney",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketServiceJourney_TicketId",
                table: "TicketServiceJourney",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketServiceJourney_TicketId_Id",
                table: "TicketServiceJourney",
                columns: new[] { "TicketId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketServiceJourney_TicketId_ServiceEndedOnUtc",
                table: "TicketServiceJourney",
                columns: new[] { "TicketId", "ServiceEndedOnUtc" },
                filter: "[ServiceEndedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TicketServiceJourneyArchive_ServiceId",
                table: "TicketServiceJourneyArchive",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketServiceJourneyArchive_TicketArchiveId_OriginalJourneyId",
                table: "TicketServiceJourneyArchive",
                columns: new[] { "TicketArchiveId", "OriginalJourneyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketWorkflowStepSnapshot_ServiceId",
                table: "TicketWorkflowStepSnapshot",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketWorkflowStepSnapshot_SourceWorkflowId",
                table: "TicketWorkflowStepSnapshot",
                column: "SourceWorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketWorkflowStepSnapshot_TicketId",
                table: "TicketWorkflowStepSnapshot",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketWorkflowStepSnapshot_TicketId_StepOrder",
                table: "TicketWorkflowStepSnapshot",
                columns: new[] { "TicketId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketWorkflowStepSnapshotArchive_ServiceId",
                table: "TicketWorkflowStepSnapshotArchive",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketWorkflowStepSnapshotArchive_TicketArchiveId_StepOrder",
                table: "TicketWorkflowStepSnapshotArchive",
                columns: new[] { "TicketArchiveId", "StepOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchConfiguration_ArchiveRetention_Positive",
                table: "BranchConfiguration");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchConfiguration_MaxTicketCallAttempts_Positive",
                table: "BranchConfiguration");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BranchConfiguration_NoShowTimeout_Positive",
                table: "BranchConfiguration");

            migrationBuilder.DropTable(
                name: "BranchServiceSegmentDailyUsage");

            migrationBuilder.DropTable(
                name: "ReservationCustomInputValue");

            migrationBuilder.DropTable(
                name: "ReservationCustomInputValueArchive");

            migrationBuilder.DropTable(
                name: "ReservationHistory");

            migrationBuilder.DropTable(
                name: "ReservationHistoryArchive");

            migrationBuilder.DropTable(
                name: "TicketCallAttempt");

            migrationBuilder.DropTable(
                name: "TicketCallAttemptArchive");

            migrationBuilder.DropTable(
                name: "TicketCustomInputValue");

            migrationBuilder.DropTable(
                name: "TicketCustomInputValueArchive");

            migrationBuilder.DropTable(
                name: "TicketHistory");

            migrationBuilder.DropTable(
                name: "TicketHistoryArchive");

            migrationBuilder.DropTable(
                name: "TicketNumberSequence");

            migrationBuilder.DropTable(
                name: "TicketServiceJourney");

            migrationBuilder.DropTable(
                name: "TicketServiceJourneyArchive");

            migrationBuilder.DropTable(
                name: "TicketWorkflowStepSnapshot");

            migrationBuilder.DropTable(
                name: "TicketWorkflowStepSnapshotArchive");

            migrationBuilder.DropTable(
                name: "ReservationArchive");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "TicketArchive");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropColumn(
                name: "MaximumTicketCallAttempts",
                table: "BranchConfiguration");

            migrationBuilder.DropColumn(
                name: "TicketArchiveRetentionDays",
                table: "BranchConfiguration");

            migrationBuilder.DropColumn(
                name: "TicketNoShowAutoCancellationMinutes",
                table: "BranchConfiguration");
        }
    }
}
