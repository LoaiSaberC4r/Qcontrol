using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Segments;

public sealed class SegmentDomainTests
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Normal_segment_accepts_priority_zero()
    {
        var segment = Segment.CreateGlobal(
            "عادي",
            "Normal",
            priority: 0,
            UserId);

        Assert.Equal(0, segment.Priority);
        Assert.False(segment.IsSystemDefault);
    }

    [Fact]
    public void Segment_rejects_negative_priority()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Segment.CreateGlobal("عربي", "English", -1, UserId));
    }

    [Fact]
    public void Branch_scoped_segment_has_owner()
    {
        var segment = Segment.CreateBranchScoped(
            ownerBranchId: 7,
            "عربي",
            "English",
            priority: 2,
            UserId);

        Assert.Equal(SegmentScope.BranchScoped, segment.Scope);
        Assert.Equal(7, segment.OwnerBranchId);
    }

    [Fact]
    public void Promotion_changes_scope_and_clears_owner()
    {
        var segment = Segment.CreateBranchScoped(
            7,
            "عربي",
            "English",
            2,
            UserId);

        segment.PromoteToGlobal(
            UserId,
            new DateTime(2026, 7, 28, 8, 0, 0, DateTimeKind.Utc));

        Assert.Equal(SegmentScope.Global, segment.Scope);
        Assert.Null(segment.OwnerBranchId);
    }

    [Fact]
    public void System_default_has_permanent_protected_values()
    {
        var segment = Segment.CreateSystemDefault(
            "افتراضي",
            "Default",
            UserId);

        Assert.True(segment.IsSystemDefault);
        Assert.Equal(0, segment.Priority);
        Assert.Equal(SegmentScope.Global, segment.Scope);
        Assert.Null(segment.OwnerBranchId);
    }

    [Fact]
    public void System_default_priority_cannot_change()
    {
        var segment = Segment.CreateSystemDefault(
            "افتراضي",
            "Default",
            UserId);

        Assert.Throws<InvalidOperationException>(() =>
            segment.UpdateDefinition(
                "افتراضي",
                "Default",
                priority: 1,
                UserId,
                DateTime.UtcNow));
    }

    [Fact]
    public void System_default_names_can_change_with_priority_zero()
    {
        var segment = Segment.CreateSystemDefault(
            "افتراضي",
            "Default",
            UserId);

        segment.UpdateDefinition(
            "الافتراضي",
            "System default",
            priority: 0,
            UserId,
            DateTime.UtcNow);

        Assert.Equal("الافتراضي", segment.ArabicName);
        Assert.Equal("System default", segment.EnglishName);
        Assert.Equal(0, segment.Priority);
    }
}
