using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QControl.Api.Attribute;
using QControl.Api.Contracts.Tickets;
using QControl.Application.Features.TicketRuntime;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/branches/{branchId:int}/tickets")]
public sealed class TicketsController : ControllerBase
{
    private readonly ISender _sender;
    public TicketsController(ISender sender) => _sender = sender;

    [HttpPost, Permission("Tickets.Create")]
    public async Task<IActionResult> Create(int branchId, CreateTicketRequest request, CancellationToken ct)
        => (await _sender.Send(new CreateTicketCommand { BranchId = branchId, ServiceId = request.ServiceId, SegmentId = request.SegmentId, Field = request.Field, CustomInputs = Map(request.CustomInputs) }, ct)).ToIActionResult();

    [HttpGet, Permission("Tickets.View")]
    public async Task<IActionResult> Get(int branchId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        [FromQuery] TicketStatus? status = null, [FromQuery] int? serviceId = null, [FromQuery] int? segmentId = null,
        [FromQuery] string? ticketNumber = null, [FromQuery] DateOnly? businessDate = null, CancellationToken ct = default)
        => (await _sender.Send(new GetTicketsQuery { BranchId = branchId, PageNumber = pageNumber, PageSize = pageSize, Status = status, ServiceId = serviceId, SegmentId = segmentId, TicketNumber = ticketNumber, BusinessDate = businessDate }, ct)).ToIActionResult();

    [HttpGet("{ticketId:int}"), Permission("Tickets.View")]
    public async Task<IActionResult> GetById(int branchId, int ticketId, CancellationToken ct)
        => (await _sender.Send(new GetTicketByIdQuery(branchId, ticketId), ct)).ToIActionResult();

    [HttpGet("{ticketId:int}/history"), Permission("Tickets.ViewHistory")]
    public async Task<IActionResult> History(int branchId, int ticketId, CancellationToken ct)
        => (await _sender.Send(new GetTicketHistoryQuery(branchId, ticketId), ct)).ToIActionResult();

    [HttpPost("{ticketId:int}/cancel"), Permission("Tickets.Cancel")]
    public async Task<IActionResult> Cancel(int branchId, int ticketId, ReasonRequest request, CancellationToken ct)
        => (await _sender.Send(new CancelTicketCommand { BranchId = branchId, TicketId = ticketId, Reason = request.Reason }, ct)).ToIActionResult();

    [HttpPost("{ticketId:int}/return-to-waiting"), Permission("Tickets.ReturnNoShowToWaiting")]
    public async Task<IActionResult> ReturnToWaiting(int branchId, int ticketId, CancellationToken ct)
        => (await _sender.Send(new ReturnNoShowTicketToWaitingCommand { BranchId = branchId, TicketId = ticketId }, ct)).ToIActionResult();

    [HttpPost("{ticketId:int}/start-service"), Permission("Tickets.StartService")]
    public async Task<IActionResult> StartService(int branchId, int ticketId, CancellationToken ct)
        => (await _sender.Send(new StartTicketServiceCommand { BranchId = branchId, TicketId = ticketId }, ct)).ToIActionResult();

    [HttpPost("{ticketId:int}/complete-service"), Permission("Tickets.CompleteService")]
    public async Task<IActionResult> CompleteService(int branchId, int ticketId, CancellationToken ct)
        => (await _sender.Send(new CompleteTicketServiceCommand { BranchId = branchId, TicketId = ticketId }, ct)).ToIActionResult();

    [HttpPost("{ticketId:int}/manual-transfer"), Permission("Tickets.ManualTransfer")]
    public async Task<IActionResult> ManualTransfer(int branchId, int ticketId, ManualTransferTicketRequest request, CancellationToken ct)
        => (await _sender.Send(new ManualTransferTicketCommand { BranchId = branchId, TicketId = ticketId, TargetServiceId = request.TargetServiceId, Reason = request.Reason }, ct)).ToIActionResult();

    [HttpGet("archive"), Permission("Tickets.ViewArchive")]
    public async Task<IActionResult> Archive(int branchId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        [FromQuery] TicketStatus? status = null, [FromQuery] int? serviceId = null, [FromQuery] int? segmentId = null,
        [FromQuery] string? ticketNumber = null, [FromQuery] DateOnly? businessDate = null, CancellationToken ct = default)
        => (await _sender.Send(new GetArchivedTicketsQuery { BranchId = branchId, PageNumber = pageNumber, PageSize = pageSize, Status = status, ServiceId = serviceId, SegmentId = segmentId, TicketNumber = ticketNumber, BusinessDate = businessDate }, ct)).ToIActionResult();

    [HttpGet("archive/{ticketId:int}"), Permission("Tickets.ViewArchive")]
    public async Task<IActionResult> ArchivedById(int branchId, int ticketId, CancellationToken ct)
        => (await _sender.Send(new GetArchivedTicketByIdQuery(branchId, ticketId), ct)).ToIActionResult();

    private static IReadOnlyCollection<CustomInputSubmission> Map(IReadOnlyCollection<CustomInputRequest>? values)
        => values?.Select(x => new CustomInputSubmission(x.ServiceCustomInputId, x.Value)).ToArray() ?? Array.Empty<CustomInputSubmission>();
}
