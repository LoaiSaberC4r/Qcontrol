using Qcontrol.Application.Features.Services.Shared;

namespace QControl.Application.Tests.Services;

public sealed class ServiceHierarchySearchTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_search_preserves_the_original_collection(
        string? searchText)
    {
        var items = Items(Item(1, englishName: "Medical Services"));

        var result = ServiceHierarchySearch.Apply(items, searchText);

        Assert.Same(items, result);
    }

    [Theory]
    [InlineData("الخدمات", 1)]
    [InlineData("laboratory", 2)]
    [InlineData("lab-001", 3)]
    public void Search_matches_confirmed_fields_case_insensitively_when_required(
        string searchText,
        int expectedMatchId)
    {
        var items = Items(
            Item(1, arabicName: "الخدمات الطبية"),
            Item(2, englishName: "Laboratory"),
            Item(3, serviceCode: "LAB-001"));

        var result = ServiceHierarchySearch.Apply(items, $" {searchText} ");

        Assert.Single(result);
        Assert.Contains(expectedMatchId, result.Keys);
    }

    [Fact]
    public void Child_match_keeps_ancestors_without_unrelated_siblings()
    {
        var items = MedicalTree();

        var result = ServiceHierarchySearch.Apply(items, "blood");

        Assert.Equal(new[] { 1, 2, 3 }, result.Keys.OrderBy(x => x));
    }

    [Fact]
    public void Parent_match_keeps_its_available_subtree()
    {
        var items = MedicalTree();

        var result = ServiceHierarchySearch.Apply(items, "Laboratory");

        Assert.Equal(new[] { 1, 2, 3 }, result.Keys.OrderBy(x => x));
    }

    [Fact]
    public void Multiple_matches_merge_paths_without_duplicates()
    {
        var items = Items(
            Item(1, englishName: "Medical Services"),
            Item(2, 1, englishName: "Laboratory"),
            Item(3, 2, englishName: "Blood Test"),
            Item(4, 1, englishName: "Donation Services"),
            Item(5, 4, englishName: "Blood Donation"));

        var result = ServiceHierarchySearch.Apply(items, "blood");

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Keys.OrderBy(x => x));
    }

    [Fact]
    public void Search_never_reintroduces_items_absent_from_the_filtered_source()
    {
        var alreadyFilteredItems = Items(
            Item(1, englishName: "Matching Parent"),
            Item(2, 1, englishName: "Allowed Child"));

        var result = ServiceHierarchySearch.Apply(
            alreadyFilteredItems,
            "Matching Parent");

        Assert.Equal(new[] { 1, 2 }, result.Keys.OrderBy(x => x));
        Assert.DoesNotContain(99, result.Keys);
    }

    [Fact]
    public void No_match_returns_an_empty_collection()
    {
        var result = ServiceHierarchySearch.Apply(
            MedicalTree(),
            "not-present");

        Assert.Empty(result);
    }

    private static IReadOnlyDictionary<int, ServiceHierarchyItem> MedicalTree()
        => Items(
            Item(1, englishName: "Medical Services"),
            Item(2, 1, englishName: "Laboratory"),
            Item(3, 2, englishName: "Blood Test"),
            Item(4, 1, englishName: "Radiology"),
            Item(5, 4, englishName: "X-Ray"));

    private static IReadOnlyDictionary<int, ServiceHierarchyItem> Items(
        params ServiceHierarchyItem[] items)
        => items.ToDictionary(x => x.Id);

    private static ServiceHierarchyItem Item(
        int id,
        int? parentServiceId = null,
        string? arabicName = null,
        string? englishName = null,
        string? serviceCode = null)
        => new()
        {
            Id = id,
            ParentServiceId = parentServiceId,
            ArabicName = arabicName ?? $"Arabic {id}",
            EnglishName = englishName ?? $"English {id}",
            ServiceCode = serviceCode
        };
}
