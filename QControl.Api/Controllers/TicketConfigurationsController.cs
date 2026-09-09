using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QControl.Api.Attribute;
using QControl.Api.Contracts.TicketConfigurations;
using QControl.Application.Features.TicketConfigurations.Command.CreateTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Command.UpdateTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Query.GetTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Shared;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/branches/{branchId:int}/ticket-configuration")]
public sealed class TicketConfigurationsController : ControllerBase
{
    private readonly ISender _sender;

    public TicketConfigurationsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Permission("TicketConfigurations.View")]
    public async Task<IActionResult> Get(int branchId, CancellationToken cancellationToken) =>
        (await _sender.Send(new GetTicketConfigurationQuery(branchId), cancellationToken))
        .ToIActionResult();

    [HttpPost]
    [Permission("TicketConfigurations.Create")]
    public async Task<IActionResult> Create(
        int branchId,
        TicketConfigurationRequest request,
        CancellationToken cancellationToken) =>
        (await _sender.Send(new CreateTicketConfigurationCommand
        {
            BranchId = branchId,
            TicketWidthMm = request.TicketWidthMm,
            TicketHeightMm = request.TicketHeightMm,
            Elements = Map(request.Elements)
        }, cancellationToken)).ToIActionResult();

    [HttpPut]
    [Permission("TicketConfigurations.Update")]
    public async Task<IActionResult> Update(
        int branchId,
        UpdateTicketConfigurationRequest request,
        CancellationToken cancellationToken) =>
        (await _sender.Send(new UpdateTicketConfigurationCommand
        {
            BranchId = branchId,
            TicketWidthMm = request.TicketWidthMm,
            TicketHeightMm = request.TicketHeightMm,
            Elements = Map(request.Elements),
            RowVersion = request.RowVersion
        }, cancellationToken)).ToIActionResult();

    private static IReadOnlyCollection<TicketPrintElementInput> Map(
        IEnumerable<TicketPrintElementRequest> elements) =>
        elements.Select(x => new TicketPrintElementInput
        {
            ElementType = x.ElementType,
            IsVisible = x.IsVisible,
            XMm = x.XMm,
            YMm = x.YMm,
            WidthMm = x.WidthMm,
            HeightMm = x.HeightMm,
            FontSizePt = x.FontSizePt,
            FontWeight = x.FontWeight,
            TextAlign = x.TextAlign,
            Language = x.Language
        }).ToArray();
}
