using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Segments;
using Qcontrol.Application.Features.Segments.Command.CreateGlobalSegment;
using Qcontrol.Application.Features.Segments.Command.UpdateSegment;
using Qcontrol.Application.Features.Segments.Query.GetSegmentById;
using Qcontrol.Application.Features.Segments.Query.GetSegments;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/segments")]
[Authorize]
public sealed class SegmentsController : ControllerBase
{
    private readonly ISender sender;

    public SegmentsController(ISender sender)
    {
        this.sender = sender;
    }

    [HttpGet]
    [Permission("Segments.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetSegmentsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetSegmentsQuery();
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{segmentId:int}")]
    [Permission("Segments.ViewDetails")]
    public async Task<IActionResult> GetById(
        int segmentId,
        CancellationToken cancellationToken)
    {
        var query = new GetSegmentByIdQuery
        {
            SegmentId = segmentId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("Segments.CreateGlobal")]
    public async Task<IActionResult> CreateGlobal(
        [FromBody] CreateGlobalSegmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateGlobalSegmentCommand
        {
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            Priority = request.Priority
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{segmentId:int}")]
    [Permission("Segments.Update")]
    public async Task<IActionResult> Update(
        int segmentId,
        [FromBody] UpdateSegmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSegmentCommand
        {
            SegmentId = segmentId,
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            Priority = request.Priority,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
