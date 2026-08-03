using Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.BranchServices
    .GetBranchTicketIssuableServices;

public sealed class TicketIssuableServiceTreeBuilderTests
{
    [Fact]
    public void Preserves_full_path_merges_shared_parents_and_prunes_siblings()
    {
        var items = new[]
        {
            Item(1, englishName: "Medical"),
            Item(2, parentId: 1, englishName: "Examinations"),
            Item(3, parentId: 2, englishName: "Internal Medicine"),
            Item(4, parentId: 2, englishName: "Pediatrics"),
            Item(5, parentId: 1, englishName: "Unavailable sibling")
        };

        var result = TicketIssuableServiceTreeBuilder.Build(
            items,
            new HashSet<int> { 3, 4 });

        var root = Assert.Single(result);
        Assert.Equal(1, root.ServiceId);
        var parent = Assert.Single(root.Children);
        Assert.Equal(2, parent.ServiceId);
        Assert.Equal(new[] { 3, 4 }, parent.Children.Select(x => x.ServiceId));
        Assert.Equal(
            new[] { 1, 2, 3, 4 },
            Flatten(result));
    }

    [Fact]
    public void Eligible_root_leaf_is_returned_with_ticket_fields()
    {
        var result = TicketIssuableServiceTreeBuilder.Build(
            new[]
            {
                Item(
                    7,
                    isTicketIssuable: true,
                    arabicUserMessage: "رسالة",
                    englishUserMessage: "Message",
                    rangePrefix: "R",
                    rangeStart: 10,
                    rangeEnd: 20)
            },
            new HashSet<int> { 7 });

        var leaf = Assert.Single(result);
        Assert.True(leaf.IsTicketIssuable);
        Assert.Empty(leaf.Children);
        Assert.Equal("رسالة", leaf.ArabicUserMessage);
        Assert.Equal("Message", leaf.EnglishUserMessage);
        Assert.Equal("R", leaf.RangePrefix);
        Assert.Equal(10, leaf.RangeStartNumber);
        Assert.Equal(20, leaf.RangeEndNumber);
    }

    [Fact]
    public void Structural_parent_never_exposes_ticket_fields()
    {
        var items = new[]
        {
            Item(
                1,
                isTicketIssuable: true,
                arabicUserMessage: "hidden",
                englishUserMessage: "hidden",
                rangePrefix: "H",
                rangeStart: 1,
                rangeEnd: 2),
            Item(2, parentId: 1, isTicketIssuable: true)
        };

        var result = TicketIssuableServiceTreeBuilder.Build(
            items,
            new HashSet<int> { 2 });

        var parent = Assert.Single(result);
        Assert.False(parent.IsTicketIssuable);
        Assert.Null(parent.ArabicUserMessage);
        Assert.Null(parent.EnglishUserMessage);
        Assert.Null(parent.RangePrefix);
        Assert.Null(parent.RangeStartNumber);
        Assert.Null(parent.RangeEndNumber);
    }

    [Fact]
    public void Sorts_roots_and_children_by_order_arabic_name_then_id()
    {
        var items = new[]
        {
            Item(30, orderNo: 1, arabicName: "ب"),
            Item(20, orderNo: 1, arabicName: "أ"),
            Item(10, orderNo: 0, arabicName: "ج"),
            Item(33, parentId: 30, orderNo: 2, arabicName: "ج"),
            Item(32, parentId: 30, orderNo: 2, arabicName: "أ"),
            Item(31, parentId: 30, orderNo: 1, arabicName: "ب")
        };

        var result = TicketIssuableServiceTreeBuilder.Build(
            items,
            new HashSet<int> { 10, 20, 31, 32, 33 });

        Assert.Equal(new[] { 10, 20, 30 }, result.Select(x => x.ServiceId));
        Assert.Equal(
            new[] { 31, 32, 33 },
            result[2].Children.Select(x => x.ServiceId));
    }

    [Fact]
    public void Does_not_duplicate_service_ids()
    {
        var items = new[]
        {
            Item(1),
            Item(2, parentId: 1),
            Item(3, parentId: 1)
        };

        var result = TicketIssuableServiceTreeBuilder.Build(
            items,
            new HashSet<int> { 2, 3 });
        var ids = Flatten(result);

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Cyclic_parent_data_does_not_recurse_forever()
    {
        var items = new[]
        {
            Item(1, parentId: 2),
            Item(2, parentId: 1)
        };

        var result = TicketIssuableServiceTreeBuilder.Build(
            items,
            new HashSet<int> { 2 });

        Assert.Empty(result);
    }

    [Fact]
    public void Missing_parent_makes_the_first_available_node_a_root()
    {
        var result = TicketIssuableServiceTreeBuilder.Build(
            new[] { Item(2, parentId: 999) },
            new HashSet<int> { 2 });

        Assert.Equal(2, Assert.Single(result).ServiceId);
    }

    private static ServiceHierarchyItem Item(
        int id,
        int? parentId = null,
        int orderNo = 0,
        string? arabicName = null,
        string? englishName = null,
        bool isTicketIssuable = false,
        string? arabicUserMessage = null,
        string? englishUserMessage = null,
        string? rangePrefix = null,
        int? rangeStart = null,
        int? rangeEnd = null) =>
        new()
        {
            Id = id,
            ParentServiceId = parentId,
            Scope = ServiceScope.Global,
            ArabicName = arabicName ?? $"Arabic {id}",
            EnglishName = englishName ?? $"English {id}",
            IsActive = true,
            IsTicketIssuable = isTicketIssuable,
            OrderNo = orderNo,
            ArabicUserMessage = arabicUserMessage,
            EnglishUserMessage = englishUserMessage,
            RangePrefix = rangePrefix,
            RangeStartNumber = rangeStart,
            RangeEndNumber = rangeEnd
        };

    private static IReadOnlyList<int> Flatten(
        IEnumerable<TicketIssuableServiceTreeNodeResponse> nodes) =>
        nodes
            .SelectMany(node =>
                new[] { node.ServiceId }.Concat(Flatten(node.Children)))
            .ToList();
}
