using BuildingBlock.Domain.Results;

namespace Qcontrol.Application.Features.BranchServiceSegments.Shared;

internal static class BranchServiceSegmentQuotaCalculator
{
    public static Result<int> CalculateCapacity(
        int? rangeStartNumber,
        int? rangeEndNumber)
    {
        if (!rangeStartNumber.HasValue ||
            !rangeEndNumber.HasValue ||
            rangeStartNumber.Value < 0 ||
            rangeEndNumber.Value < rangeStartNumber.Value)
        {
            return Result<int>.Fail(new Error(
                "BranchServiceSegments.InvalidServiceRange",
                BranchServiceSegmentMessages.InvalidServiceRange,
                ErrorType.Validation));
        }

        var capacity =
            (long)rangeEndNumber.Value - rangeStartNumber.Value + 1;
        return capacity <= 0 || capacity > int.MaxValue
            ? Result<int>.Fail(new Error(
                "BranchServiceSegments.InvalidServiceRange",
                BranchServiceSegmentMessages.InvalidServiceRange,
                ErrorType.Validation))
            : Result<int>.Ok((int)capacity);
    }

    public static Result<int> CalculateDefaultQuota(
        int capacity,
        long nonDefaultQuota)
    {
        if (capacity < 0 || nonDefaultQuota < 0)
        {
            return Result<int>.Fail(new Error(
                "BranchServiceSegments.InvalidQuotaCalculation",
                BranchServiceSegmentMessages.QuotaNonNegative,
                ErrorType.Validation));
        }

        if (nonDefaultQuota > capacity)
        {
            return Result<int>.Fail(new Error(
                "BranchServiceSegments.QuotaExceedsServiceCapacity",
                BranchServiceSegmentMessages.QuotaExceedsCapacity,
                ErrorType.Validation));
        }

        return Result<int>.Ok(capacity - (int)nonDefaultQuota);
    }
}
