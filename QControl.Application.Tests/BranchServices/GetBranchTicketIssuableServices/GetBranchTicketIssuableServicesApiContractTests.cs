using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Controllers;
using Qcontrol.Api.Contracts.Branches;
using Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;
using Qcontrol.Application.Features.BranchBranding.Shared;
using Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;
using QControl.Api.Attribute;

namespace QControl.Application.Tests.BranchServices
    .GetBranchTicketIssuableServices;

public sealed class GetBranchTicketIssuableServicesApiContractTests
{
    [Fact]
    public async Task Controller_maps_route_branch_id_to_query()
    {
        var sender = new CapturingSender();
        var controller = new BranchesController(sender);

        var actionResult = await controller.GetTicketIssuableServices(
            25,
            CancellationToken.None);

        var query = Assert.IsType<GetBranchTicketIssuableServicesQuery>(
            sender.Request);
        Assert.Equal(25, query.BranchId);
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public void Controller_action_uses_confirmed_route_and_is_anonymous()
    {
        var method = typeof(BranchesController).GetMethod(
            nameof(BranchesController.GetTicketIssuableServices),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.Equal(
            "{branchId:int}/ticket-issuable-services",
            method!.GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Null(method.GetCustomAttribute<PermissionAttribute>());
    }

    [Fact]
    public async Task Branding_update_maps_complete_layout_contract()
    {
        var sender = new CapturingSender();
        var controller = new BranchesController(sender);

        await controller.UpdateTheme(
            25,
            new UpdateBranchThemeRequest
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
                LanguageButtonWidth = 40m,
                LanguageButtonText = "اختيار اللغة",
                ServiceButtonSpace = 2m,
                ServiceButtonFontSize = 1.8m,
                ServiceButtonText = "اختيار الخدمة",
                KeypadButtonText = "تأكيد",
                FooterButtonText = "رجوع",
                RowVersion = "AQIDBAUGBwg="
            },
            CancellationToken.None);

        var command = Assert.IsType<UpdateBranchThemeCommand>(sender.Request);
        Assert.Equal(25, command.BranchId);
        Assert.Equal("#444444", command.HeaderColor);
        Assert.True(command.ShowLanguagePage);
        Assert.True(command.AllowOperatorSelection);
        Assert.Equal(40m, command.LanguageButtonWidth);
        Assert.Equal("اختيار الخدمة", command.ServiceButtonText);
        Assert.Equal(1.8m, command.ServiceButtonFontSize);
        Assert.Equal("تأكيد", command.KeypadButtonText);
        Assert.Equal("رجوع", command.FooterButtonText);
        Assert.Equal("AQIDBAUGBwg=", command.RowVersion);
    }

    private sealed class CapturingSender : ISender
    {
        public object? Request { get; private set; }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Request = request;

            object response = request switch
            {
                GetBranchTicketIssuableServicesQuery =>
                    Result<GetBranchTicketIssuableServicesResponse>.Ok(new()
                    {
                        BranchId = 25
                    }),
                UpdateBranchThemeCommand =>
                    Result<BranchBrandingResponse>.Ok(new()
                    {
                        BranchId = 25,
                        IsConfigured = true
                    }),
                _ => throw new NotSupportedException()
            };

            return Task.FromResult((TResponse)response);
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
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(
            object request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
