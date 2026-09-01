using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QControl.Api.Attribute;
using QControl.Api.Contracts.Kiosk;
using QControl.Api.Contracts.Tickets;
using QControl.Application.Features.TicketRuntime;
using QControl.Application.Features.TicketRuntime.Shared;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/branches/{branchId:int}/kiosk")]
public sealed class KioskController : ControllerBase
{
    private readonly ISender _sender;

    public KioskController(ISender sender) => _sender = sender;

    [HttpPost("reservations/search"), Permission("Reservations.View")]
    public async Task<IActionResult> SearchReservations(
        int branchId,
        KioskReservationSearchRequest request,
        CancellationToken cancellationToken) =>
        (await _sender.Send(new SearchKioskReservationsQuery
        {
            BranchId = branchId,
            ServiceId = request.ServiceId,
            Field = request.Field,
            CustomInputs = Map(request.CustomInputs)
        }, cancellationToken)).ToIActionResult();

    [HttpPost("reservations/{reservationId:int}/ticket"),
     Permission("Reservations.ConvertToTicket")]
    public async Task<IActionResult> CreateTicketFromReservation(
        int branchId,
        int reservationId,
        CancellationToken cancellationToken) =>
        (await _sender.Send(new CreateKioskTicketFromReservationCommand
        {
            BranchId = branchId,
            ReservationId = reservationId
        }, cancellationToken)).ToIActionResult();

    [HttpPost("tickets"), Permission("Tickets.Create")]
    public async Task<IActionResult> CreateTicket(
        int branchId,
        CreateKioskTicketRequest request,
        CancellationToken cancellationToken) =>
        (await _sender.Send(new CreateKioskTicketCommand
        {
            BranchId = branchId,
            ServiceId = request.ServiceId,
            SegmentId = request.SegmentId,
            Field = request.Field,
            CustomInputs = Map(request.CustomInputs)
        }, cancellationToken)).ToIActionResult();

    private static IReadOnlyCollection<CustomInputSubmission> Map(
        IReadOnlyCollection<CustomInputRequest>? values) =>
        values?.Select(x => new CustomInputSubmission(
            x.ServiceCustomInputId,
            x.Value)).ToArray() ?? Array.Empty<CustomInputSubmission>();
}
