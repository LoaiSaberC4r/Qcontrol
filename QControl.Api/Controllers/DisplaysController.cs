using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Displays;
using Qcontrol.Application.Features.Displays.Command.CreateDisplay;
using Qcontrol.Application.Features.Displays.Command.DeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.RestoreDisplay;
using Qcontrol.Application.Features.Displays.Command.UpdateDisplay;
using Qcontrol.Application.Features.Displays.Query.GetDeletedDisplays;
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

    [HttpGet("deleted")]
    [Permission("Displays.ViewDeleted")]
    public async Task<IActionResult> GetDeleted(
        [FromQuery] GetDeletedDisplaysQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetDeletedDisplaysQuery();
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
            RequestId = request.Id,
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

    [HttpDelete("{displayId:int}")]
    [Permission("Displays.Delete")]
    public async Task<IActionResult> Delete(
        int displayId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDisplayCommand
        {
            Id = displayId
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{displayId:int}/restore")]
    [Permission("Displays.Restore")]
    public async Task<IActionResult> Restore(
        int displayId,
        CancellationToken cancellationToken)
    {
        var command = new RestoreDisplayCommand
        {
            Id = displayId
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
        CancellationToken cancellationToken)
    {
        var command = new PermanentDeleteDisplayCommand
        {
            Id = displayId
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
