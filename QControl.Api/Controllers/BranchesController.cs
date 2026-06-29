using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Branches;
using Qcontrol.Application.Features.BranchAdvertisements.Command.AddBranchAdvertisements;
using Qcontrol.Application.Features.BranchAdvertisements.Command.DeactivateBranchAdvertisement;
using Qcontrol.Application.Features.BranchAdvertisements.Command.DeleteBranchAdvertisement;
using Qcontrol.Application.Features.BranchAdvertisements.Command.ReactivateBranchAdvertisement;
using Qcontrol.Application.Features.BranchAdvertisements.Command.ReorderBranchAdvertisements;
using Qcontrol.Application.Features.BranchAdvertisements.Query.GetBranchAdvertisements;
using Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;
using Qcontrol.Application.Features.BranchBranding.Command.UploadBranchLogo;
using Qcontrol.Application.Features.BranchBranding.Query.GetBranchBranding;
using Qcontrol.Application.Features.Branches.Command.CreateBranch;
using Qcontrol.Application.Features.Branches.Command.DeactivateBranch;
using Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;
using Qcontrol.Application.Features.Branches.Command.ReactivateBranch;
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

    [HttpGet("{branchId:int}/branding")]
    [Permission("Branches.ViewDetails")]
    public async Task<IActionResult> GetBranding(
        int branchId,
        CancellationToken cancellationToken)
    {
        var query = new GetBranchBrandingQuery
        {
            BranchId = branchId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{branchId:int}/branding/theme")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> UpdateTheme(
        int branchId,
        [FromBody] UpdateBranchThemeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBranchThemeCommand
        {
            BranchId = branchId,
            MainColor = request.MainColor,
            SecondaryColor = request.SecondaryColor,
            BackgroundColor = request.BackgroundColor,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{branchId:int}/branding/logo")]
    [Permission("Branches.Update")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo(
        int branchId,
        [FromForm] UploadBranchLogoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UploadBranchLogoCommand
        {
            BranchId = branchId,
            Logo = request.Logo,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{branchId:int}/advertisements")]
    [Permission("Branches.ViewDetails")]
    public async Task<IActionResult> GetAdvertisements(
        int branchId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = new GetBranchAdvertisementsQuery
        {
            BranchId = branchId,
            IsActive = isActive
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{branchId:int}/advertisements")]
    [Permission("Branches.Update")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddAdvertisements(
        int branchId,
        [FromForm] AddBranchAdvertisementsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddBranchAdvertisementsCommand
        {
            BranchId = branchId,
            Images = request.Images
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut("{branchId:int}/advertisements/order")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> ReorderAdvertisements(
        int branchId,
        [FromBody] ReorderBranchAdvertisementsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderBranchAdvertisementsCommand
        {
            BranchId = branchId,
            Items = request.Items
                .Select(item => new ReorderBranchAdvertisementItem
                {
                    AdvertisementId = item.AdvertisementId,
                    DisplayOrder = item.DisplayOrder,
                    RowVersion = item.RowVersion
                })
                .ToList()
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{branchId:int}/advertisements/{advertisementId:int}/deactivate")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> DeactivateAdvertisement(
        int branchId,
        int advertisementId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateBranchAdvertisementCommand
        {
            BranchId = branchId,
            AdvertisementId = advertisementId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{branchId:int}/advertisements/{advertisementId:int}/reactivate")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> ReactivateAdvertisement(
        int branchId,
        int advertisementId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateBranchAdvertisementCommand
        {
            BranchId = branchId,
            AdvertisementId = advertisementId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{branchId:int}/advertisements/{advertisementId:int}")]
    [Permission("Branches.Update")]
    public async Task<IActionResult> DeleteAdvertisement(
        int branchId,
        int advertisementId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeleteBranchAdvertisementCommand
        {
            BranchId = branchId,
            AdvertisementId = advertisementId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
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
            Governorate = request.Location.Governorate,
            City = request.Location.City,
            Area = request.Location.Area,
            Address = request.Location.Address,
            Longitude = request.Location.Longitude,
            Latitude = request.Location.Latitude
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
            Governorate = request.Location.Governorate,
            City = request.Location.City,
            Area = request.Location.Area,
            Address = request.Location.Address,
            Longitude = request.Location.Longitude,
            Latitude = request.Location.Latitude,
            RowVersion = request.RowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{branchId:int}/deactivate")]
    [Permission("Branches.Deactivate")]
    public async Task<IActionResult> Deactivate(
        int branchId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateBranchCommand
        {
            BranchId = branchId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{branchId:int}/reactivate")]
    [Permission("Branches.Reactivate")]
    public async Task<IActionResult> Reactivate(
        int branchId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateBranchCommand
        {
            BranchId = branchId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpDelete("{branchId:int}/permanent")]
    [Permission("Branches.DeletePermanent")]
    public async Task<IActionResult> DeletePermanently(
        int branchId,
        [FromHeader(Name = "If-Match")] string rowVersion,
        CancellationToken cancellationToken)
    {
        var command = new PermanentDeleteBranchCommand
        {
            BranchId = branchId,
            RowVersion = rowVersion
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
