using QControl.Domain.Enums;

namespace QControl.infrastructure.Persistence;

internal sealed class TicketArchive
{
    public long Id { get; set; }
    public int OriginalTicketId { get; set; }
    public int BranchId { get; set; }
    public int IssuingServiceId { get; set; }
    public int CurrentServiceId { get; set; }
    public int SegmentId { get; set; }
    public int? ReservationId { get; set; }
    public string? LookupValue { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }
    public TicketStatus Status { get; set; }
    public DateTime CurrentQueueEnteredOnUtc { get; set; }
    public DateTime? CurrentServiceStartedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? CancelledOnUtc { get; set; }
    public string? CancellationReason { get; set; }
    public int? BoundWorkflowId { get; set; }
    public int? CurrentWorkflowStepOrder { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ArchivedOnUtc { get; set; }
    public List<TicketServiceJourneyArchive> Journeys { get; set; } = new();
    public List<TicketHistoryArchive> History { get; set; } = new();
    public List<TicketCallAttemptArchive> CallAttempts { get; set; } = new();
    public List<TicketCustomInputValueArchive> CustomInputs { get; set; } = new();
    public List<TicketWorkflowStepSnapshotArchive> WorkflowSteps { get; set; } = new();
}

internal sealed class TicketServiceJourneyArchive
{
    public long Id { get; set; }
    public long TicketArchiveId { get; set; }
    public TicketArchive TicketArchive { get; set; } = null!;
    public int OriginalJourneyId { get; set; }
    public int ServiceId { get; set; }
    public TicketJourneyEntryType EntryType { get; set; }
    public int? WorkflowStepOrder { get; set; }
    public DateTime EnteredWaitingOnUtc { get; set; }
    public DateTime? ServiceStartedOnUtc { get; set; }
    public DateTime? ServiceEndedOnUtc { get; set; }
    public TicketJourneyOutcome? Outcome { get; set; }
}

internal sealed class TicketHistoryArchive
{
    public long Id { get; set; }
    public long TicketArchiveId { get; set; }
    public TicketArchive TicketArchive { get; set; } = null!;
    public long? OriginalHistoryId { get; set; }
    public TicketHistoryEventType EventType { get; set; }
    public TicketStatus? FromStatus { get; set; }
    public TicketStatus? ToStatus { get; set; }
    public int? ServiceId { get; set; }
    public int? FromServiceId { get; set; }
    public int? ToServiceId { get; set; }
    public int? WindowId { get; set; }
    public Guid? PerformerApplicationUserId { get; set; }
    public string? Reason { get; set; }
    public DateTime OccurredOnUtc { get; set; }
}

internal sealed class TicketCallAttemptArchive
{
    public long Id { get; set; }
    public long TicketArchiveId { get; set; }
    public TicketArchive TicketArchive { get; set; } = null!;
    public long OriginalCallAttemptId { get; set; }
    public int CallCycleNumber { get; set; }
    public int AttemptNumber { get; set; }
    public int WindowId { get; set; }
    public Guid? PerformerApplicationUserId { get; set; }
    public DateTime CalledOnUtc { get; set; }
}

internal sealed class TicketCustomInputValueArchive
{
    public long Id { get; set; }
    public long TicketArchiveId { get; set; }
    public TicketArchive TicketArchive { get; set; } = null!;
    public int? ServiceCustomInputId { get; set; }
    public string NameSnapshot { get; set; } = string.Empty;
    public string? LabelEnSnapshot { get; set; }
    public string? LabelArSnapshot { get; set; }
    public ServiceCustomInputType TypeSnapshot { get; set; }
    public string Value { get; set; } = string.Empty;
}

internal sealed class TicketWorkflowStepSnapshotArchive
{
    public long Id { get; set; }
    public long TicketArchiveId { get; set; }
    public TicketArchive TicketArchive { get; set; } = null!;
    public int SourceWorkflowId { get; set; }
    public int ServiceId { get; set; }
    public int StepOrder { get; set; }
}

internal sealed class ReservationArchive
{
    public long Id { get; set; }
    public int OriginalReservationId { get; set; }
    public int BranchId { get; set; }
    public int ServiceId { get; set; }
    public int SegmentId { get; set; }
    public int BranchServiceSegmentId { get; set; }
    public string? LookupValue { get; set; }
    public DateTime ScheduledOnUtc { get; set; }
    public DateOnly BusinessDate { get; set; }
    public ReservationStatus Status { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledOnUtc { get; set; }
    public DateTime? ConvertedToTicketOnUtc { get; set; }
    public DateTime? ExpiredOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ArchivedOnUtc { get; set; }
    public List<ReservationHistoryArchive> History { get; set; } = new();
    public List<ReservationCustomInputValueArchive> CustomInputs { get; set; } = new();
}

internal sealed class ReservationHistoryArchive
{
    public long Id { get; set; }
    public long ReservationArchiveId { get; set; }
    public ReservationArchive ReservationArchive { get; set; } = null!;
    public long? OriginalHistoryId { get; set; }
    public ReservationHistoryEventType EventType { get; set; }
    public ReservationStatus? FromStatus { get; set; }
    public ReservationStatus? ToStatus { get; set; }
    public Guid? PerformerApplicationUserId { get; set; }
    public string? Reason { get; set; }
    public DateTime OccurredOnUtc { get; set; }
}

internal sealed class ReservationCustomInputValueArchive
{
    public long Id { get; set; }
    public long ReservationArchiveId { get; set; }
    public ReservationArchive ReservationArchive { get; set; } = null!;
    public int? ServiceCustomInputId { get; set; }
    public string NameSnapshot { get; set; } = string.Empty;
    public string? LabelEnSnapshot { get; set; }
    public string? LabelArSnapshot { get; set; }
    public ServiceCustomInputType TypeSnapshot { get; set; }
    public string Value { get; set; } = string.Empty;
}
