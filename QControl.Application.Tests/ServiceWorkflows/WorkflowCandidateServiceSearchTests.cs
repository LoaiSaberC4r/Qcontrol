using Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowCandidateServices;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.ServiceWorkflows;

public sealed class WorkflowCandidateServiceSearchTests
{
    [Theory]
    [InlineData("تحليل")]
    [InlineData("blood count")]
    [InlineData("lab-001")]
    public async Task Search_matches_arabic_english_and_service_code(
        string searchText)
    {
        var parent = EntityTestFactory.Service(
            1,
            arabicName: "خدمات المختبر",
            englishName: "Laboratory Services");
        var leaf = EntityTestFactory.Service(
            2,
            parentServiceId: 1,
            arabicName: "تحليل دم كامل",
            englishName: "Complete Blood Count",
            isTicketIssuable: true,
            serviceCode: "LAB-001",
            isServiceCodeRequired: true);
        var handler = Handler(
            new List<Service> { parent, leaf },
            new[] { leaf.Id });

        var result = await handler.Handle(
            new GetWorkflowCandidateServicesQuery
            {
                BranchId = 1,
                SearchText = $" {searchText} "
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var response = Assert.Single(result.Value.Data);
        Assert.Equal(leaf.Id, response.ServiceId);
        Assert.Equal(parent.Id, response.ParentServiceId);
        Assert.Equal(parent.ArabicName, response.ParentArabicName);
        Assert.Equal(parent.EnglishName, response.ParentEnglishName);
        Assert.Equal(leaf.ArabicName, response.ArabicName);
        Assert.Equal(leaf.EnglishName, response.EnglishName);
        Assert.Equal("LAB-001", response.ServiceCode);
        Assert.True(response.IsActive);
        Assert.False(response.IsDeleted);
        Assert.True(response.EffectiveIsActive);
        Assert.True(response.IsTicketIssuable);
    }

    [Fact]
    public async Task Null_service_code_does_not_break_other_search_fields()
    {
        var leaf = EntityTestFactory.Service(
            1,
            englishName: "No Code Service",
            isTicketIssuable: true);
        var handler = Handler(
            new List<Service> { leaf },
            new[] { leaf.Id });

        var result = await handler.Handle(
            new GetWorkflowCandidateServicesQuery
            {
                BranchId = 1,
                SearchText = "no code"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(result.Value.Data).ServiceCode);
    }

    [Fact]
    public async Task Search_keeps_parent_eligibility_filters_and_pagination_total()
    {
        var parent = EntityTestFactory.Service(10);
        var otherParent = EntityTestFactory.Service(20);
        var first = Candidate(11, parent.Id, "LAB-001");
        var second = Candidate(12, parent.Id, "LAB-002");
        var otherParentCandidate = Candidate(21, otherParent.Id, "LAB-003");
        var inactive = Candidate(13, parent.Id, "LAB-004");
        EntityTestFactory.SetServiceActive(inactive, false);
        var deleted = Candidate(14, parent.Id, "LAB-005");
        deleted.SoftDelete(DateTime.UtcNow, EntityTestFactory.CurrentUserId);
        var unassigned = Candidate(15, parent.Id, "LAB-006");
        var foreignScoped = EntityTestFactory.BranchScopedService(
            16,
            ownerBranchId: 2,
            parentServiceId: parent.Id,
            isTicketIssuable: true,
            englishName: "Candidate LAB-007",
            serviceCode: "LAB-007",
            isServiceCodeRequired: true);
        var services = new List<Service>
        {
            parent,
            otherParent,
            first,
            second,
            otherParentCandidate,
            inactive,
            deleted,
            unassigned,
            foreignScoped
        };
        var assignedIds = services
            .Where(x => x.Id != unassigned.Id)
            .Select(x => x.Id);
        var handler = Handler(services, assignedIds);

        var result = await handler.Handle(
            new GetWorkflowCandidateServicesQuery
            {
                BranchId = 1,
                ParentServiceId = parent.Id,
                SearchText = "lab-",
                PageNumber = 1,
                PageSize = 1
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.Single(result.Value.Data);
        Assert.Contains(
            result.Value.Data[0].ServiceCode,
            new[] { "LAB-001", "LAB-002" });
    }

    private static GetWorkflowCandidateServicesQueryHandler Handler(
        List<Service> services,
        IEnumerable<int> assignedServiceIds)
    {
        var assignments = assignedServiceIds
            .Select((serviceId, index) => EntityTestFactory.BranchService(
                index + 1,
                branchId: 1,
                serviceId: serviceId))
            .ToList();

        return new GetWorkflowCandidateServicesQueryHandler(
            new InMemoryWriteReadRepository<Service>(services),
            new InMemoryWriteReadRepository<Branch>(
                new List<Branch> { EntityTestFactory.Branch(1) }),
            new InMemoryWriteReadRepository<BranchService>(assignments),
            new TestCurrentUser(),
            new TestBranchAccessValidator());
    }

    private static Service Candidate(
        int id,
        int parentServiceId,
        string serviceCode)
        => EntityTestFactory.Service(
            id,
            parentServiceId,
            englishName: $"Candidate {serviceCode}",
            isTicketIssuable: true,
            serviceCode: serviceCode,
            isServiceCodeRequired: true);
}
