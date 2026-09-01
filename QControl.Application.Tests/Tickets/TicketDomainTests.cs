using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Tickets;

public sealed class TicketDomainTests
{
    private static readonly Guid Performer = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime Now = new(2026, 8, 31, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_enters_waiting_and_appends_initial_journey_and_history()
    {
        var ticket = Create();
        Assert.Equal(TicketStatus.Waiting, ticket.Status);
        Assert.Equal("A1", ticket.TicketNumber);
        Assert.Equal(TicketJourneyEntryType.Initial, Assert.Single(ticket.ServiceJourneys).EntryType);
        Assert.Equal(TicketHistoryEventType.Created, Assert.Single(ticket.History).EventType);
    }

    [Fact]
    public void Waiting_can_cancel_but_cancelled_is_terminal()
    {
        var ticket = Create();
        Assert.True(ticket.TryCancel("customer request", Performer, Now.AddMinutes(1)));
        Assert.Equal(TicketStatus.Cancelled, ticket.Status);
        Assert.False(ticket.TryReturnNoShowToWaiting(Performer, Now.AddMinutes(2)));
        Assert.False(ticket.TryStartService(Performer, Now.AddMinutes(2)));
        Assert.False(ticket.TryCancel("again", Performer, Now.AddMinutes(2)));
    }

    [Fact]
    public void Call_attempt_limit_marks_no_show_and_return_starts_new_cycle()
    {
        var ticket = Create();
        Assert.True(ticket.TryRecordCallAttempt(4, Performer, 2, Now.AddMinutes(1)));
        Assert.Equal(TicketStatus.Called, ticket.Status);
        Assert.True(ticket.TryRecordCallAttempt(4, Performer, 2, Now.AddMinutes(2)));
        Assert.Equal(TicketStatus.NoShow, ticket.Status);
        Assert.Null(ticket.CurrentWindowId);
        Assert.True(ticket.TryReturnNoShowToWaiting(Performer, Now.AddMinutes(3)));
        Assert.True(ticket.TryRecordCallAttempt(5, Performer, 3, Now.AddMinutes(4)));
        Assert.Equal(2, ticket.CurrentCallCycleNumber);
        Assert.Equal(new[] { 1, 2 }, ticket.CallAttempts.Select(x => x.CallCycleNumber).Distinct());
    }

    [Fact]
    public void Called_starts_service_and_in_progress_cannot_cancel()
    {
        var ticket = Create();
        ticket.TryRecordCallAttempt(4, Performer, 3, Now.AddMinutes(1));
        Assert.True(ticket.TryStartService(Performer, Now.AddMinutes(2)));
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.NotNull(Assert.Single(ticket.ServiceJourneys).ServiceStartedOnUtc);
        Assert.False(ticket.TryCancel("not allowed", Performer, Now.AddMinutes(3)));
    }

    [Fact]
    public void Workflow_completion_moves_same_ticket_and_number_then_completes_final_step()
    {
        var ticket = Create();
        ticket.BindWorkflow(7, new[] { (10, 1), (20, 2) }, Now);
        CallAndStart(ticket, Now);
        Assert.True(ticket.TryCompleteCurrentService(20, 2, Performer, Now.AddMinutes(3)));
        Assert.Equal(TicketStatus.Waiting, ticket.Status);
        Assert.Equal(20, ticket.CurrentServiceId);
        Assert.Equal("A1", ticket.TicketNumber);
        CallAndStart(ticket, Now.AddMinutes(4));
        Assert.True(ticket.TryCompleteCurrentService(null, null, Performer, Now.AddMinutes(7)));
        Assert.Equal(TicketStatus.Completed, ticket.Status);
        Assert.Equal(2, ticket.ServiceJourneys.Count);
        Assert.False(ticket.TryReturnNoShowToWaiting(Performer, Now.AddMinutes(8)));
    }

    [Fact]
    public void Manual_transfer_records_transferred_outcome_and_preserves_number()
    {
        var ticket = Create();
        CallAndStart(ticket, Now);
        Assert.True(ticket.TryManualTransfer(30, "specialist", Performer, Now.AddMinutes(3)));
        Assert.Equal(TicketStatus.Waiting, ticket.Status);
        Assert.Equal(30, ticket.CurrentServiceId);
        Assert.Equal("A1", ticket.TicketNumber);
        Assert.Equal(TicketJourneyOutcome.Transferred, ticket.ServiceJourneys.OrderBy(x => x.EnteredWaitingOnUtc).First().Outcome);
        Assert.Equal(TicketJourneyEntryType.ManualTransfer, ticket.ServiceJourneys.OrderBy(x => x.EnteredWaitingOnUtc).Last().EntryType);
    }

    [Fact]
    public void Workflow_snapshot_blocks_manual_transfer_and_is_immutable_from_source_collection()
    {
        var ticket = Create();
        var steps = new List<(int ServiceId, int StepOrder)> { (10, 1), (20, 2) };
        ticket.BindWorkflow(7, steps, Now);
        steps[1] = (99, 2);
        CallAndStart(ticket, Now);
        Assert.False(ticket.TryManualTransfer(30, "outside", Performer, Now.AddMinutes(3)));
        Assert.Equal(20, ticket.WorkflowSteps.Single(x => x.StepOrder == 2).ServiceId);
    }

    private static Ticket Create() => Ticket.Create(1, 10, 3, null, "A1",
        new DateOnly(2026, 8, 31), Now, Performer);

    private static void CallAndStart(Ticket ticket, DateTime at)
    {
        Assert.True(ticket.TryRecordCallAttempt(4, Performer, 3, at.AddMinutes(1)));
        Assert.True(ticket.TryStartService(Performer, at.AddMinutes(2)));
    }
}
