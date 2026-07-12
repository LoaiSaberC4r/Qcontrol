using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Services;
using Qcontrol.Application.Features.ServiceImages.Command.DeleteServiceImage;
using Qcontrol.Application.Features.ServiceImages.Command.UploadServiceAdsImages;
using Qcontrol.Application.Features.ServiceImages.Command.UploadServiceIconImage;
using Qcontrol.Application.Features.ServiceImages.Command.UploadServiceLogoImage;
using Qcontrol.Application.Features.ServiceImages.Query.GetServiceImages;
using Qcontrol.Application.Features.Services.Command.CreateService;
using Qcontrol.Application.Features.Services.Command.DeleteService;
using Qcontrol.Application.Features.Services.Command.RestoreService;
using Qcontrol.Application.Features.Services.Command.UpdateService;
using Qcontrol.Application.Features.Services.Query.GetAvailableParentServices;
using Qcontrol.Application.Features.Services.Query.GetServiceSelectionOptions;
using Qcontrol.Application.Features.Services.Query.GetServiceById;
using Qcontrol.Application.Features.Services.Query.GetServices;
using Qcontrol.Application.Features.Services.Query.GetServicesTree;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowsForService;
using Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowStartOptions;
using QControl.Api.Attribute;
using QControl.Domain.Enums;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public sealed class ServicesController : ControllerBase
{
    private readonly ISender sender;

    public ServicesController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("Services.ViewAll")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] GetServicesQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetServicesQuery();
        query.SearchText ??= string.Empty;

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("tree")]
    [Permission("Services.ViewAll")]
    public async Task<IActionResult> GetTree(
        [FromQuery] bool includeInactive,
        [FromQuery] bool includeDeleted,
        [FromQuery] ServiceScope? scope,
        [FromQuery] int? ownerBranchId,
        CancellationToken cancellationToken)
    {
        var query = new GetServicesTreeQuery
        {
            IncludeInactive = includeInactive,
            IncludeDeleted = includeDeleted,
            Scope = scope,
            OwnerBranchId = ownerBranchId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("available-parents")]
    [Permission("Services.ViewAll")]
    public async Task<IActionResult> GetAvailableParents(
        [FromQuery] int? excludeServiceId,
        [FromQuery] string? searchText,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailableParentServicesQuery
        {
            ExcludeServiceId = excludeServiceId,
            SearchText = searchText
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("selection-options")]
    [Permission("Services.ViewAll")]
    public async Task<IActionResult> GetSelectionOptions(
        [FromQuery] int? serviceId,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceSelectionOptionsQuery
        {
            ServiceId = serviceId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{serviceId:int}")]
    [Permission("Services.ViewDetails")]
    public async Task<IActionResult> GetById(
        int serviceId,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceByIdQuery
        {
            Id = serviceId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{serviceId:int}/workflows")]
    [Permission("ServiceWorkflows.ViewServiceWorkflows")]
    public async Task<IActionResult> GetWorkflowsForService(
        int serviceId,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceWorkflowsForServiceQuery
        {
            ServiceId = serviceId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{serviceId:int}/workflow-start-options")]
    [Permission("ServiceWorkflows.ViewStartOptions")]
    public async Task<IActionResult> GetWorkflowStartOptions(
        int serviceId,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkflowStartOptionsQuery
        {
            ServiceId = serviceId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("Services.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCommand
        {
            ParentServiceId = request.ParentServiceId,
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            ArabicUserMessage = request.ArabicUserMessage,
            EnglishUserMessage = request.EnglishUserMessage,
            IsTicketIssuable = request.IsTicketIssuable,
            IsClientInputRequired = request.IsClientInputRequired,
            HasReservation = request.HasReservation,
            OrderNo = request.OrderNo,
            Priority = request.Priority,
            RangePrefix = request.RangePrefix,
            RangeStartNumber = request.RangeStartNumber,
            RangeEndNumber = request.RangeEndNumber,
            WaitingDuration = request.WaitingDuration,
            NoOfTicketCopies = request.NoOfTicketCopies
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{serviceId:int}")]
    [Permission("Services.Update")]
    public async Task<IActionResult> Update(
        int serviceId,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCommand
        {
            Id = serviceId,
            ParentServiceId = request.ParentServiceId,
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            ArabicUserMessage = request.ArabicUserMessage,
            EnglishUserMessage = request.EnglishUserMessage,
            IsTicketIssuable = request.IsTicketIssuable,
            IsClientInputRequired = request.IsClientInputRequired,
            HasReservation = request.HasReservation,
            OrderNo = request.OrderNo,
            Priority = request.Priority,
            RangePrefix = request.RangePrefix,
            RangeStartNumber = request.RangeStartNumber,
            RangeEndNumber = request.RangeEndNumber,
            WaitingDuration = request.WaitingDuration,
            NoOfTicketCopies = request.NoOfTicketCopies,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{serviceId:int}/images")]
    [Permission("Services.ViewDetails")]
    public async Task<IActionResult> GetImages(
        int serviceId,
        [FromQuery] ServiceImageType? imageType,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceImagesQuery
        {
            ServiceId = serviceId,
            ImageType = imageType
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{serviceId:int}/images/logo")]
    [Permission("Services.Update")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogoImage(
        int serviceId,
        [FromForm] UploadServiceImageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UploadServiceLogoImageCommand
        {
            ServiceId = serviceId,
            Image = request.Image,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{serviceId:int}/images/icon")]
    [Permission("Services.Update")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadIconImage(
        int serviceId,
        [FromForm] UploadServiceImageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UploadServiceIconImageCommand
        {
            ServiceId = serviceId,
            Image = request.Image,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{serviceId:int}/images/ads")]
    [Permission("Services.Update")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAdsImages(
        int serviceId,
        [FromForm] UploadServiceAdsImagesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UploadServiceAdsImagesCommand
        {
            ServiceId = serviceId,
            Images = request.Images,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{serviceId:int}/images/{imageId:int}")]
    [Permission("Services.Update")]
    public async Task<IActionResult> DeleteImage(
        int serviceId,
        int imageId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeleteServiceImageCommand
        {
            ServiceId = serviceId,
            ImageId = imageId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{serviceId:int}")]
    [Permission("Services.Delete")]
    public async Task<IActionResult> Delete(
        int serviceId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeleteServiceCommand
        {
            Id = serviceId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{serviceId:int}/restore")]
    [Permission("Services.Restore")]
    public async Task<IActionResult> Restore(
        int serviceId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new RestoreServiceCommand
        {
            Id = serviceId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
