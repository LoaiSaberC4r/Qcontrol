using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.BranchServiceSegments;
using Qcontrol.Application.Features.BranchServiceSegments.Command.AssignBranchServiceSegments;
using Qcontrol.Application.Features.BranchServiceSegments.Command.UnassignBranchServiceSegment;
using Qcontrol.Application.Features.BranchServiceSegments.Command.UpdateBranchServiceSegmentQuota;
using Qcontrol.Application.Features.BranchServiceSegments.Query.GetAssignedBranchServiceSegments;
using Qcontrol.Application.Features.BranchServiceSegments.Query.GetAvailableBranchServiceSegments;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/services/{leafServiceId:int}/segments")]
[Authorize]
public sealed class BranchServiceSegmentsController : ControllerBase
{
    private readonly ISender sender;

    public BranchServiceSegmentsController(ISender sender)
    {
        this.sender = sender;
    }

    [HttpGet]
    [Permission("BranchServiceSegments.View")]
    public async Task<IActionResult> GetAssigned(
        int branchId,
        int leafServiceId,
        CancellationToken cancellationToken)
    {
        var query = new GetAssignedBranchServiceSegmentsQuery
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("available")]
    [Permission("BranchServiceSegments.ViewAvailable")]
    public async Task<IActionResult> GetAvailable(
        int branchId,
        int leafServiceId,
        [FromQuery] GetAvailableBranchServiceSegmentsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetAvailableBranchServiceSegmentsQuery();
        query.BranchId = branchId;
        query.LeafServiceId = leafServiceId;
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("assign")]
    [Permission("BranchServiceSegments.Assign")]
    public async Task<IActionResult> Assign(
        int branchId,
        int leafServiceId,
        [FromBody] AssignBranchServiceSegmentsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignBranchServiceSegmentsCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            Items = request.Items.Select(x =>
                new AssignBranchServiceSegmentItem
                {
                    SegmentId = x.SegmentId,
                    Quota = x.Quota
                }).ToList()
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{segmentId:int}/quota")]
    [Permission("BranchServiceSegments.UpdateQuota")]
    public async Task<IActionResult> UpdateQuota(
        int branchId,
        int leafServiceId,
        int segmentId,
        [FromBody] UpdateBranchServiceSegmentQuotaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBranchServiceSegmentQuotaCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            SegmentId = segmentId,
            Quota = request.Quota,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{segmentId:int}")]
    [Permission("BranchServiceSegments.Unassign")]
    public async Task<IActionResult> Unassign(
        int branchId,
        int leafServiceId,
        int segmentId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new UnassignBranchServiceSegmentCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            SegmentId = segmentId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
