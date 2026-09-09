using QControl.Domain.Enums;

namespace QControl.Application.Features.TicketRuntime.Shared;

public sealed record CustomInputSubmission(int ServiceCustomInputId, string Value);

public sealed record TicketListItemResponse(
    int Id,
    string TicketNumber,
    DateOnly BusinessDate,
    TicketStatus Status,
    int IssuingServiceId,
    int CurrentServiceId,
    int SegmentId,
    DateTime CurrentQueueEnteredOnUtc);

public sealed record TicketDetailsResponse(
    int Id,
    int BranchId,
    string TicketNumber,
    DateOnly BusinessDate,
    TicketStatus Status,
    int IssuingServiceId,
    int CurrentServiceId,
    int SegmentId,
    int? ReservationId,
    int? CurrentWindowId,
    DateTime CurrentQueueEnteredOnUtc,
    DateTime? CurrentServiceStartedOnUtc,
    DateTime? CompletedOnUtc,
    DateTime? CancelledOnUtc,
    string? CancellationReason,
    int? BoundWorkflowId,
    int? CurrentWorkflowStepOrder,
    IReadOnlyList<CustomInputValueResponse> CustomInputs,
    IReadOnlyList<TicketJourneyResponse> Journeys,
    string? RowVersion = null,
    bool IsArchived = false,
    IReadOnlyList<TicketHistoryResponse>? History = null,
    IReadOnlyList<TicketCallAttemptResponse>? CallAttempts = null,
    string? Field = null,
    TicketPrintModelResponse? Print = null);

public sealed record TicketPrintModelResponse(
    decimal TicketWidthMm,
    decimal TicketHeightMm,
    IReadOnlyList<TicketPrintRuntimeElementResponse> Elements);

public sealed record TicketPrintRuntimeElementResponse(
    TicketPrintElementType ElementType,
    bool IsVisible,
    decimal? XMm,
    decimal? YMm,
    decimal? WidthMm,
    decimal? HeightMm,
    decimal? FontSizePt,
    TicketFontWeight? FontWeight,
    TicketTextAlign? TextAlign,
    TicketPrintLanguage? Language,
    string? Text,
    string? ImageUrl,
    TicketPrintOverflowBehavior? OverflowBehavior);

public sealed record TicketJourneyResponse(
    int ServiceId,
    TicketJourneyEntryType EntryType,
    int? WorkflowStepOrder,
    DateTime EnteredWaitingOnUtc,
    DateTime? ServiceStartedOnUtc,
    DateTime? ServiceEndedOnUtc,
    TicketJourneyOutcome? Outcome);

public sealed record TicketHistoryResponse(
    long Id,
    TicketHistoryEventType EventType,
    TicketStatus? FromStatus,
    TicketStatus? ToStatus,
    int? ServiceId,
    int? FromServiceId,
    int? ToServiceId,
    int? WindowId,
    Guid? PerformerApplicationUserId,
    string? Reason,
    DateTime OccurredOnUtc);

public sealed record TicketCallAttemptResponse(
    int CallCycleNumber,
    int AttemptNumber,
    int WindowId,
    Guid? PerformerApplicationUserId,
    DateTime CalledOnUtc);

public sealed record CustomInputValueResponse(
    int? ServiceCustomInputId,
    string Name,
    string? LabelEn,
    string? LabelAr,
    ServiceCustomInputType Type,
    string Value,
    int? OrderSnapshot = null);

public sealed record ReservationListItemResponse(
    int Id,
    DateTime ScheduledOnUtc,
    ReservationStatus Status,
    int ServiceId,
    int SegmentId);

public sealed record ReservationDetailsResponse(
    int Id,
    int BranchId,
    int ServiceId,
    int SegmentId,
    DateTime ScheduledOnUtc,
    DateOnly BusinessDate,
    ReservationStatus Status,
    string? CancellationReason,
    DateTime? CancelledOnUtc,
    DateTime? ConvertedToTicketOnUtc,
    DateTime? ExpiredOnUtc,
    IReadOnlyList<CustomInputValueResponse> CustomInputs,
    string? RowVersion = null,
    bool IsArchived = false,
    IReadOnlyList<ReservationHistoryResponse>? History = null,
    string? Field = null);

public sealed record KioskReservationSearchItemResponse(
    int ReservationId,
    int ServiceId,
    int SegmentId,
    DateTime ScheduledOnUtc,
    DateOnly BusinessDate,
    ReservationStatus Status);

public sealed record ReservationHistoryResponse(
    long Id,
    ReservationHistoryEventType EventType,
    ReservationStatus? FromStatus,
    ReservationStatus? ToStatus,
    Guid? PerformerApplicationUserId,
    string? Reason,
    DateTime OccurredOnUtc);
