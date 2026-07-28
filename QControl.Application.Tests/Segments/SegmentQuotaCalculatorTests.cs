using Qcontrol.Application.Features.BranchServiceSegments.Shared;

namespace QControl.Application.Tests.Segments;

public sealed class SegmentQuotaCalculatorTests
{
    [Fact]
    public void Capacity_is_inclusive()
    {
        var result =
            BranchServiceSegmentQuotaCalculator.CalculateCapacity(1, 100);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value);
    }

    [Fact]
    public void No_explicit_allocations_leave_full_default_quota()
    {
        var result =
            BranchServiceSegmentQuotaCalculator.CalculateDefaultQuota(
                capacity: 100,
                nonDefaultQuota: 0);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value);
    }

    [Fact]
    public void Full_allocation_leaves_zero_default_quota()
    {
        var result =
            BranchServiceSegmentQuotaCalculator.CalculateDefaultQuota(
                capacity: 100,
                nonDefaultQuota: 100);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void Allocation_above_capacity_is_rejected()
    {
        var result =
            BranchServiceSegmentQuotaCalculator.CalculateDefaultQuota(
                capacity: 100,
                nonDefaultQuota: 101);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "BranchServiceSegments.QuotaExceedsServiceCapacity",
            result.Errors.Single().Code);
    }
}
