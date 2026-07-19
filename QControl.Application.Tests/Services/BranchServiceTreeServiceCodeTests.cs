using Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;
using Qcontrol.Application.Features.BranchServiceTrees.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.Services;

public sealed class BranchServiceTreeServiceCodeTests
{
    [Fact]
    public void Payload_validation_rejects_duplicate_code_across_ancestor_and_descendant()
    {
        var root = Node(
            "ROOT-001",
            true,
            Node(" ROOT-001 ", true));

        var result = BranchServiceTreePayloadValidator.Validate(
            root,
            Options());

        Assert.NotNull(result);
        Assert.Equal(
            "BranchServiceTrees.Create.DuplicateServiceCodeInTreePayload",
            result.Code);
    }

    [Fact]
    public void Payload_validation_applies_service_code_rule_to_deep_nodes()
    {
        var root = Node(
            null,
            false,
            Node(
                null,
                false,
                Node(null, true)));

        var result = BranchServiceTreePayloadValidator.Validate(
            root,
            Options());

        Assert.NotNull(result);
        Assert.Equal(
            "BranchServiceTrees.Create.ServiceCodeRequired",
            result.Code);
    }

    [Fact]
    public void Payload_validation_collects_normalized_codes_in_one_traversal()
    {
        var root = Node(
            " ROOT-001 ",
            true,
            Node(null, false),
            Node("CHILD-001", true));
        var codes = new List<string>();

        var result = BranchServiceTreePayloadValidator.Validate(
            root,
            Options(),
            codes);

        Assert.Null(result);
        Assert.Equal(new[] { "ROOT-001", "CHILD-001" }, codes);
    }

    [Fact]
    public async Task Database_conflict_check_uses_one_batched_lookup_and_includes_deleted_services()
    {
        var deleted = EntityTestFactory.Service(
            1,
            serviceCode: "USED-001",
            isServiceCodeRequired: true);
        deleted.SoftDelete(
            DateTime.UtcNow,
            EntityTestFactory.CurrentUserId);
        var repository = new InMemoryWriteReadRepository<Service>(
            new List<Service> { deleted });

        var result = await ServiceRuleChecks
            .ValidateServiceCodesAreUniqueAsync(
                repository,
                new[] { "NEW-001", "USED-001", "NEW-002" },
                "BranchServiceTrees.Create.ServiceCodeAlreadyExists",
                CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, repository.ListProjectionSpecCallCount);
        Assert.Equal(
            "BranchServiceTrees.Create.ServiceCodeAlreadyExists",
            result.Code);
    }

    [Fact]
    public async Task Database_conflict_check_skips_query_when_payload_has_no_codes()
    {
        var repository = new InMemoryWriteReadRepository<Service>(
            new List<Service>());

        var result = await ServiceRuleChecks
            .ValidateServiceCodesAreUniqueAsync(
                repository,
                new string?[] { null, " " },
                "BranchServiceTrees.Create.ServiceCodeAlreadyExists",
                CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, repository.ListProjectionSpecCallCount);
    }

    private static BranchServiceTreePayloadValidationOptions Options()
        => new()
        {
            CodePrefix = "BranchServiceTrees.Create",
            MaximumNodesExceededCode =
                "BranchServiceTrees.Create.MaximumNodesExceeded",
            MaximumNodesExceededMessage = "Too many nodes.",
            TicketIssuableCannotHaveChildrenMessage =
                "Ticket service cannot have children.",
            DuplicateArabicNameMessage = "Duplicate Arabic name.",
            DuplicateEnglishNameMessage = "Duplicate English name."
        };

    private static CreateBranchServiceTreeNodeCommand Node(
        string? serviceCode,
        bool isServiceCodeRequired,
        params CreateBranchServiceTreeNodeCommand[] children)
        => new()
        {
            ArabicName = Guid.NewGuid().ToString("N"),
            EnglishName = Guid.NewGuid().ToString("N"),
            ServiceCode = serviceCode,
            IsServiceCodeRequired = isServiceCodeRequired,
            IsTicketIssuable = false,
            Children = children
        };
}
