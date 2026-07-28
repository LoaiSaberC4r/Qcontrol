using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Segments;
using Qcontrol.Application.Features.Segments.Command.CreateBranchSegment;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/segments")]
[Authorize]
public sealed class BranchSegmentsController : ControllerBase
{
    private readonly ISender sender;

    public BranchSegmentsController(ISender sender)
    {
        this.sender = sender;
    }

    [HttpPost]
    [Permission("Segments.CreateBranchScoped")]
    public async Task<IActionResult> Create(
        int branchId,
        [FromBody] CreateBranchSegmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBranchSegmentCommand
        {
            BranchId = branchId,
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            Priority = request.Priority
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
