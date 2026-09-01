namespace QControl.Domain.Enums;

public enum TicketStatus
{
    Waiting = 1,
    Called = 2,
    InProgress = 3,
    NoShow = 4,
    Completed = 5,
    Cancelled = 6
}

public enum ReservationStatus
{
    Active = 1,
    NoShow = 2,
    Cancelled = 3,
    Expired = 4,
    ConvertedToTicket = 5
}

public enum TicketHistoryEventType
{
    Created = 1,
    Called = 2,
    CallAttempt = 3,
    ServiceStarted = 4,
    NoShow = 5,
    ReturnedToWaiting = 6,
    ServiceCompleted = 7,
    WorkflowTransition = 8,
    ManualTransfer = 9,
    Cancelled = 10,
    Completed = 11,
    Archived = 12
}

public enum ReservationHistoryEventType
{
    Created = 1,
    NoShow = 2,
    Cancelled = 3,
    Expired = 4,
    ConvertedToTicket = 5,
    Archived = 6
}

public enum TicketJourneyEntryType
{
    Initial = 1,
    WorkflowTransition = 2,
    ManualTransfer = 3
}

public enum TicketJourneyOutcome
{
    Completed = 1,
    Transferred = 2
}
