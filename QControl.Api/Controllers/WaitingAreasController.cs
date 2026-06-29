using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.WaitingAreas;
using Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.DeactivateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.PermanentDeleteWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.ReactivateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreaById;
using Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/waiting-areas")]
[Authorize]
public sealed class WaitingAreasController : ControllerBase
{
    private readonly ISender sender;

    public WaitingAreasController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("WaitingAreas.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetWaitingAreasQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetWaitingAreasQuery();
        query.Search ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{waitingAreaId:int}")]
    [Permission("WaitingAreas.ViewDetails")]
    public async Task<IActionResult> GetById(
        int waitingAreaId,
        CancellationToken cancellationToken)
    {
        var query = new GetWaitingAreaByIdQuery
        {
            Id = waitingAreaId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("WaitingAreas.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateWaitingAreaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWaitingAreaCommand
        {
            BranchId = request.BranchId,
            Number = request.Number,
            AudioDevice = request.AudioDevice,
            ControlDevice = request.ControlDevice,
            DescriptiveName = request.DescriptiveName
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{waitingAreaId:int}")]
    [Permission("WaitingAreas.Update")]
    public async Task<IActionResult> Update(
        int waitingAreaId,
        [FromBody] UpdateWaitingAreaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWaitingAreaCommand
        {
            Id = waitingAreaId,
            Number = request.Number,
            AudioDevice = request.AudioDevice,
            ControlDevice = request.ControlDevice,
            DescriptiveName = request.DescriptiveName,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{waitingAreaId:int}/deactivate")]
    [Permission("WaitingAreas.Deactivate")]
    public async Task<IActionResult> Deactivate(
        int waitingAreaId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateWaitingAreaCommand
        {
            Id = waitingAreaId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{waitingAreaId:int}/reactivate")]
    [Permission("WaitingAreas.Reactivate")]
    public async Task<IActionResult> Reactivate(
        int waitingAreaId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateWaitingAreaCommand
        {
            Id = waitingAreaId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{waitingAreaId:int}/permanent")]
    [Permission("WaitingAreas.DeletePermanent")]
    public async Task<IActionResult> DeletePermanently(
        int waitingAreaId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new PermanentDeleteWaitingAreaCommand
        {
            Id = waitingAreaId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
