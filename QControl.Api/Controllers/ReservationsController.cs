using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QControl.Api.Attribute;
using QControl.Api.Contracts.Reservations;
using QControl.Api.Contracts.Tickets;
using QControl.Application.Features.TicketRuntime;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/branches/{branchId:int}/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ISender _sender;
    public ReservationsController(ISender sender) => _sender = sender;

    [HttpPost, Permission("Reservations.Create")]
    public async Task<IActionResult> Create(int branchId, CreateReservationRequest request, CancellationToken ct)
        => (await _sender.Send(new CreateReservationCommand { BranchId = branchId, ServiceId = request.ServiceId, SegmentId = request.SegmentId, ScheduledOnUtc = request.ScheduledOnUtc, CustomInputs = Map(request.CustomInputs) }, ct)).ToIActionResult();

    [HttpGet, Permission("Reservations.View")]
    public async Task<IActionResult> Get(int branchId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        [FromQuery] ReservationStatus? status = null, [FromQuery] int? serviceId = null, [FromQuery] int? segmentId = null,
        [FromQuery] DateOnly? businessDate = null, CancellationToken ct = default)
        => (await _sender.Send(new GetReservationsQuery { BranchId = branchId, PageNumber = pageNumber, PageSize = pageSize, Status = status, ServiceId = serviceId, SegmentId = segmentId, BusinessDate = businessDate }, ct)).ToIActionResult();

    [HttpGet("{reservationId:int}"), Permission("Reservations.View")]
    public async Task<IActionResult> GetById(int branchId, int reservationId, CancellationToken ct)
        => (await _sender.Send(new GetReservationByIdQuery(branchId, reservationId), ct)).ToIActionResult();

    [HttpGet("{reservationId:int}/history"), Permission("Reservations.ViewHistory")]
    public async Task<IActionResult> History(int branchId, int reservationId, CancellationToken ct)
        => (await _sender.Send(new GetReservationHistoryQuery(branchId, reservationId), ct)).ToIActionResult();

    [HttpPost("{reservationId:int}/cancel"), Permission("Reservations.Cancel")]
    public async Task<IActionResult> Cancel(int branchId, int reservationId, ReasonRequest request, CancellationToken ct)
        => (await _sender.Send(new CancelReservationCommand { BranchId = branchId, ReservationId = reservationId, Reason = request.Reason }, ct)).ToIActionResult();

    [HttpPost("{reservationId:int}/ticket"), Permission("Reservations.ConvertToTicket")]
    public async Task<IActionResult> Convert(int branchId, int reservationId, CancellationToken ct)
        => (await _sender.Send(new CreateTicketFromReservationCommand { BranchId = branchId, ReservationId = reservationId }, ct)).ToIActionResult();

    [HttpGet("archive"), Permission("Reservations.ViewArchive")]
    public async Task<IActionResult> Archive(int branchId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        [FromQuery] ReservationStatus? status = null, [FromQuery] int? serviceId = null, [FromQuery] int? segmentId = null,
        [FromQuery] DateOnly? businessDate = null, CancellationToken ct = default)
        => (await _sender.Send(new GetArchivedReservationsQuery { BranchId = branchId, PageNumber = pageNumber, PageSize = pageSize, Status = status, ServiceId = serviceId, SegmentId = segmentId, BusinessDate = businessDate }, ct)).ToIActionResult();

    [HttpGet("archive/{reservationId:int}"), Permission("Reservations.ViewArchive")]
    public async Task<IActionResult> ArchivedById(int branchId, int reservationId, CancellationToken ct)
        => (await _sender.Send(new GetArchivedReservationByIdQuery(branchId, reservationId), ct)).ToIActionResult();

    private static IReadOnlyCollection<CustomInputSubmission> Map(IReadOnlyCollection<CustomInputRequest>? values)
        => values?.Select(x => new CustomInputSubmission(x.ServiceCustomInputId, x.Value)).ToArray() ?? Array.Empty<CustomInputSubmission>();
}
