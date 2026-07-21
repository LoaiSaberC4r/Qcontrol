using Qcontrol.Application.Features.BranchServices.Query.GetBranchServiceTree;
using Qcontrol.Application.Features.Services.Query.GetServicesTree;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Shared.Security;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Services;

public sealed class ServiceTreeSearchHandlerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Branch_tree_empty_search_preserves_the_original_tree(
        string? searchText)
    {
        var services = MedicalServices();
        var handler = BranchHandler(services, services.Select(x => x.Id));

        var result = await handler.Handle(
            new GetBranchServiceTreeQuery
            {
                BranchId = 1,
                SearchText = searchText
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            services.Select(x => x.Id).OrderBy(x => x),
            Flatten(result.Value).OrderBy(x => x));
    }

    [Fact]
    public async Task Branch_tree_child_code_search_keeps_ancestors_and_excludes_siblings()
    {
        var services = MedicalServices();
        var handler = BranchHandler(services, services.Select(x => x.Id));

        var result = await handler.Handle(
            new GetBranchServiceTreeQuery
            {
                BranchId = 1,
                SearchText = " lab-001 "
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, Flatten(result.Value));
        Assert.Equal("LAB-001", result.Value[0].Children[0].Children[0].ServiceCode);
    }

    [Fact]
    public async Task Branch_tree_parent_search_keeps_its_available_subtree()
    {
        var services = MedicalServices();
        var handler = BranchHandler(services, services.Select(x => x.Id));

        var result = await handler.Handle(
            new GetBranchServiceTreeQuery
            {
                BranchId = 1,
                SearchText = "laboratory"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, Flatten(result.Value));
    }

    [Fact]
    public async Task Branch_tree_search_does_not_reintroduce_excluded_services()
    {
        var inactive = EntityTestFactory.Service(
            1,
            englishName: "Hidden inactive");
        EntityTestFactory.SetServiceActive(inactive, false);
        var deleted = EntityTestFactory.Service(
            2,
            englishName: "Hidden deleted");
        deleted.SoftDelete(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var unassigned = EntityTestFactory.Service(
            3,
            englishName: "Hidden unassigned");
        var foreignScoped = EntityTestFactory.BranchScopedService(
            4,
            ownerBranchId: 2,
            englishName: "Hidden foreign");
        var services = new List<Service>
        {
            inactive,
            deleted,
            unassigned,
            foreignScoped
        };
        var handler = BranchHandler(services, new[] { 1, 2, 4 });

        var result = await handler.Handle(
            new GetBranchServiceTreeQuery
            {
                BranchId = 1,
                SearchText = "Hidden"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Services_tree_search_applies_hierarchy_rules_after_visibility_filters()
    {
        var services = MedicalServices();
        var handler = ServicesHandler(services);

        var childResult = await handler.Handle(
            new GetServicesTreeQuery { SearchText = "blood" },
            CancellationToken.None);
        var parentResult = await handler.Handle(
            new GetServicesTreeQuery { SearchText = "Laboratory" },
            CancellationToken.None);

        Assert.True(childResult.IsSuccess);
        Assert.True(parentResult.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, Flatten(childResult.Value));
        Assert.Equal(new[] { 1, 2, 3 }, Flatten(parentResult.Value));
    }

    [Fact]
    public async Task Services_tree_search_keeps_scope_and_owner_filters()
    {
        var global = EntityTestFactory.Service(
            1,
            englishName: "Target global");
        var owned = EntityTestFactory.BranchScopedService(
            2,
            ownerBranchId: 5,
            englishName: "Target owned");
        var foreign = EntityTestFactory.BranchScopedService(
            3,
            ownerBranchId: 6,
            englishName: "Target foreign");
        var handler = ServicesHandler(new List<Service>
        {
            global,
            owned,
            foreign
        });

        var result = await handler.Handle(
            new GetServicesTreeQuery
            {
                SearchText = "Target",
                Scope = ServiceScope.BranchScoped,
                OwnerBranchId = 5
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 2 }, Flatten(result.Value));
    }

    [Fact]
    public async Task Services_tree_search_keeps_visibility_active_and_deleted_filters()
    {
        var inactive = EntityTestFactory.Service(
            1,
            englishName: "Hidden inactive");
        EntityTestFactory.SetServiceActive(inactive, false);
        var deleted = EntityTestFactory.Service(
            2,
            englishName: "Hidden deleted");
        deleted.SoftDelete(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var services = new List<Service> { inactive, deleted };

        var hiddenResult = await ServicesHandler(services).Handle(
            new GetServicesTreeQuery { SearchText = "Hidden" },
            CancellationToken.None);
        var includedResult = await ServicesHandler(services).Handle(
            new GetServicesTreeQuery
            {
                SearchText = "Hidden",
                IncludeInactive = true,
                IncludeDeleted = true
            },
            CancellationToken.None);
        var invisibleResult = await ServicesHandler(
            services,
            canView: false).Handle(
                new GetServicesTreeQuery
                {
                    SearchText = "Hidden",
                    IncludeInactive = true,
                    IncludeDeleted = true
                },
                CancellationToken.None);

        Assert.True(hiddenResult.IsSuccess);
        Assert.Empty(hiddenResult.Value);
        Assert.Equal(new[] { 1, 2 }, Flatten(includedResult.Value));
        Assert.Empty(invisibleResult.Value);
    }

    private static GetBranchServiceTreeQueryHandler BranchHandler(
        List<Service> services,
        IEnumerable<int> assignedServiceIds)
    {
        var assignments = assignedServiceIds
            .Select((serviceId, index) => EntityTestFactory.BranchService(
                index + 1,
                branchId: 1,
                serviceId))
            .ToList();

        return new GetBranchServiceTreeQueryHandler(
            new InMemoryWriteReadRepository<Branch>(
                new List<Branch> { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<Service>(services),
            new InMemoryWriteReadRepository<BranchService>(assignments),
            new TestCurrentUser(),
            new TestCurrentBranchContext(),
            AllowAllServiceDefinitionAccessValidator.Instance);
    }

    private static GetServicesTreeQueryHandler ServicesHandler(
        List<Service> services,
        bool canView = true)
        => new(
            new InMemoryWriteReadRepository<Service>(services),
            new InMemoryWriteReadRepository<BranchService>(
                new List<BranchService>()),
            new TestCurrentUser(),
            new TestCurrentBranchContext(),
            new TestServiceVisibilityPolicy { CanViewResult = canView });

    private static List<Service> MedicalServices()
        => new()
        {
            EntityTestFactory.Service(
                1,
                englishName: "Medical Services"),
            EntityTestFactory.Service(
                2,
                parentServiceId: 1,
                englishName: "Laboratory"),
            EntityTestFactory.Service(
                3,
                parentServiceId: 2,
                arabicName: "تحليل دم",
                englishName: "Blood Test",
                serviceCode: "LAB-001",
                isServiceCodeRequired: true),
            EntityTestFactory.Service(
                4,
                parentServiceId: 1,
                englishName: "Radiology"),
            EntityTestFactory.Service(
                5,
                parentServiceId: 4,
                englishName: "X-Ray")
        };

    private static IReadOnlyList<int> Flatten(
        IEnumerable<ServiceTreeNodeResponse> nodes)
        => nodes
            .SelectMany(node => new[] { node.Id }.Concat(Flatten(node.Children)))
            .ToList();
}
