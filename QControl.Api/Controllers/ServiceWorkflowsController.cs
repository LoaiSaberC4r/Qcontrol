using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.ServiceWorkflows;
using Qcontrol.Application.Features.ServiceWorkflows.Command.CreateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.DeactivateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.ReactivateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.UpdateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowById;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflows;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowCandidateServices;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/service-workflows")]
[Authorize]
public sealed class ServiceWorkflowsController : ControllerBase
{
    private readonly ISender sender;

    public ServiceWorkflowsController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet("candidate-services")]
    [Permission("ServiceWorkflows.ViewCandidateServices")]
    public async Task<IActionResult> GetCandidateServices(
        [FromQuery] GetWorkflowCandidateServicesQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetWorkflowCandidateServicesQuery();
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet]
    [Permission("ServiceWorkflows.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetServiceWorkflowsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetServiceWorkflowsQuery();
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{workflowId:int}")]
    [Permission("ServiceWorkflows.ViewDetails")]
    public async Task<IActionResult> GetById(
        int workflowId,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceWorkflowByIdQuery
        {
            Id = workflowId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("ServiceWorkflows.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceWorkflowCommand
        {
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            Steps = ToCommandSteps(request.Steps)
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{workflowId:int}")]
    [Permission("ServiceWorkflows.Update")]
    public async Task<IActionResult> Update(
        int workflowId,
        [FromBody] UpdateServiceWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceWorkflowCommand
        {
            Id = workflowId,
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            Steps = ToCommandSteps(request.Steps),
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{workflowId:int}/deactivate")]
    [Permission("ServiceWorkflows.Deactivate")]
    public async Task<IActionResult> Deactivate(
        int workflowId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceWorkflowCommand
        {
            Id = workflowId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{workflowId:int}/reactivate")]
    [Permission("ServiceWorkflows.Reactivate")]
    public async Task<IActionResult> Reactivate(
        int workflowId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateServiceWorkflowCommand
        {
            Id = workflowId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    private static IReadOnlyList<ServiceWorkflowStepCommandItem> ToCommandSteps(
        IEnumerable<ServiceWorkflowStepRequest>? steps)
    {
        if (steps is null)
        {
            return Array.Empty<ServiceWorkflowStepCommandItem>();
        }

        return steps
            .Select(x => new ServiceWorkflowStepCommandItem
            {
                ServiceId = x.ServiceId,
                StepOrder = x.StepOrder
            })
            .ToList();
    }
}
