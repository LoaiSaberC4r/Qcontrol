using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.BranchConfigurations;
using Qcontrol.Application.Features.BranchConfigurations.Command.CreateBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Query.GetBranchConfiguration;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/branches/{branchId:int}/configuration")]
public sealed class BranchConfigurationsController : ControllerBase
{
    private readonly ISender sender;

    public BranchConfigurationsController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("BranchConfigurations.View")]
    public async Task<IActionResult> Get(
        int branchId,
        CancellationToken cancellationToken)
    {
        var query = new GetBranchConfigurationQuery
        {
            BranchId = branchId
        };

        var result = await sender.Send(query, cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("BranchConfigurations.Create")]
    public async Task<IActionResult> Create(
        int branchId,
        [FromBody] CreateBranchConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBranchConfigurationCommand
        {
            BranchId = branchId,
            AllowedTime = request.AllowedTime
        };

        var result = await sender.Send(command, cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut]
    [Permission("BranchConfigurations.Update")]
    public async Task<IActionResult> Update(
        int branchId,
        [FromBody] UpdateBranchConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBranchConfigurationCommand
        {
            BranchId = branchId,
            AllowedTime = request.AllowedTime,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(command, cancellationToken);

        return result.ToIActionResult();
    }
}
