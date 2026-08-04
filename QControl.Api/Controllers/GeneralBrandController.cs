using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.GeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Command.CreateGeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Query.GetGeneralBrand;
using QControl.Api.Attribute;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/general-brand")]
[Authorize]
public sealed class GeneralBrandController : ControllerBase
{
    private readonly ISender sender;

    public GeneralBrandController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpGet]
    [Permission("GeneralBrand.View")]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetGeneralBrandQuery(),
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost]
    [Permission("GeneralBrand.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateGeneralBrandRequest request,
        CancellationToken cancellationToken)
    {
        var command = MapCreateCommand(request);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPut]
    [Permission("GeneralBrand.Update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateGeneralBrandRequest request,
        CancellationToken cancellationToken)
    {
        var command = MapUpdateCommand(request);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    private static CreateGeneralBrandCommand MapCreateCommand(
        CreateGeneralBrandRequest request) =>
        new()
        {
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
            FooterButtonText = request.FooterButtonText
        };

    private static UpdateGeneralBrandCommand MapUpdateCommand(
        UpdateGeneralBrandRequest request) =>
        new()
        {
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
}
