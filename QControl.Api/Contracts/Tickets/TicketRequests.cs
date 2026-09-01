namespace QControl.Api.Contracts.Tickets;

public sealed record CustomInputRequest(int ServiceCustomInputId, string Value);
public sealed record CreateTicketRequest(int ServiceId, int SegmentId, string? Field,
    IReadOnlyCollection<CustomInputRequest> CustomInputs);
public sealed record ReasonRequest(string Reason);
public sealed record ManualTransferTicketRequest(int TargetServiceId, string Reason);
