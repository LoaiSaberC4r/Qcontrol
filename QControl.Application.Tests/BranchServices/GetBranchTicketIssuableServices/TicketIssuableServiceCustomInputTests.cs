using Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.BranchServices
    .GetBranchTicketIssuableServices;

public sealed class TicketIssuableServiceCustomInputTests
{
    [Fact]
    public async Task Handler_returns_inputs_only_on_eligible_leaf_with_one_batched_query()
    {
        var parent = EntityTestFactory.Service(1);
        var leaf = EntityTestFactory.Service(
            2,
            parentServiceId: parent.Id,
            isTicketIssuable: true,
            isClientInputRequired: true);
        EntityTestFactory.ServiceCustomInput(
            leaf,
            10,
            "NationalId",
            order: 1,
            isRequired: true,
            minLength: 14,
            maxLength: 14);
        EntityTestFactory.ServiceCustomInput(
            leaf,
            11,
            "Age",
            ServiceCustomInputType.Integer,
            order: 2,
            minValue: 18,
            maxValue: 100);
        var services = new List<Service> { parent, leaf };
        var serviceRepository =
            new InMemoryWriteReadRepository<Service>(services);
        var handler = new GetBranchTicketIssuableServicesQueryHandler(
            new InMemoryWriteReadRepository<Branch>(
                new List<Branch> { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<BranchService>(
                new List<BranchService>
                {
                    EntityTestFactory.BranchService(1, 1, leaf.Id)
                }),
            serviceRepository,
            new InMemoryWriteReadRepository<ServiceSchedule>(
                new List<ServiceSchedule>
                {
                    EntityTestFactory.ServiceSchedule(
                        1,
                        1,
                        leaf.Id,
                        timeSlots: new[]
                        {
                            new ServiceScheduleTimeSlotDefinition(
                                DayOfWeek.Sunday,
                                new TimeOnly(9, 0),
                                new TimeOnly(12, 0))
                        })
                }),
            new InMemoryWriteReadRepository<GeneralBrand>(
                new List<GeneralBrand>()),
            new TestCurrentUser(),
            new TestBranchAccessValidator(),
            new TestDateTimeProvider
            {
                UtcNow = new DateTime(
                    2026,
                    8,
                    2,
                    10,
                    0,
                    0,
                    DateTimeKind.Utc)
            });

        var result = await handler.Handle(
            new GetBranchTicketIssuableServicesQuery { BranchId = 1 },
            CancellationToken.None);

        var root = Assert.Single(result.Value.Services);
        Assert.False(root.IsClientInputRequired);
        Assert.Null(root.CustomInputs);
        var leafResponse = Assert.Single(root.Children);
        Assert.True(leafResponse.IsClientInputRequired);
        var customInputs = leafResponse.CustomInputs!;
        Assert.Equal(
            new[] { "NationalId", "Age" },
            customInputs.Select(x => x.Name));
        Assert.Equal(14, customInputs[0].MaxLength);
        Assert.Equal(100, customInputs[1].MaxValue);
        Assert.Equal(1, serviceRepository.QueryCallCount);
    }

    [Fact]
    public void Builder_returns_null_when_leaf_does_not_require_inputs()
    {
        var items = new[]
        {
            new ServiceHierarchyItem
            {
                Id = 1,
                ArabicName = "خدمة",
                EnglishName = "Service",
                IsActive = true,
                IsTicketIssuable = true,
                IsClientInputRequired = false
            }
        };

        var result = TicketIssuableServiceTreeBuilder.Build(
            items,
            new HashSet<int> { 1 },
            new Dictionary<int, IReadOnlyList<ServiceCustomInputResponse>>());

        var leaf = Assert.Single(result);
        Assert.False(leaf.IsClientInputRequired);
        Assert.Null(leaf.CustomInputs);
    }
}
