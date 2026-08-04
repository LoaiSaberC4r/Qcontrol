using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.BranchAdmins;
using Qcontrol.Api.Contracts.BranchServices;
using Qcontrol.Api.Contracts.BranchServiceTrees;
using Qcontrol.Api.Contracts.Branches;
using Qcontrol.Application.Features.BranchAdmins.Command.CreateBranchAdmin;
using Qcontrol.Application.Features.BranchAdvertisements.Command.AddBranchAdvertisements;
using Qcontrol.Application.Features.BranchAdvertisements.Command.DeactivateBranchAdvertisement;
using Qcontrol.Application.Features.BranchAdvertisements.Command.DeleteBranchAdvertisement;
using Qcontrol.Application.Features.BranchAdvertisements.Command.ReactivateBranchAdvertisement;
using Qcontrol.Application.Features.BranchAdvertisements.Command.ReorderBranchAdvertisements;
using Qcontrol.Application.Features.BranchAdvertisements.Query.GetBranchAdvertisements;
using Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;
using Qcontrol.Application.Features.BranchBranding.Command.UploadBranchLogo;
using Qcontrol.Application.Features.BranchBranding.Query.GetBranchBranding;
using Qcontrol.Application.Features.BranchServices.Command.AssignBranchServices;
using Qcontrol.Application.Features.BranchServices.Query.GetBranchTicketIssuableServices;
using Qcontrol.Application.Features.BranchServices.Query.GetBranchServiceTree;
using Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceSubtree;
using Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;
using Qcontrol.Application.Features.Branches.Command.CreateBranch;
using Qcontrol.Application.Features.Branches.Command.DeactivateBranch;
using Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;
using Qcontrol.Application.Features.Branches.Command.ReactivateBranch;
using Qcontrol.Application.Features.Branches.Command.UpdateBranch;
using Qcontrol.Application.Features.Branches.Query.GetBranchDetails;
using Qcontrol.Application.Features.Branches.Query.GetBranchesPagination;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetBranchServiceGlobalizationRequests;
using QControl.Api.Attribute;
using Qcontrol.Application.Features.BranchServices.Command.UnassignBranchLeafService;

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

    [HttpGet("{branchId:int}/services/tree")]
    [Permission("BranchServices.View")]
    public async Task<IActionResult> GetAssignedServicesTree(
        int branchId,
        [FromQuery] bool includeInactive,
        [FromQuery] bool includeDeleted,
        [FromQuery] string? searchText,
        CancellationToken cancellationToken)
    {
        var query = new GetBranchServiceTreeQuery
        {
            BranchId = branchId,
            IncludeInactive = includeInactive,
            IncludeDeleted = includeDeleted,
            SearchText = searchText
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{branchId:int}/ticket-issuable-services")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTicketIssuableServices(
        int branchId,
        CancellationToken cancellationToken)
    {
        var query = new GetBranchTicketIssuableServicesQuery
        {
            BranchId = branchId
        };

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{branchId:int}/services/assign")]
    [Permission("BranchServices.Assign")]
    public async Task<IActionResult> AssignServices(
        int branchId,
        [FromBody] AssignBranchServicesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignBranchServicesCommand
        {
            BranchId = branchId,
            ServiceIds = request.ServiceIds
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{branchId:int}/service-trees")]
    [Permission("BranchServiceTrees.Create")]
    public async Task<IActionResult> CreateServiceTree(
        int branchId,
        [FromBody] CreateBranchServiceTreeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBranchServiceTreeCommand
        {
            BranchId = branchId,
            Root = request.Root is null
                ? null
                : MapTreeNode(request.Root)
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("{branchId:int}/services/{parentServiceId:int}/subtree")]
    [Permission("BranchServiceTrees.Create")]
    public async Task<IActionResult> CreateServiceSubtree(
        int branchId,
        int parentServiceId,
        [FromBody] CreateBranchServiceSubtreeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBranchServiceSubtreeCommand
        {
            BranchId = branchId,
            ParentServiceId = parentServiceId,
            Root = request.Root is null
                ? null
                : MapTreeNode(request.Root)
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpGet("{branchId:int}/service-globalization-requests")]
    [Permission("ServiceGlobalizationRequests.ViewOwn")]
    public async Task<IActionResult> GetServiceGlobalizationRequests(
        int branchId,
        [FromQuery] GetBranchServiceGlobalizationRequestsQuery query,
        CancellationToken cancellationToken)
    {
        query ??= new GetBranchServiceGlobalizationRequestsQuery();
        query.BranchId = branchId;
        query.SearchText ??= string.Empty;

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
            HeaderColor = request.HeaderColor,
            FooterColor = request.FooterColor,
            MainTextColor = request.MainTextColor,
            ShowLanguagePage = request.ShowLanguagePage,
            DefaultLanguageIsArabic = request.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = request.AlwaysRequireUserInput,
            ShowServiceNavigationPath = request.ShowServiceNavigationPath,
            AllowOperatorSelection = request.AllowOperatorSelection,
            AllowRequestMoreServices = request.AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                request.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = request.LanguageButtonTextColor,
            LanguageButtonWidth = request.LanguageButtonWidth,
            LanguageButtonHeight = request.LanguageButtonHeight,
            LanguageButtonText = request.LanguageButtonText,
            ServiceButtonBackgroundColor =
                request.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = request.ServiceButtonTextColor,
            ServiceButtonWidth = request.ServiceButtonWidth,
            ServiceButtonHeight = request.ServiceButtonHeight,
            ServiceButtonSpace = request.ServiceButtonSpace,
            ServiceButtonFontSize = request.ServiceButtonFontSize,
            ServiceButtonText = request.ServiceButtonText,
            KeypadButtonBackgroundColor =
                request.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = request.KeypadButtonTextColor,
            KeypadButtonWidth = request.KeypadButtonWidth,
            KeypadButtonHeight = request.KeypadButtonHeight,
            KeypadButtonText = request.KeypadButtonText,
            FooterButtonBackgroundColor =
                request.FooterButtonBackgroundColor,
            FooterButtonTextColor = request.FooterButtonTextColor,
            FooterButtonWidth = request.FooterButtonWidth,
            FooterButtonHeight = request.FooterButtonHeight,
            FooterButtonText = request.FooterButtonText,
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

    [HttpPost("{branchId:int}/admins")]
    [Permission("BranchAdmins.Create")]
    public async Task<IActionResult> CreateAdmin(
        int branchId,
        [FromBody] CreateBranchAdminRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBranchAdminCommand
        {
            BranchId = branchId,
            UserName = request.UserName,
            Email = request.Email,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            PhoneNumber = request.PhoneNumber,
            TemporaryPassword = request.TemporaryPassword
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

    [HttpDelete("{branchId:int}/services/{leafServiceId:int}")]
    [Permission("BranchServices.Unassign")]
    public async Task<IActionResult> UnassignLeafService(
    int branchId,
    int leafServiceId,
    CancellationToken cancellationToken)
    {
        var command = new UnassignBranchLeafServiceCommand
        {
            BranchId = branchId,
            LeafServiceId = leafServiceId
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    private static CreateBranchServiceTreeNodeCommand MapTreeNode(
        CreateBranchServiceTreeNodeRequest request)
    {
        return new CreateBranchServiceTreeNodeCommand
        {
            ArabicName = request.ArabicName,
            EnglishName = request.EnglishName,
            ServiceCode = request.ServiceCode,
            IsServiceCodeRequired = request.IsServiceCodeRequired,
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
            Children = request.Children
                .Select(MapTreeNode)
                .ToList()
        };
    }
}
