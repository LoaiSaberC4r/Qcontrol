using QControl.Domain.Entities;

namespace QControl.Application.Tests.Segments;

public sealed class BranchServiceSegmentDomainTests
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Quota_zero_is_allowed()
    {
        var relationship = BranchServiceSegment.Create(
            branchServiceId: 3,
            segmentId: 5,
            quota: 0,
            UserId);

        Assert.Equal(0, relationship.Quota);
    }

    [Fact]
    public void Negative_quota_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BranchServiceSegment.Create(3, 5, -1, UserId));
    }

    [Fact]
    public void Quota_can_be_updated_without_storing_priority()
    {
        var relationship = BranchServiceSegment.Create(
            3,
            5,
            10,
            UserId);

        relationship.UpdateQuota(
            25,
            UserId,
            new DateTime(2026, 7, 28, 8, 0, 0, DateTimeKind.Utc));

        Assert.Equal(25, relationship.Quota);
        Assert.Null(
            typeof(BranchServiceSegment).GetProperty("Priority"));
        Assert.Null(
            typeof(BranchServiceSegment).GetProperty("DisplayOrder"));
    }
}
