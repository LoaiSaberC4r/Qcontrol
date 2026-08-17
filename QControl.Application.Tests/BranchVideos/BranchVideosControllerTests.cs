using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Qcontrol.Api.Contracts.BranchVideos;
using Qcontrol.Api.Controllers;
using Qcontrol.Application.Features.BranchVideos.Command.AddBranchVideo;
using Qcontrol.Application.Features.BranchVideos.Command.ReorderBranchVideos;
using Qcontrol.Application.Features.BranchVideos.Shared;
using QControl.Api.Attribute;

namespace QControl.Application.Tests.BranchVideos;

public sealed class BranchVideosControllerTests
{
    [Fact]
    public void Controller_and_actions_expose_only_the_confirmed_routes_and_permissions()
    {
        var type = typeof(BranchVideosController);
        Assert.Equal(
            "api/branches/{branchId:int}/display-videos",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());

        AssertAction(nameof(BranchVideosController.Get), typeof(HttpGetAttribute), null, "PERMISSION_Branches.ViewDetails");
        AssertAction(nameof(BranchVideosController.Upload), typeof(HttpPostAttribute), null, "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchVideosController.Reorder), typeof(HttpPutAttribute), "order", "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchVideosController.Deactivate), typeof(HttpPostAttribute), "{videoId:int}/deactivate", "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchVideosController.Reactivate), typeof(HttpPostAttribute), "{videoId:int}/reactivate", "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchVideosController.PermanentDelete), typeof(HttpDeleteAttribute), "{videoId:int}/permanent", "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchVideosController.GetPlaylist), typeof(HttpGetAttribute), "playlist", "PERMISSION_Branches.ViewDetails");

        Assert.Equal(
            "multipart/form-data",
            type.GetMethod(nameof(BranchVideosController.Upload))!
                .GetCustomAttribute<ConsumesAttribute>()!
                .ContentTypes.Single());
    }

    [Theory]
    [InlineData(nameof(BranchVideosController.Deactivate))]
    [InlineData(nameof(BranchVideosController.Reactivate))]
    [InlineData(nameof(BranchVideosController.PermanentDelete))]
    public void Lifecycle_routes_bind_concurrency_from_if_match(string methodName)
    {
        var parameter = typeof(BranchVideosController)
            .GetMethod(methodName)!
            .GetParameters()
            .Single(x => x.Name == "rowVersion");

        Assert.Equal(
            "If-Match",
            parameter.GetCustomAttribute<FromHeaderAttribute>()?.Name);
    }

    [Fact]
    public async Task Upload_maps_branch_form_file_and_display_order()
    {
        var sender = new CapturingSender
        {
            Response = Result<BranchVideoResponse>.Ok(new BranchVideoResponse())
        };
        var controller = new BranchVideosController(sender);
        var file = FormVideo();

        await controller.Upload(
            17,
            new AddBranchVideoRequest { Video = file, DisplayOrder = 4 },
            CancellationToken.None);

        var command = Assert.IsType<AddBranchVideoCommand>(sender.Request);
        Assert.Equal(17, command.BranchId);
        Assert.Same(file, command.Video);
        Assert.Equal(4, command.DisplayOrder);
    }

    [Fact]
    public async Task Reorder_maps_all_item_concurrency_tokens()
    {
        var sender = new CapturingSender
        {
            Response = Result<IReadOnlyList<BranchVideoResponse>>.Ok(
                Array.Empty<BranchVideoResponse>())
        };
        var controller = new BranchVideosController(sender);

        await controller.Reorder(
            17,
            new ReorderBranchVideosRequest
            {
                Items = new()
                {
                    new ReorderBranchVideoItemRequest
                    {
                        VideoId = 9,
                        DisplayOrder = 2,
                        RowVersion = "AQIDBAUGBwg="
                    }
                }
            },
            CancellationToken.None);

        var command = Assert.IsType<ReorderBranchVideosCommand>(sender.Request);
        var item = Assert.Single(command.Items);
        Assert.Equal(17, command.BranchId);
        Assert.Equal(9, item.VideoId);
        Assert.Equal(2, item.DisplayOrder);
        Assert.Equal("AQIDBAUGBwg=", item.RowVersion);
    }

    private static void AssertAction(
        string methodName,
        Type attributeType,
        string? template,
        string permission)
    {
        var method = typeof(BranchVideosController).GetMethod(methodName)!;
        var httpAttribute = (HttpMethodAttribute)method.GetCustomAttribute(attributeType)!;
        Assert.Equal(template, httpAttribute.Template);
        Assert.Equal(permission, method.GetCustomAttribute<PermissionAttribute>()?.Policy);
    }

    private static IFormFile FormVideo()
    {
        var stream = new MemoryStream(new byte[] { 1 });
        return new FormFile(stream, 0, 1, "Video", "video.mp4")
        {
            Headers = new HeaderDictionary(),
            ContentType = "video/mp4"
        };
    }

    private sealed class CapturingSender : ISender
    {
        public object? Request { get; private set; }
        public object Response { get; init; } = null!;

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult((TResponse)Response);
        }
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest => throw new NotSupportedException();
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
