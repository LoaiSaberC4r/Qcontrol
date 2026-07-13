using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.ServiceGlobalizationRequests;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Command.ApproveServiceGlobalizationRequest;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Command.RejectServiceGlobalizationRequest;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetServiceGlobalizationRequestById;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetServiceGlobalizationRequests;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/service-globalization-requests")]
[Authorize]
public sealed class ServiceGlobalizationRequestsController : ControllerBase
{
    private readonly ISender sender;

    public ServiceGlobalizationRequestsController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("ServiceGlobalizationRequests.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetServiceGlobalizationRequestsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetServiceGlobalizationRequestsQuery();
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{requestId:int}")]
    [Permission("ServiceGlobalizationRequests.ViewDetails")]
    public async Task<IActionResult> GetById(
        int requestId,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceGlobalizationRequestByIdQuery
        {
            RequestId = requestId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{requestId:int}/approve")]
    [Permission("ServiceGlobalizationRequests.Approve")]
    public async Task<IActionResult> Approve(
        int requestId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ApproveServiceGlobalizationRequestCommand
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
    [Permission("ServiceGlobalizationRequests.Reject")]
    public async Task<IActionResult> Reject(
        int requestId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        [FromBody] RejectServiceGlobalizationRequestRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new RejectServiceGlobalizationRequestCommand
        {
            RequestId = requestId,
            RowVersion = rowVersion,
            RejectionReason = request?.RejectionReason
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
