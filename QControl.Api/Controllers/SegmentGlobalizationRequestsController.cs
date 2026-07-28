using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.SegmentGlobalizationRequests;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Command.ApproveSegmentGlobalizationRequest;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Command.RejectSegmentGlobalizationRequest;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetSegmentGlobalizationRequestById;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetSegmentGlobalizationRequests;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/segment-globalization-requests")]
[Authorize]
public sealed class SegmentGlobalizationRequestsController : ControllerBase
{
    private readonly ISender sender;

    public SegmentGlobalizationRequestsController(ISender sender)
    {
        this.sender = sender;
    }

    [HttpGet]
    [Permission("SegmentGlobalizationRequests.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetSegmentGlobalizationRequestsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetSegmentGlobalizationRequestsQuery();
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{requestId:int}")]
    [Permission("SegmentGlobalizationRequests.ViewDetails")]
    public async Task<IActionResult> GetById(
        int requestId,
        CancellationToken cancellationToken)
    {
        var query = new GetSegmentGlobalizationRequestByIdQuery
        {
            RequestId = requestId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{requestId:int}/approve")]
    [Permission("SegmentGlobalizationRequests.Approve")]
    public async Task<IActionResult> Approve(
        int requestId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ApproveSegmentGlobalizationRequestCommand
        {
            RequestId = requestId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{requestId:int}/reject")]
    [Permission("SegmentGlobalizationRequests.Reject")]
    public async Task<IActionResult> Reject(
        int requestId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        [FromBody] RejectSegmentGlobalizationRequestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RejectSegmentGlobalizationRequestCommand
        {
            RequestId = requestId,
            RowVersion = rowVersion,
            RejectionReason = request.RejectionReason
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
