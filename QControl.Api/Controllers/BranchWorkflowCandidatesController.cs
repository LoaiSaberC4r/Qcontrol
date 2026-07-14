using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowCandidateServices;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/services")]
[Authorize]
public sealed class BranchWorkflowCandidatesController : ControllerBase
{
    private readonly ISender sender;

    public BranchWorkflowCandidatesController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet("workflow-candidates")]
    [Permission("ServiceWorkflows.ViewCandidateServices")]
    public async Task<IActionResult> GetWorkflowCandidates(
        int branchId,
        [FromQuery] GetWorkflowCandidateServicesQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetWorkflowCandidateServicesQuery();
        query.BranchId = branchId;
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }
}
