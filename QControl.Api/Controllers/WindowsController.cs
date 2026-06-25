using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Windows;
using Qcontrol.Application.Features.Windows.Command.CreateWindow;
using Qcontrol.Application.Features.Windows.Command.DeleteWindow;
using Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;
using Qcontrol.Application.Features.Windows.Command.UpdateWindow;
using Qcontrol.Application.Features.Windows.Query.GetDeletedWindows;
using Qcontrol.Application.Features.Windows.Query.GetWindowById;
using Qcontrol.Application.Features.Windows.Query.GetWindows;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/windows")]
[Authorize]
public sealed class WindowsController : ControllerBase
{
    private readonly ISender sender;

    public WindowsController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("Windows.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetWindowsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetWindowsQuery();
        query.Search ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("deleted")]
    [Permission("Windows.ViewDeleted")]
    public async Task<IActionResult> GetDeleted(
        [FromQuery] GetDeletedWindowsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetDeletedWindowsQuery();
        query.Search ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{windowId:int}")]
    [Permission("Windows.ViewDetails")]
    public async Task<IActionResult> GetById(
        int windowId,
        CancellationToken cancellationToken)
    {
        var query = new GetWindowByIdQuery
        {
            Id = windowId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("Windows.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateWindowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWindowCommand
        {
            WaitingAreaId = request.WaitingAreaId,
            Number = request.Number,
            DescriptiveName = request.DescriptiveName,
            IPAddress = request.IPAddress,
            EnableTicketBooking = request.EnableTicketBooking,
            EnableDirectCall = request.EnableDirectCall
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{windowId:int}")]
    [Permission("Windows.Update")]
    public async Task<IActionResult> Update(
        int windowId,
        [FromBody] UpdateWindowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWindowCommand
        {
            Id = windowId,
            RequestId = request.Id,
            Number = request.Number,
            DescriptiveName = request.DescriptiveName,
            IPAddress = request.IPAddress,
            EnableTicketBooking = request.EnableTicketBooking,
            EnableDirectCall = request.EnableDirectCall
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{windowId:int}")]
    [Permission("Windows.Delete")]
    public async Task<IActionResult> Delete(
        int windowId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteWindowCommand
        {
            Id = windowId
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{windowId:int}/permanent")]
    [Permission("Windows.DeletePermanent")]
    public async Task<IActionResult> DeletePermanently(
        int windowId,
        CancellationToken cancellationToken)
    {
        var command = new PermanentDeleteWindowCommand
        {
            Id = windowId
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
