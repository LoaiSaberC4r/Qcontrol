using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.GeneralBrand;
using Qcontrol.Api.Controllers;
using Qcontrol.Application.Features.GeneralBrand.Command.CreateGeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Query.GetGeneralBrand;
using Qcontrol.Application.Features.GeneralBrand.Shared;
using QControl.Api.Attribute;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.GeneralBranding;

public sealed class GeneralBrandControllerTests
{
    [Fact]
    public void Controller_has_confirmed_route_and_authorize_attribute()
    {
        var controllerType = typeof(GeneralBrandController);

        Assert.Equal(
            "api/general-brand",
            controllerType.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(
            controllerType.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(
            controllerType.GetCustomAttribute<ApiControllerAttribute>());
    }

    [Theory]
    [InlineData(nameof(GeneralBrandController.Get), typeof(HttpGetAttribute),
        "PERMISSION_GeneralBrand.View")]
    [InlineData(nameof(GeneralBrandController.Create), typeof(HttpPostAttribute),
        "PERMISSION_GeneralBrand.Create")]
    [InlineData(nameof(GeneralBrandController.Update), typeof(HttpPutAttribute),
        "PERMISSION_GeneralBrand.Update")]
    public void Actions_have_confirmed_http_methods_and_permissions(
        string methodName,
        Type httpAttributeType,
        string expectedPolicy)
    {
        var method = typeof(GeneralBrandController).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute(httpAttributeType));
        Assert.Equal(
            expectedPolicy,
            method.GetCustomAttribute<PermissionAttribute>()?.Policy);
    }

    [Fact]
    public async Task Get_dispatches_query_and_returns_via_result_mapping()
    {
        var sender = new CapturingSender();
        var controller = new GeneralBrandController(sender);

        var actionResult = await controller.Get(CancellationToken.None);

        Assert.IsType<GetGeneralBrandQuery>(sender.Request);
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public async Task Post_dispatches_complete_create_command()
    {
        var sender = new CapturingSender();
        var controller = new GeneralBrandController(sender);

        var actionResult = await controller.Create(
            CreateRequest(),
            CancellationToken.None);

        var command = Assert.IsType<CreateGeneralBrandCommand>(sender.Request);
        Assert.Equal("#111111", command.MainColor);
        Assert.Equal("#444444", command.HeaderColor);
        Assert.True(command.AlwaysRequireUserInput);
        Assert.Equal(2m, command.ServiceButtonSpace);
        Assert.Equal("Back", command.FooterButtonText);
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public async Task Put_dispatches_complete_update_command()
    {
        var sender = new CapturingSender();
        var controller = new GeneralBrandController(sender);
        var request = UpdateRequest();

        var actionResult = await controller.Update(
            request,
            CancellationToken.None);

        var command = Assert.IsType<UpdateGeneralBrandCommand>(sender.Request);
        Assert.Equal("#111111", command.MainColor);
        Assert.Equal("#444444", command.HeaderColor);
        Assert.True(command.AlwaysRequireUserInput);
        Assert.Equal(2m, command.ServiceButtonSpace);
        Assert.Equal("Back", command.FooterButtonText);
        Assert.Equal(request.RowVersion, command.RowVersion);
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public async Task Result_failures_are_returned_through_ToIActionResult()
    {
        var sender = new CapturingSender
        {
            Result = Result<GeneralBrandResponse>.Fail(new Error(
                "GeneralBrand.NotConfigured",
                "Not configured",
                ErrorType.NotFound))
        };
        var controller = new GeneralBrandController(sender);

        var actionResult = await controller.Get(CancellationToken.None);

        Assert.Equal("ProblemActionResult", actionResult.GetType().Name);
    }

    [Fact]
    public void Permission_catalog_contains_confirmed_permissions()
    {
        var infrastructureAssembly =
            typeof(PlatformWriteDbContext).Assembly;
        var catalogType = infrastructureAssembly.GetType(
            "Qcontrol.infrastructure.Seeders.SeedConstants+SeedCatalog",
            throwOnError: true)!;
        var permissions = Assert.IsAssignableFrom<IEnumerable>(
            catalogType.GetProperty(
                "Permissions",
                BindingFlags.Public | BindingFlags.Static)!
                .GetValue(null));
        var names = permissions
            .Cast<object>()
            .Select(item => (string)item.GetType()
                .GetProperty("Name")!
                .GetValue(item)!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GeneralBrand.View", names);
        Assert.Contains("GeneralBrand.Create", names);
        Assert.Contains("GeneralBrand.Update", names);
    }

    private static CreateGeneralBrandRequest CreateRequest() =>
        new()
        {
            MainColor = "#111111",
            SecondaryColor = "#222222",
            BackgroundColor = "#333333",
            HeaderColor = "#444444",
            FooterColor = "#555555",
            MainTextColor = "#666666",
            ShowLanguagePage = true,
            DefaultLanguageIsArabic = true,
            AlwaysRequireUserInput = true,
            ShowServiceNavigationPath = true,
            AllowOperatorSelection = true,
            AllowRequestMoreServices = true,
            LanguageButtonBackgroundColor = "#777777",
            LanguageButtonTextColor = "#888888",
            LanguageButtonWidth = 40m,
            LanguageButtonHeight = 22m,
            LanguageButtonText = "Language",
            ServiceButtonBackgroundColor = "#999999",
            ServiceButtonTextColor = "#AAAAAA",
            ServiceButtonWidth = 40m,
            ServiceButtonHeight = 20m,
            ServiceButtonSpace = 2m,
            ServiceButtonFontSize = 1.8m,
            ServiceButtonText = "Service",
            KeypadButtonBackgroundColor = "#BBBBBB",
            KeypadButtonTextColor = "#CCCCCC",
            KeypadButtonWidth = 45m,
            KeypadButtonHeight = 10m,
            KeypadButtonText = "Confirm",
            FooterButtonBackgroundColor = "#DDDDDD",
            FooterButtonTextColor = "#EEEEEE",
            FooterButtonWidth = 10m,
            FooterButtonHeight = 12m,
            FooterButtonText = "Back"
        };

    private static UpdateGeneralBrandRequest UpdateRequest()
    {
        var create = CreateRequest();

        return new UpdateGeneralBrandRequest
        {
            MainColor = create.MainColor,
            SecondaryColor = create.SecondaryColor,
            BackgroundColor = create.BackgroundColor,
            HeaderColor = create.HeaderColor,
            FooterColor = create.FooterColor,
            MainTextColor = create.MainTextColor,
            ShowLanguagePage = create.ShowLanguagePage,
            DefaultLanguageIsArabic = create.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = create.AlwaysRequireUserInput,
            ShowServiceNavigationPath = create.ShowServiceNavigationPath,
            AllowOperatorSelection = create.AllowOperatorSelection,
            AllowRequestMoreServices = create.AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                create.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = create.LanguageButtonTextColor,
            LanguageButtonWidth = create.LanguageButtonWidth,
            LanguageButtonHeight = create.LanguageButtonHeight,
            LanguageButtonText = create.LanguageButtonText,
            ServiceButtonBackgroundColor =
                create.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = create.ServiceButtonTextColor,
            ServiceButtonWidth = create.ServiceButtonWidth,
            ServiceButtonHeight = create.ServiceButtonHeight,
            ServiceButtonSpace = create.ServiceButtonSpace,
            ServiceButtonFontSize = create.ServiceButtonFontSize,
            ServiceButtonText = create.ServiceButtonText,
            KeypadButtonBackgroundColor =
                create.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = create.KeypadButtonTextColor,
            KeypadButtonWidth = create.KeypadButtonWidth,
            KeypadButtonHeight = create.KeypadButtonHeight,
            KeypadButtonText = create.KeypadButtonText,
            FooterButtonBackgroundColor =
                create.FooterButtonBackgroundColor,
            FooterButtonTextColor = create.FooterButtonTextColor,
            FooterButtonWidth = create.FooterButtonWidth,
            FooterButtonHeight = create.FooterButtonHeight,
            FooterButtonText = create.FooterButtonText,
            RowVersion = "AQIDBAUGBwg="
        };
    }

    private sealed class CapturingSender : ISender
    {
        public object? Request { get; private set; }

        public Result<GeneralBrandResponse> Result { get; init; } =
            Result<GeneralBrandResponse>.Ok(new GeneralBrandResponse
            {
                Id = 1,
                IsConfigured = true
            });

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult((TResponse)(object)Result);
        }

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest =>
            throw new NotSupportedException();

        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            [EnumeratorCancellation] CancellationToken cancellationToken =
                default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(
            object request,
            [EnumeratorCancellation] CancellationToken cancellationToken =
                default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
