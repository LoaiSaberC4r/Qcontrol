using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Qcontrol.Api.Controllers;
using Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayRuntimePlaylist;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Application.Features.BranchBranding.Command.UploadBranchLogo;
using QControl.Api.Attribute;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.BranchDisplays;

public sealed class BranchDisplaysControllerAndPlaylistTests
{
    [Fact]
    public void Controller_exposes_eleven_routes_with_only_runtime_reads_anonymous()
    {
        var type = typeof(BranchDisplaysController);
        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal("api/branches/{branchId:int}", type.GetCustomAttribute<RouteAttribute>()?.Template);

        AssertAction(nameof(BranchDisplaysController.GetConfiguration), typeof(HttpGetAttribute), "display-configuration", false, "PERMISSION_Branches.ViewDetails");
        AssertAction(nameof(BranchDisplaysController.CreateConfiguration), typeof(HttpPostAttribute), "display-configuration", false, "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchDisplaysController.UpdateConfiguration), typeof(HttpPutAttribute), "display-configuration", false, "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchDisplaysController.GetMessages), typeof(HttpGetAttribute), "display-messages", false, "PERMISSION_Branches.ViewDetails");
        AssertAction(nameof(BranchDisplaysController.CreateMessage), typeof(HttpPostAttribute), "display-messages", false, "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchDisplaysController.UpdateMessage), typeof(HttpPutAttribute), "display-messages/{messageId:int}", false, "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchDisplaysController.ReorderMessages), typeof(HttpPutAttribute), "display-messages/order", false, "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchDisplaysController.DeactivateMessage), typeof(HttpPostAttribute), "display-messages/{messageId:int}/deactivate", false, "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchDisplaysController.ReactivateMessage), typeof(HttpPostAttribute), "display-messages/{messageId:int}/reactivate", false, "PERMISSION_Branches.Update");
        AssertAction(nameof(BranchDisplaysController.GetRuntimeConfiguration), typeof(HttpGetAttribute), "display-runtime-configuration", true, null);
        AssertAction(nameof(BranchDisplaysController.GetRuntimePlaylist), typeof(HttpGetAttribute), "display-runtime-playlist", true, null);

        Assert.DoesNotContain(
            type.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.GetCustomAttribute<HttpDeleteAttribute>() is not null);
    }

    [Fact]
    public async Task Anonymous_runtime_playlist_reuses_ready_active_hls_filter_and_order()
    {
        var videos = new List<BranchVideo>
        {
            EntityTestFactory.BranchVideo(5, 1, 3, processingStatus: BranchVideoProcessingStatus.Ready),
            EntityTestFactory.BranchVideo(1, 1, 1, processingStatus: BranchVideoProcessingStatus.Ready),
            EntityTestFactory.BranchVideo(2, 1, 2, isActive: false, processingStatus: BranchVideoProcessingStatus.Ready),
            EntityTestFactory.BranchVideo(4, 1, 4, processingStatus: BranchVideoProcessingStatus.Processing),
            EntityTestFactory.BranchVideo(8, 2, 1, processingStatus: BranchVideoProcessingStatus.Ready)
        };
        var handler = new GetBranchDisplayRuntimePlaylistQueryHandler(
            new InMemoryWriteReadRepository<Branch>(new() { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<BranchVideo>(videos),
            new TestCacheService());

        var result = await handler.Handle(
            new GetBranchDisplayRuntimePlaylistQuery { BranchId = 1 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 5 }, result.Value.Videos.Select(x => x.VideoId));
        Assert.All(result.Value.Videos, video => Assert.EndsWith(".m3u8", video.StreamUrl));
    }

    [Fact]
    public void Runtime_configuration_cache_shares_the_branch_branding_invalidation_tag()
    {
        var runtimeTags = BranchDisplayCacheTags.ForBranch(7);
        var logoMutationTags = new UploadBranchLogoCommand { BranchId = 7 }.Tags;

        Assert.Contains("branch:7:branding", runtimeTags);
        Assert.Contains("branch:7:branding", logoMutationTags);
    }

    private static void AssertAction(
        string methodName,
        Type attributeType,
        string template,
        bool allowAnonymous,
        string? permission)
    {
        var method = typeof(BranchDisplaysController).GetMethod(methodName)!;
        var http = (HttpMethodAttribute)method.GetCustomAttribute(attributeType)!;
        Assert.Equal(template, http.Template);
        Assert.Equal(allowAnonymous, method.GetCustomAttribute<AllowAnonymousAttribute>() is not null);
        Assert.Equal(permission, method.GetCustomAttribute<PermissionAttribute>()?.Policy);
    }
}
