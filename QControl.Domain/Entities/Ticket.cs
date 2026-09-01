using BuildingBlock.Domain.EntitiesHelper;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class Ticket : AggregateRoot<int>
{
    private readonly List<TicketServiceJourney> _serviceJourneys = new();
    private readonly List<TicketHistory> _history = new();
    private readonly List<TicketCallAttempt> _callAttempts = new();
    private readonly List<TicketCustomInputValue> _customInputValues = new();
    private readonly List<TicketWorkflowStepSnapshot> _workflowSteps = new();

    private Ticket() { }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;
    public int IssuingServiceId { get; private set; }
    public Service IssuingService { get; private set; } = null!;
    public int CurrentServiceId { get; private set; }
    public Service CurrentService { get; private set; } = null!;
    public int SegmentId { get; private set; }
    public Segment Segment { get; private set; } = null!;
    public int? ReservationId { get; private set; }
    public Reservation? Reservation { get; private set; }
    public string TicketNumber { get; private set; } = string.Empty;
    public DateOnly BusinessDate { get; private set; }
    public TicketStatus Status { get; private set; }
    public int? CurrentWindowId { get; private set; }
    public Window? CurrentWindow { get; private set; }
    public Guid? CurrentAssignedApplicationUserId { get; private set; }
    public DateTime CurrentQueueEnteredOnUtc { get; private set; }
    public DateTime? CurrentServiceStartedOnUtc { get; private set; }
    public DateTime? CompletedOnUtc { get; private set; }
    public DateTime? CancelledOnUtc { get; private set; }
    public string? CancellationReason { get; private set; }
    public int? BoundWorkflowId { get; private set; }
    public int? CurrentWorkflowStepOrder { get; private set; }
    public int CurrentCallCycleNumber { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public IReadOnlyCollection<TicketServiceJourney> ServiceJourneys => _serviceJourneys.AsReadOnly();
    public IReadOnlyCollection<TicketHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<TicketCallAttempt> CallAttempts => _callAttempts.AsReadOnly();
    public IReadOnlyCollection<TicketCustomInputValue> CustomInputValues => _customInputValues.AsReadOnly();
    public IReadOnlyCollection<TicketWorkflowStepSnapshot> WorkflowSteps => _workflowSteps.AsReadOnly();

    public static Ticket Create(
        int branchId,
        int serviceId,
        int segmentId,
        int? reservationId,
        string ticketNumber,
        DateOnly businessDate,
        DateTime createdOnUtc,
        Guid performerId)
    {
        var ticket = new Ticket
        {
            BranchId = branchId,
            IssuingServiceId = serviceId,
            CurrentServiceId = serviceId,
            SegmentId = segmentId,
            ReservationId = reservationId,
            TicketNumber = ticketNumber.Trim(),
            BusinessDate = businessDate,
            Status = TicketStatus.Waiting,
            CurrentQueueEnteredOnUtc = createdOnUtc,
            CreatedOnUtc = createdOnUtc
        };

        ticket._serviceJourneys.Add(TicketServiceJourney.Create(
            serviceId,
            TicketJourneyEntryType.Initial,
            workflowStepOrder: null,
            createdOnUtc));
        ticket.AppendHistory(TicketHistoryEventType.Created, null, TicketStatus.Waiting,
            serviceId, null, null, null, performerId, null, createdOnUtc);
        return ticket;
    }

    public void AddCustomInput(TicketCustomInputValue value) =>
        _customInputValues.Add(value ?? throw new ArgumentNullException(nameof(value)));

    public void BindWorkflow(int workflowId, IEnumerable<(int ServiceId, int StepOrder)> steps,
        DateTime capturedOnUtc)
    {
        if (BoundWorkflowId.HasValue)
        {
            return;
        }

        var ordered = steps.OrderBy(x => x.StepOrder).ToArray();
        BoundWorkflowId = workflowId;
        CurrentWorkflowStepOrder = ordered.FirstOrDefault(x => x.ServiceId == CurrentServiceId).StepOrder;
        foreach (var step in ordered)
        {
            _workflowSteps.Add(TicketWorkflowStepSnapshot.Create(workflowId, step.ServiceId,
                step.StepOrder, capturedOnUtc));
        }

        var currentJourney = _serviceJourneys.Single(x => x.ServiceEndedOnUtc == null);
        currentJourney.SetWorkflowStep(CurrentWorkflowStepOrder);
    }

    public bool TryCancel(string reason, Guid? performerId, DateTime occurredOnUtc)
    {
        if (string.IsNullOrWhiteSpace(reason) ||
            Status is not (TicketStatus.Waiting or TicketStatus.NoShow))
        {
            return false;
        }

        var from = Status;
        Status = TicketStatus.Cancelled;
        CancelledOnUtc = occurredOnUtc;
        CancellationReason = reason.Trim();
        ReleaseAssignment();
        ModifiedOnUtc = occurredOnUtc;
        AppendHistory(TicketHistoryEventType.Cancelled, from, Status, CurrentServiceId,
            null, null, null, performerId, CancellationReason, occurredOnUtc);
        return true;
    }

    public bool TryStartService(Guid performerId, DateTime occurredOnUtc)
    {
        if (Status != TicketStatus.Called)
        {
            return false;
        }

        Status = TicketStatus.InProgress;
        CurrentServiceStartedOnUtc = occurredOnUtc;
        _serviceJourneys.Single(x => x.ServiceEndedOnUtc == null).StartService(occurredOnUtc);
        ModifiedOnUtc = occurredOnUtc;
        AppendHistory(TicketHistoryEventType.ServiceStarted, TicketStatus.Called, Status,
            CurrentServiceId, null, null, CurrentWindowId, performerId, null, occurredOnUtc);
        return true;
    }

    public bool TryReturnNoShowToWaiting(Guid performerId, DateTime occurredOnUtc)
    {
        if (Status != TicketStatus.NoShow)
        {
            return false;
        }

        Status = TicketStatus.Waiting;
        CurrentQueueEnteredOnUtc = occurredOnUtc;
        CurrentServiceStartedOnUtc = null;
        ReleaseAssignment();
        ModifiedOnUtc = occurredOnUtc;
        AppendHistory(TicketHistoryEventType.ReturnedToWaiting, TicketStatus.NoShow, Status,
            CurrentServiceId, null, null, null, performerId, null, occurredOnUtc);
        return true;
    }

    public bool TryRecordCallAttempt(
        int windowId,
        Guid performerId,
        int maximumAttempts,
        DateTime occurredOnUtc)
    {
        if (Status == TicketStatus.Waiting)
        {
            Status = TicketStatus.Called;
            CurrentCallCycleNumber++;
            CurrentWindowId = windowId;
            CurrentAssignedApplicationUserId = performerId;
            AppendHistory(TicketHistoryEventType.Called, TicketStatus.Waiting, TicketStatus.Called,
                CurrentServiceId, null, null, windowId, performerId, null, occurredOnUtc);
        }
        else if (Status != TicketStatus.Called || CurrentWindowId != windowId ||
                 CurrentAssignedApplicationUserId != performerId)
        {
            return false;
        }

        var attemptNumber = _callAttempts.Count(x => x.CallCycleNumber == CurrentCallCycleNumber) + 1;
        _callAttempts.Add(TicketCallAttempt.Create(
            CurrentCallCycleNumber, attemptNumber, windowId, performerId, occurredOnUtc));
        AppendHistory(TicketHistoryEventType.CallAttempt, TicketStatus.Called, TicketStatus.Called,
            CurrentServiceId, null, null, windowId, performerId, null, occurredOnUtc);

        if (attemptNumber >= maximumAttempts)
        {
            Status = TicketStatus.NoShow;
            AppendHistory(TicketHistoryEventType.NoShow, TicketStatus.Called, TicketStatus.NoShow,
                CurrentServiceId, null, null, windowId, performerId, null, occurredOnUtc);
            ReleaseAssignment();
        }

        ModifiedOnUtc = occurredOnUtc;
        return true;
    }

    public bool TryCompleteCurrentService(
        int? nextServiceId,
        int? nextStepOrder,
        Guid performerId,
        DateTime occurredOnUtc)
    {
        if (Status != TicketStatus.InProgress)
        {
            return false;
        }

        var completedServiceId = CurrentServiceId;
        _serviceJourneys.Single(x => x.ServiceEndedOnUtc == null)
            .End(TicketJourneyOutcome.Completed, occurredOnUtc);
        AppendHistory(TicketHistoryEventType.ServiceCompleted, Status, Status,
            completedServiceId, null, null, CurrentWindowId, performerId, null, occurredOnUtc);

        if (!nextServiceId.HasValue)
        {
            Status = TicketStatus.Completed;
            CompletedOnUtc = occurredOnUtc;
            ReleaseAssignment();
            AppendHistory(TicketHistoryEventType.Completed, TicketStatus.InProgress, Status,
                completedServiceId, null, null, null, performerId, null, occurredOnUtc);
        }
        else
        {
            CurrentServiceId = nextServiceId.Value;
            CurrentWorkflowStepOrder = nextStepOrder;
            Status = TicketStatus.Waiting;
            CurrentQueueEnteredOnUtc = occurredOnUtc;
            CurrentServiceStartedOnUtc = null;
            ReleaseAssignment();
            _serviceJourneys.Add(TicketServiceJourney.Create(
                nextServiceId.Value,
                TicketJourneyEntryType.WorkflowTransition,
                nextStepOrder,
                occurredOnUtc));
            AppendHistory(TicketHistoryEventType.WorkflowTransition,
                TicketStatus.InProgress, TicketStatus.Waiting, null,
                completedServiceId, nextServiceId, null, performerId, null, occurredOnUtc);
        }

        ModifiedOnUtc = occurredOnUtc;
        return true;
    }

    public bool TryManualTransfer(
        int targetServiceId,
        string reason,
        Guid performerId,
        DateTime occurredOnUtc)
    {
        if (string.IsNullOrWhiteSpace(reason) || Status != TicketStatus.InProgress ||
            BoundWorkflowId.HasValue)
        {
            return false;
        }

        var sourceServiceId = CurrentServiceId;
        _serviceJourneys.Single(x => x.ServiceEndedOnUtc == null)
            .End(TicketJourneyOutcome.Transferred, occurredOnUtc);
        CurrentServiceId = targetServiceId;
        Status = TicketStatus.Waiting;
        CurrentQueueEnteredOnUtc = occurredOnUtc;
        CurrentServiceStartedOnUtc = null;
        ReleaseAssignment();
        _serviceJourneys.Add(TicketServiceJourney.Create(
            targetServiceId, TicketJourneyEntryType.ManualTransfer, null, occurredOnUtc));
        AppendHistory(TicketHistoryEventType.ManualTransfer,
            TicketStatus.InProgress, TicketStatus.Waiting, null,
            sourceServiceId, targetServiceId, null, performerId, reason.Trim(), occurredOnUtc);
        ModifiedOnUtc = occurredOnUtc;
        return true;
    }

    private void ReleaseAssignment()
    {
        CurrentWindowId = null;
        CurrentAssignedApplicationUserId = null;
    }

    private void AppendHistory(
        TicketHistoryEventType eventType,
        TicketStatus? fromStatus,
        TicketStatus? toStatus,
        int? serviceId,
        int? fromServiceId,
        int? toServiceId,
        int? windowId,
        Guid? performerId,
        string? reason,
        DateTime occurredOnUtc) =>
        _history.Add(TicketHistory.Create(eventType, fromStatus, toStatus, serviceId,
            fromServiceId, toServiceId, windowId, performerId, reason, occurredOnUtc));
}
