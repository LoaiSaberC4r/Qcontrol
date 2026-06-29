using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Terminals;
using Qcontrol.Application.Features.Terminals.Command.CreateTerminal;
using Qcontrol.Application.Features.Terminals.Command.DeactivateTerminal;
using Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;
using Qcontrol.Application.Features.Terminals.Command.ReactivateTerminal;
using Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;
using Qcontrol.Application.Features.Terminals.Query.GetTerminalById;
using Qcontrol.Application.Features.Terminals.Query.GetTerminals;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/terminals")]
[Authorize]
public sealed class TerminalsController : ControllerBase
{
    private readonly ISender sender;

    public TerminalsController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("Terminals.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetTerminalsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetTerminalsQuery();
        query.Search ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{terminalId:int}")]
    [Permission("Terminals.ViewDetails")]
    public async Task<IActionResult> GetById(
        int terminalId,
        CancellationToken cancellationToken)
    {
        var query = new GetTerminalByIdQuery
        {
            Id = terminalId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("Terminals.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateTerminalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTerminalCommand
        {
            WindowId = request.WindowId,
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

    [HttpPut("{terminalId:int}")]
    [Permission("Terminals.Update")]
    public async Task<IActionResult> Update(
        int terminalId,
        [FromBody] UpdateTerminalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTerminalCommand
        {
            Id = terminalId,
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

    [HttpPost("{terminalId:int}/deactivate")]
    [Permission("Terminals.Deactivate")]
    public async Task<IActionResult> Deactivate(
        int terminalId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateTerminalCommand
        {
            Id = terminalId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{terminalId:int}/reactivate")]
    [Permission("Terminals.Reactivate")]
    public async Task<IActionResult> Reactivate(
        int terminalId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateTerminalCommand
        {
            Id = terminalId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{terminalId:int}/permanent")]
    [Permission("Terminals.DeletePermanent")]
    public async Task<IActionResult> DeletePermanently(
        int terminalId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new PermanentDeleteTerminalCommand
        {
            Id = terminalId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
