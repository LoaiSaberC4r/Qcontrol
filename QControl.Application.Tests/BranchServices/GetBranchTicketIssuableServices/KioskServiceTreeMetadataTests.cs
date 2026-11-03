using System.Reflection;
using System.Text.Json;
using Qcontrol.Application.Features.BranchServices.Query.GetBranchTicketIssuableServices;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.BranchServices.GetBranchTicketIssuableServices;

public sealed class KioskServiceTreeMetadataTests
{
    [Fact]
    public async Task Discovery_returns_reservation_support_and_batched_images_for_leaves_and_parents()
    {
        var parent = EntityTestFactory.Service(1);
        var leaves = Enumerable.Range(2, 4)
            .Select(id => EntityTestFactory.Service(id, parentServiceId: 1, isTicketIssuable: true))
            .ToArray();
        foreach (var service in new[] { parent, leaves[0], leaves[2] })
            typeof(Service).GetProperty(nameof(Service.HasReservation))!.SetValue(service, true);
        var images = new[]
        {
            EntityTestFactory.ServiceImage(1, 1, "Services/1/logo.png", ServiceImageType.Logo),
            EntityTestFactory.ServiceImage(2, 2, "Media/Services/2/logo.png", ServiceImageType.Logo),
            EntityTestFactory.ServiceImage(3, 2, "Services\\2\\icon.png", ServiceImageType.Icon),
            EntityTestFactory.ServiceImage(4, 3, "Services/3/logo.png", ServiceImageType.Logo),
            EntityTestFactory.ServiceImage(5, 4, "Services/4/icon.png", ServiceImageType.Icon),
            EntityTestFactory.ServiceImage(6, 5, "Services/5/ad.png", ServiceImageType.Ads),
            EntityTestFactory.ServiceImage(7, 5, "Services/5/inactive.png", ServiceImageType.Logo),
            EntityTestFactory.ServiceImage(8, 2, "Services/2/later.png", ServiceImageType.Logo)
        };
        images[6].Deactivate(EntityTestFactory.CurrentUserId);
        var services = new[] { parent }.Concat(leaves).ToList();
        foreach (var service in services)
        {
            var storedImages = (List<ServiceImage>)typeof(Service)
                .GetField("_images", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(service)!;
            storedImages.AddRange(images.Where(x => x.ServiceId == service.Id));
        }
        var repository = new InMemoryWriteReadRepository<Service>(services);
        var handler = new GetBranchTicketIssuableServicesQueryHandler(
            new InMemoryWriteReadRepository<Branch>(new() { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<BranchService>(leaves.Select(x =>
                EntityTestFactory.BranchService(x.Id, 1, x.Id)).ToList()),
            repository,
            new InMemoryWriteReadRepository<ServiceSchedule>(leaves.Select(x =>
                EntityTestFactory.ServiceSchedule(x.Id, 1, x.Id)).ToList()),
            new InMemoryWriteReadRepository<GeneralBrand>(new()),
            new TestCurrentUser { IsAuthenticated = false, UserId = null },
            new TestBranchAccessValidator(),
            new TestDateTimeProvider { UtcNow = new DateTime(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc) });

        var result = await handler.Handle(new GetBranchTicketIssuableServicesQuery { BranchId = 1 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var root = Assert.Single(result.Value.Services);
        Assert.False(root.HasReservation);
        Assert.False(root.IsTicketIssuable);
        Assert.Equal("/Media/Services/1/logo.png", root.LogoUrl);
        Assert.Null(root.IconUrl);
        Assert.Equal(new[] { 2, 3, 4, 5 }, root.Children.Select(x => x.ServiceId));
        Assert.Equal(new[] { true, false, true, false }, root.Children.Select(x => x.HasReservation));
        var expected = new (string? Logo, string? Icon)[]
        {
            ("/Media/Services/2/logo.png", "/Media/Services/2/icon.png"),
            ("/Media/Services/3/logo.png", null),
            (null, "/Media/Services/4/icon.png"),
            (null, null)
        };
        for (var i = 0; i < leaves.Length; i++)
        {
            var node = root.Children[i];
            Assert.Equal(expected[i].Logo, node.LogoUrl);
            Assert.Equal(expected[i].Icon, node.IconUrl);
            var detailsImages = ServiceImageResponseFactory.ToImagesForResponse(
                images.Where(x => x.ServiceId == node.ServiceId), includeAds: false);
            Assert.Equal(detailsImages.LogoUrl, node.LogoUrl);
            Assert.Equal(detailsImages.IconUrl, node.IconUrl);
            Assert.Null(node.CustomInputs);
        }
        Assert.Equal(2, repository.QueryCallCount); // custom inputs + one image batch
    }

    [Fact]
    public void Discovery_serialization_keeps_existing_properties_and_adds_only_approved_metadata()
    {
        var node = new TicketIssuableServiceTreeNodeResponse { HasReservation = true };
        var json = JsonSerializer.SerializeToElement(node, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.True(json.GetProperty("hasReservation").GetBoolean());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("logoUrl").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.GetProperty("iconUrl").ValueKind);
        Assert.Equal(new[]
        {
            "arabicName", "arabicUserMessage", "children", "customInputs", "englishName",
            "englishUserMessage", "hasReservation", "iconUrl", "isClientInputRequired",
            "isTicketIssuable", "logoUrl", "rangeEndNumber", "rangePrefix", "rangeStartNumber", "serviceId"
        }, json.EnumerateObject().Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal));
    }
}
