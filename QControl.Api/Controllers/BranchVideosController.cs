using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.BranchVideos;
using Qcontrol.Application.Features.BranchVideos.Command.AddBranchVideo;
using Qcontrol.Application.Features.BranchVideos.Command.DeactivateBranchVideo;
using Qcontrol.Application.Features.BranchVideos.Command.PermanentDeleteBranchVideo;
using Qcontrol.Application.Features.BranchVideos.Command.ReactivateBranchVideo;
using Qcontrol.Application.Features.BranchVideos.Command.ReorderBranchVideos;
using Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayPlaylist;
using Qcontrol.Application.Features.BranchVideos.Query.GetBranchVideos;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/branches/{branchId:int}/display-videos")]
public sealed class BranchVideosController : ControllerBase
{
    private readonly ISender _sender;

    public BranchVideosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Permission("Branches.ViewDetails")]
    public async Task<IActionResult> Get(
        int branchId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBranchVideosQuery
        {
            BranchId = branchId,
            IsActive = isActive
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("Branches.Update")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        int branchId,
        [FromForm] AddBranchVideoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AddBranchVideoCommand
        {
            BranchId = branchId,
            Video = request.Video,
            DisplayOrder = request.DisplayOrder
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPut("order")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> Reorder(
        int branchId,
        [FromBody] ReorderBranchVideosRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReorderBranchVideosCommand
        {
            BranchId = branchId,
            Items = request.Items.Select(x => new ReorderBranchVideoItem
            {
                VideoId = x.VideoId,
                DisplayOrder = x.DisplayOrder,
                RowVersion = x.RowVersion
            }).ToList()
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPost("{videoId:int}/deactivate")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> Deactivate(
        int branchId,
        int videoId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateBranchVideoCommand
        {
            BranchId = branchId,
            VideoId = videoId,
            RowVersion = rowVersion
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPost("{videoId:int}/reactivate")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> Reactivate(
        int branchId,
        int videoId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReactivateBranchVideoCommand
        {
            BranchId = branchId,
            VideoId = videoId,
            RowVersion = rowVersion
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpDelete("{videoId:int}/permanent")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> PermanentDelete(
        int branchId,
        int videoId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PermanentDeleteBranchVideoCommand
        {
            BranchId = branchId,
            VideoId = videoId,
            RowVersion = rowVersion
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpGet("playlist")]
    [Permission("Branches.ViewDetails")]
    public async Task<IActionResult> GetPlaylist(
        int branchId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBranchDisplayPlaylistQuery
        {
            BranchId = branchId
        }, cancellationToken);
        return result.ToIActionResult();
    }
}
