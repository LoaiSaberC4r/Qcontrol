using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Branches;
using Qcontrol.Application.Features.Branches.Command.CreateBranch;
using Qcontrol.Application.Features.Branches.Command.DeleteBranch;
using Qcontrol.Application.Features.Branches.Command.UpdateBranch;
using Qcontrol.Application.Features.Branches.Query.GetBranchDetails;
using Qcontrol.Application.Features.Branches.Query.GetBranchesPagination;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize]
public sealed class BranchesController : ControllerBase
{
    private readonly ISender sender;

    public BranchesController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("Branches.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetBranchesPaginationQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetBranchesPaginationQuery();
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{branchId:int}")]
    [Permission("Branches.ViewDetails")]
    public async Task<IActionResult> GetById(
        int branchId,
        CancellationToken cancellationToken)
    {
        var query = new GetBranchDetailsQuery
        {
            BranchId = branchId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("Branches.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBranchCommand
        {
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            IPAddress = request.IPAddress,
            License = request.License,
            Governorate = request.Location?.Governorate,
            City = request.Location?.City,
            Area = request.Location?.Area,
            Address = request.Location?.Address,
            Longitude = request.Location?.Longitude,
            Latitude = request.Location?.Latitude
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{branchId:int}")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> Update(
        int branchId,
        [FromBody] UpdateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBranchCommand
        {
            BranchId = branchId,
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            IPAddress = request.IPAddress,
            License = request.License,
            Governorate = request.Location?.Governorate,
            City = request.Location?.City,
            Area = request.Location?.Area,
            Address = request.Location?.Address,
            Longitude = request.Location?.Longitude,
            Latitude = request.Location?.Latitude
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{branchId:int}")]
    [Permission("Branches.Delete")]
    public async Task<IActionResult> Delete(
        int branchId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteBranchCommand
        {
            BranchId = branchId
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}