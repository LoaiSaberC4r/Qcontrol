using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.ServiceSchedules;
using Qcontrol.Application.Features.ServiceSchedules.Command.CreateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Command.UpdateServiceSchedule;
using Qcontrol.Application.Features.ServiceSchedules.Query.GetServiceSchedule;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/branches/{branchId:int}/services/{leafServiceId:int}/schedule")]
public sealed class BranchServiceSchedulesController : ControllerBase
{
    private readonly ISender sender;

    public BranchServiceSchedulesController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("ServiceSchedules.View")]
    public async Task<IActionResult> Get(
        int branchId,
        int leafServiceId,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceScheduleQuery
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("ServiceSchedules.Create")]
    public async Task<IActionResult> Create(
        int branchId,
        int leafServiceId,
        [FromBody] CreateServiceScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceScheduleCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            WorkDays = request.WorkDays,
            IsSlotCodeRequired = request.IsSlotCodeRequired,
            SlotCode = request.SlotCode
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut]
    [Permission("ServiceSchedules.Update")]
    public async Task<IActionResult> Update(
        int branchId,
        int leafServiceId,
        [FromBody] UpdateServiceScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceScheduleCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            WorkDays = request.WorkDays,
            IsSlotCodeRequired = request.IsSlotCodeRequired,
            SlotCode = request.SlotCode,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
