using BuildingBlock.Domain.EntitiesHelper;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class TicketServiceJourney : Entity<int>
{
    private TicketServiceJourney() { }
    public int TicketId { get; private set; }
    public Ticket Ticket { get; private set; } = null!;
    public int ServiceId { get; private set; }
    public Service Service { get; private set; } = null!;
    public TicketJourneyEntryType EntryType { get; private set; }
    public int? WorkflowStepOrder { get; private set; }
    public DateTime EnteredWaitingOnUtc { get; private set; }
    public DateTime? ServiceStartedOnUtc { get; private set; }
    public DateTime? ServiceEndedOnUtc { get; private set; }
    public TicketJourneyOutcome? Outcome { get; private set; }

    public static TicketServiceJourney Create(int serviceId, TicketJourneyEntryType entryType,
        int? workflowStepOrder, DateTime enteredOnUtc) => new()
        {
            ServiceId = serviceId,
            EntryType = entryType,
            WorkflowStepOrder = workflowStepOrder,
            EnteredWaitingOnUtc = enteredOnUtc,
            CreatedOnUtc = enteredOnUtc
        };

    internal void SetWorkflowStep(int? stepOrder) => WorkflowStepOrder = stepOrder;
    internal void StartService(DateTime occurredOnUtc) => ServiceStartedOnUtc = occurredOnUtc;
    internal void End(TicketJourneyOutcome outcome, DateTime occurredOnUtc)
    {
        Outcome = outcome;
        ServiceEndedOnUtc = occurredOnUtc;
    }
}

public sealed class TicketHistory : Entity<long>
{
    private TicketHistory() { }
    public int TicketId { get; private set; }
    public Ticket Ticket { get; private set; } = null!;
    public TicketHistoryEventType EventType { get; private set; }
    public TicketStatus? FromStatus { get; private set; }
    public TicketStatus? ToStatus { get; private set; }
    public int? ServiceId { get; private set; }
    public int? FromServiceId { get; private set; }
    public int? ToServiceId { get; private set; }
    public int? WindowId { get; private set; }
    public Guid? PerformerApplicationUserId { get; private set; }
    public string? Reason { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }

    public static TicketHistory Create(TicketHistoryEventType eventType, TicketStatus? fromStatus,
        TicketStatus? toStatus, int? serviceId, int? fromServiceId, int? toServiceId,
        int? windowId, Guid? performerId, string? reason, DateTime occurredOnUtc) => new()
        {
            EventType = eventType,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ServiceId = serviceId,
            FromServiceId = fromServiceId,
            ToServiceId = toServiceId,
            WindowId = windowId,
            PerformerApplicationUserId = performerId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            OccurredOnUtc = occurredOnUtc,
            CreatedOnUtc = occurredOnUtc
        };
}

public sealed class TicketCallAttempt : Entity<long>
{
    private TicketCallAttempt() { }
    public int TicketId { get; private set; }
    public Ticket Ticket { get; private set; } = null!;
    public int CallCycleNumber { get; private set; }
    public int AttemptNumber { get; private set; }
    public int WindowId { get; private set; }
    public Window Window { get; private set; } = null!;
    public Guid? PerformerApplicationUserId { get; private set; }
    public DateTime CalledOnUtc { get; private set; }

    public static TicketCallAttempt Create(int callCycleNumber, int attemptNumber,
        int windowId, Guid? performerId, DateTime calledOnUtc) => new()
        {
            CallCycleNumber = callCycleNumber,
            AttemptNumber = attemptNumber,
            WindowId = windowId,
            PerformerApplicationUserId = performerId,
            CalledOnUtc = calledOnUtc,
            CreatedOnUtc = calledOnUtc
        };
}

public sealed class TicketWorkflowStepSnapshot : Entity<int>
{
    private TicketWorkflowStepSnapshot() { }
    public int TicketId { get; private set; }
    public Ticket Ticket { get; private set; } = null!;
    public int SourceWorkflowId { get; private set; }
    public int ServiceId { get; private set; }
    public int StepOrder { get; private set; }

    public static TicketWorkflowStepSnapshot Create(int sourceWorkflowId, int serviceId,
        int stepOrder, DateTime capturedOnUtc) => new()
    {
        SourceWorkflowId = sourceWorkflowId,
        ServiceId = serviceId,
        StepOrder = stepOrder,
        CreatedOnUtc = capturedOnUtc
    };
}

public sealed class TicketCustomInputValue : Entity<int>
{
    private TicketCustomInputValue() { }
    public int TicketId { get; private set; }
    public Ticket Ticket { get; private set; } = null!;
    public int? ServiceCustomInputId { get; private set; }
    public string NameSnapshot { get; private set; } = string.Empty;
    public string? LabelEnSnapshot { get; private set; }
    public string? LabelArSnapshot { get; private set; }
    public int? OrderSnapshot { get; private set; }
    public ServiceCustomInputType TypeSnapshot { get; private set; }
    public string Value { get; private set; } = string.Empty;

    public static TicketCustomInputValue Create(int? inputId, string name, string? labelEn,
        string? labelAr, ServiceCustomInputType type, string value, DateTime createdOnUtc,
        int? orderSnapshot = null) => new()
        {
            ServiceCustomInputId = inputId,
            NameSnapshot = name,
            LabelEnSnapshot = labelEn,
            LabelArSnapshot = labelAr,
            OrderSnapshot = orderSnapshot,
            TypeSnapshot = type,
            Value = value,
            CreatedOnUtc = createdOnUtc
        };
}
