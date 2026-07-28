using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Displays;
using Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;
using Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;
using Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;
using Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;
using Qcontrol.Application.Features.Displays.Command.CreateDisplay;
using Qcontrol.Application.Features.Displays.Command.DeactivateDisplay;
using Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.ReactivateDisplay;
using Qcontrol.Application.Features.Displays.Command.UpdateDisplay;
using Qcontrol.Application.Features.Displays.Query.GetDisplayById;
using Qcontrol.Application.Features.Displays.Query.GetDisplays;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/displays")]
[Authorize]
public sealed class DisplaysController : ControllerBase
{
    private readonly ISender sender;

    public DisplaysController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("Displays.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetDisplaysQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetDisplaysQuery();
        query.Search ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{displayId:int}")]
    [Permission("Displays.ViewDetails")]
    public async Task<IActionResult> GetById(
        int displayId,
        CancellationToken cancellationToken)
    {
        var query = new GetDisplayByIdQuery
        {
            Id = displayId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{displayId:int}/windows")]
    [Permission("DisplayWindows.ViewLinked")]
    public async Task<IActionResult> GetLinkedWindows(
        int displayId,
        [FromQuery] GetDisplayLinkedWindowsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetDisplayLinkedWindowsQuery();
        query.DisplayId = displayId;
        query.Search ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{displayId:int}/available-windows")]
    [Permission("DisplayWindows.ViewAvailable")]
    public async Task<IActionResult> GetAvailableWindows(
        int displayId,
        [FromQuery] GetDisplayAvailableWindowsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetDisplayAvailableWindowsQuery();
        query.DisplayId = displayId;
        query.Search ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{displayId:int}/windows/{windowId:int}")]
    [Permission("DisplayWindows.Assign")]
    public async Task<IActionResult> AssignWindow(
        int displayId,
        int windowId,
        CancellationToken cancellationToken)
    {
        var command = new AssignWindowToDisplayCommand
        {
            DisplayId = displayId,
            WindowId = windowId
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{displayId:int}/windows/{windowId:int}")]
    [Permission("DisplayWindows.Unassign")]
    public async Task<IActionResult> UnassignWindow(
        int displayId,
        int windowId,
        CancellationToken cancellationToken)
    {
        var command = new UnassignWindowFromDisplayCommand
        {
            DisplayId = displayId,
            WindowId = windowId
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("Displays.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateDisplayRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDisplayCommand
        {
            BranchId = request.BranchId,
            Number = request.Number,
            IPAddress = request.IPAddress,
            SerialNo = request.SerialNo,
            Type = request.Type
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{displayId:int}")]
    [Permission("Displays.Update")]
    public async Task<IActionResult> Update(
        int displayId,
        [FromBody] UpdateDisplayRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDisplayCommand
        {
            Id = displayId,
            Number = request.Number,
            IPAddress = request.IPAddress,
            SerialNo = request.SerialNo,
            Type = request.Type,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{displayId:int}/deactivate")]
    [Permission("Displays.Deactivate")]
    public async Task<IActionResult> Deactivate(
        int displayId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateDisplayCommand
        {
            Id = displayId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{displayId:int}/reactivate")]
    [Permission("Displays.Reactivate")]
    public async Task<IActionResult> Reactivate(
        int displayId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateDisplayCommand
        {
            Id = displayId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{displayId:int}/permanent")]
    [Permission("Displays.DeletePermanent")]
    public async Task<IActionResult> DeletePermanently(
        int displayId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new PermanentDeleteDisplayCommand
        {
            Id = displayId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}