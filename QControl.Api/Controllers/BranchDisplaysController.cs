using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.BranchDisplays;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Command.CreateBranchDisplayConfiguration;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Command.UpdateBranchDisplayConfiguration;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayConfiguration;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayRuntimeConfiguration;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.CreateBranchDisplayMessage;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.DeactivateBranchDisplayMessage;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.ReactivateBranchDisplayMessage;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.ReorderBranchDisplayMessages;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.UpdateBranchDisplayMessage;
using Qcontrol.Application.Features.BranchDisplayMessages.Query.GetBranchDisplayMessages;
using Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayRuntimePlaylist;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/branches/{branchId:int}")]
public sealed class BranchDisplaysController : ControllerBase
{
    private readonly ISender _sender;

    public BranchDisplaysController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("display-configuration")]
    [Permission("Branches.ViewDetails")]
    public async Task<IActionResult> GetConfiguration(
        int branchId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetBranchDisplayConfigurationQuery { BranchId = branchId },
            cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPost("display-configuration")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> CreateConfiguration(
        int branchId,
        [FromBody] CreateBranchDisplayConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(MapCreateConfiguration(branchId, request), cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPut("display-configuration")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> UpdateConfiguration(
        int branchId,
        [FromBody] UpdateBranchDisplayConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(MapUpdateConfiguration(branchId, request), cancellationToken);
        return result.ToIActionResult();
    }

    [HttpGet("display-messages")]
    [Permission("Branches.ViewDetails")]
    public async Task<IActionResult> GetMessages(
        int branchId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetBranchDisplayMessagesQuery { BranchId = branchId },
            cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPost("display-messages")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> CreateMessage(
        int branchId,
        [FromBody] CreateBranchDisplayMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateBranchDisplayMessageCommand
        {
            BranchId = branchId,
            TextAr = request.TextAr,
            TextEn = request.TextEn,
            DisplayOrder = request.DisplayOrder
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPut("display-messages/{messageId:int}")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> UpdateMessage(
        int branchId,
        int messageId,
        [FromBody] UpdateBranchDisplayMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateBranchDisplayMessageCommand
        {
            BranchId = branchId,
            MessageId = messageId,
            TextAr = request.TextAr,
            TextEn = request.TextEn,
            DisplayOrder = request.DisplayOrder,
            RowVersion = request.RowVersion
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPut("display-messages/order")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> ReorderMessages(
        int branchId,
        [FromBody] ReorderBranchDisplayMessagesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReorderBranchDisplayMessagesCommand
        {
            BranchId = branchId,
            Items = request.Items.Select(x => new ReorderBranchDisplayMessageItem
            {
                MessageId = x.MessageId,
                DisplayOrder = x.DisplayOrder,
                RowVersion = x.RowVersion
            }).ToList()
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPost("display-messages/{messageId:int}/deactivate")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> DeactivateMessage(
        int branchId,
        int messageId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateBranchDisplayMessageCommand
        {
            BranchId = branchId,
            MessageId = messageId,
            RowVersion = rowVersion
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpPost("display-messages/{messageId:int}/reactivate")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> ReactivateMessage(
        int branchId,
        int messageId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReactivateBranchDisplayMessageCommand
        {
            BranchId = branchId,
            MessageId = messageId,
            RowVersion = rowVersion
        }, cancellationToken);
        return result.ToIActionResult();
    }

    [HttpGet("display-runtime-configuration")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRuntimeConfiguration(
        int branchId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetBranchDisplayRuntimeConfigurationQuery { BranchId = branchId },
            cancellationToken);
        return result.ToIActionResult();
    }

    [HttpGet("display-runtime-playlist")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRuntimePlaylist(
        int branchId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetBranchDisplayRuntimePlaylistQuery { BranchId = branchId },
            cancellationToken);
        return result.ToIActionResult();
    }

    private static CreateBranchDisplayConfigurationCommand MapCreateConfiguration(
        int branchId,
        BranchDisplayConfigurationRequestBase request) =>
        new()
        {
            BranchId = branchId,
            DisplayBackgroundColor = request.DisplayBackgroundColor,
            MainTitleAr = request.MainTitleAr,
            MainTitleEn = request.MainTitleEn,
            HeaderBackgroundColor = request.HeaderBackgroundColor,
            MainTitleTextColor = request.MainTitleTextColor,
            MainTitleFontSize = request.MainTitleFontSize,
            TableHeaderBackgroundColor = request.TableHeaderBackgroundColor,
            TableHeaderTextColor = request.TableHeaderTextColor,
            TableRowBackgroundColor = request.TableRowBackgroundColor,
            TableRowTextColor = request.TableRowTextColor,
            TicketNumberBackgroundColor = request.TicketNumberBackgroundColor,
            TicketNumberTextColor = request.TicketNumberTextColor,
            TicketColumnTitleAr = request.TicketColumnTitleAr,
            TicketColumnTitleEn = request.TicketColumnTitleEn,
            ServiceColumnTitleAr = request.ServiceColumnTitleAr,
            ServiceColumnTitleEn = request.ServiceColumnTitleEn,
            WindowColumnTitleAr = request.WindowColumnTitleAr,
            WindowColumnTitleEn = request.WindowColumnTitleEn,
            TickerBackgroundColor = request.TickerBackgroundColor,
            TickerTextColor = request.TickerTextColor,
            TickerFontSize = request.TickerFontSize,
            ShowClock = request.ShowClock,
            ClockBackgroundColor = request.ClockBackgroundColor,
            ClockTextColor = request.ClockTextColor
        };

    private static UpdateBranchDisplayConfigurationCommand MapUpdateConfiguration(
        int branchId,
        UpdateBranchDisplayConfigurationRequest request) =>
        new()
        {
            BranchId = branchId,
            DisplayBackgroundColor = request.DisplayBackgroundColor,
            MainTitleAr = request.MainTitleAr,
            MainTitleEn = request.MainTitleEn,
            HeaderBackgroundColor = request.HeaderBackgroundColor,
            MainTitleTextColor = request.MainTitleTextColor,
            MainTitleFontSize = request.MainTitleFontSize,
            TableHeaderBackgroundColor = request.TableHeaderBackgroundColor,
            TableHeaderTextColor = request.TableHeaderTextColor,
            TableRowBackgroundColor = request.TableRowBackgroundColor,
            TableRowTextColor = request.TableRowTextColor,
            TicketNumberBackgroundColor = request.TicketNumberBackgroundColor,
            TicketNumberTextColor = request.TicketNumberTextColor,
            TicketColumnTitleAr = request.TicketColumnTitleAr,
            TicketColumnTitleEn = request.TicketColumnTitleEn,
            ServiceColumnTitleAr = request.ServiceColumnTitleAr,
            ServiceColumnTitleEn = request.ServiceColumnTitleEn,
            WindowColumnTitleAr = request.WindowColumnTitleAr,
            WindowColumnTitleEn = request.WindowColumnTitleEn,
            TickerBackgroundColor = request.TickerBackgroundColor,
            TickerTextColor = request.TickerTextColor,
            TickerFontSize = request.TickerFontSize,
            ShowClock = request.ShowClock,
            ClockBackgroundColor = request.ClockBackgroundColor,
            ClockTextColor = request.ClockTextColor,
            RowVersion = request.RowVersion
        };
}
