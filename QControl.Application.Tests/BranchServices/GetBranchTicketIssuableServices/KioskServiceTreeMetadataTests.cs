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
    public async Task Discovery_batches_images_and_scoped_historical_inputs_for_eligible_reservation_leaves()
    {
        var parent = EntityTestFactory.Service(1);
        var leaves = Enumerable.Range(2, 4)
            .Select(id => EntityTestFactory.Service(id, parentServiceId: 1, isTicketIssuable: true))
            .ToArray();
        foreach (var service in new[] { parent, leaves[0], leaves[2] })
            typeof(Service).GetProperty(nameof(Service.HasReservation))!.SetValue(service, true);
        var current = EntityTestFactory.ServiceCustomInput(leaves[0], 101, "Current", isRequired: true);
        typeof(Service).GetProperty(nameof(Service.IsClientInputRequired))!.SetValue(leaves[0], true);
        var now = new DateTime(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc);
        var reservationRepository = new InMemoryWriteReadRepository<Reservation>(new()
        {
            HistoricalReservation(1, 2, 102, 20, "Tie lost", now),
            HistoricalReservation(1, 2, 102, 21, "Latest", now),
            HistoricalReservation(1, 2, 102, 22, "Earlier", now.AddDays(-1)),
            HistoricalReservation(1, 2, current.Id, 23, "Superseded by current", now),
            HistoricalReservation(1, 2, null, 24, "Unknown", now),
            HistoricalReservation(2, 2, 103, 25, "Other branch", now),
            HistoricalReservation(1, 1, 104, 26, "Structural parent", now),
            HistoricalReservation(1, 3, 105, 27, "No reservation support", now),
            HistoricalReservation(1, 4, 106, 28, "Different eligible leaf", now),
            HistoricalReservation(1, 99, 107, 29, "Outside eligible services", now)
        });

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
            reservationRepository,
            new TestCurrentUser { IsAuthenticated = false, UserId = null },
            new TestBranchAccessValidator(),
            new TestDateTimeProvider { UtcNow = now });

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
        }
        var currentField = Assert.Single(root.Children[0].CustomInputs!);
        Assert.Equal(current.Id, currentField.CustomInputId);
        Assert.True(currentField.IsRequired);
        var historical = Assert.Single(root.Children[0].HistoricalSearchInputs!);
        Assert.Equal(102, historical.CustomInputId);
        Assert.Equal("Latest", historical.Name);
        Assert.Equal("Historical label", historical.LabelEn);
        Assert.Equal("التسمية", historical.LabelAr);
        Assert.Equal(ServiceCustomInputType.Integer, historical.Type);
        Assert.Equal(7, historical.Order);
        Assert.False(historical.IsRequired);
        Assert.Null(historical.MinLength);
        Assert.Null(historical.MaxLength);
        Assert.Null(historical.MinValue);
        Assert.Null(historical.MaxValue);
        Assert.Null(historical.StartWith);
        Assert.Null(root.HistoricalSearchInputs);
        Assert.Null(root.Children[1].HistoricalSearchInputs);
        Assert.Equal(106, Assert.Single(root.Children[2].HistoricalSearchInputs!).CustomInputId);
        Assert.Null(root.Children[3].HistoricalSearchInputs);
        Assert.DoesNotContain("customer-value", JsonSerializer.Serialize(result.Value));
        Assert.Equal(1, reservationRepository.QueryCallCount);
        Assert.Equal(2, repository.QueryCallCount); // custom inputs + one image batch
    }

    private static Reservation HistoricalReservation(
        int branchId, int serviceId, int? originalId, int snapshotId, string name, DateTime createdOn)
    {
        var reservation = Reservation.Create(branchId, serviceId, 1, 1, createdOn.AddHours(1),
            DateOnly.FromDateTime(createdOn), createdOn, EntityTestFactory.CurrentUserId);
        var snapshot = ReservationCustomInputValue.Create(originalId ?? 1, originalId ?? 1, name,
            "Historical label", "التسمية", ServiceCustomInputType.Integer, "customer-value", createdOn, 7);
        EntityTestFactory.AssignId(snapshot, snapshotId);
        typeof(ReservationCustomInputValue).GetProperty(nameof(snapshot.ServiceCustomInputId))!
            .SetValue(snapshot, null);
        typeof(ReservationCustomInputValue).GetProperty(nameof(snapshot.OriginalServiceCustomInputId))!
            .SetValue(snapshot, originalId);
        reservation.AddCustomInput(snapshot);
        return reservation;
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
            "englishUserMessage", "hasReservation", "historicalSearchInputs", "iconUrl", "isClientInputRequired",
            "isTicketIssuable", "logoUrl", "rangeEndNumber", "rangePrefix", "rangeStartNumber", "serviceId"
        }, json.EnumerateObject().Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal));
    }
}
