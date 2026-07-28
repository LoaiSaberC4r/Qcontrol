using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetBranchSegmentGlobalizationRequests;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/segment-globalization-requests")]
[Authorize]
public sealed class BranchSegmentGlobalizationRequestsController
    : ControllerBase
{
    private readonly ISender sender;

    public BranchSegmentGlobalizationRequestsController(ISender sender)
    {
        this.sender = sender;
    }

    [HttpGet]
    [Permission("SegmentGlobalizationRequests.ViewOwn")]
    public async Task<IActionResult> GetPaginated(
        int branchId,
        [FromQuery] GetBranchSegmentGlobalizationRequestsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetBranchSegmentGlobalizationRequestsQuery();
        query.BranchId = branchId;
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }
}
