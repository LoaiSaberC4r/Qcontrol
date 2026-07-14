using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.ServiceWorkflows;
using Qcontrol.Application.Features.ServiceWorkflows.Command.CreateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.DeactivateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.ReactivateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.SetDefaultServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Command.UpdateServiceWorkflow;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowById;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflows;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowStartOptions;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/branches/{branchId:int}/services/{leafServiceId:int}")]
[Authorize]
public sealed class BranchServiceWorkflowsController : ControllerBase
{
    private readonly ISender sender;

    public BranchServiceWorkflowsController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet("workflows")]
    [Permission("ServiceWorkflows.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        int branchId,
        int leafServiceId,
        [FromQuery] GetServiceWorkflowsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetServiceWorkflowsQuery();
        query.BranchId = branchId;
        query.LeafServiceId = leafServiceId;
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("workflows/{workflowId:int}")]
    [Permission("ServiceWorkflows.ViewDetails")]
    public async Task<IActionResult> GetById(
        int branchId,
        int leafServiceId,
        int workflowId,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceWorkflowByIdQuery
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            Id = workflowId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("workflows")]
    [Permission("ServiceWorkflows.Create")]
    public async Task<IActionResult> Create(
        int branchId,
        int leafServiceId,
        [FromBody] CreateServiceWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceWorkflowCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            Steps = ToCommandSteps(request.Steps)
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("workflows/{workflowId:int}")]
    [Permission("ServiceWorkflows.Update")]
    public async Task<IActionResult> Update(
        int branchId,
        int leafServiceId,
        int workflowId,
        [FromBody] UpdateServiceWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceWorkflowCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
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

    [HttpPost("workflows/{workflowId:int}/set-default")]
    [Permission("ServiceWorkflows.SetDefault")]
    public async Task<IActionResult> SetDefault(
        int branchId,
        int leafServiceId,
        int workflowId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new SetDefaultServiceWorkflowCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            Id = workflowId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("workflows/{workflowId:int}/deactivate")]
    [Permission("ServiceWorkflows.Deactivate")]
    public async Task<IActionResult> Deactivate(
        int branchId,
        int leafServiceId,
        int workflowId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceWorkflowCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            Id = workflowId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("workflows/{workflowId:int}/reactivate")]
    [Permission("ServiceWorkflows.Reactivate")]
    public async Task<IActionResult> Reactivate(
        int branchId,
        int leafServiceId,
        int workflowId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateServiceWorkflowCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            Id = workflowId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("workflow-start-options")]
    [Permission("ServiceWorkflows.ViewStartOptions")]
    public async Task<IActionResult> GetWorkflowStartOptions(
        int branchId,
        int leafServiceId,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkflowStartOptionsQuery
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId
        };

        var result = await sender.Send(
            query,
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
