using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServiceSegments.Shared;

internal static class DefaultBranchServiceSegmentFactory
{
    public static async Task<Result<List<BranchServiceSegment>>>
        CreateForNewServicesAsync(
            IReadOnlyList<Service> services,
            IReadOnlyList<BranchService> assignments,
            IWriteReadRepository<Segment> segments,
            Guid createdByApplicationUserId,
            CancellationToken cancellationToken)
    {
        if (services.Count != assignments.Count)
        {
            return Result<List<BranchServiceSegment>>.Fail(new Error(
                "BranchServiceSegments.InvalidAssignmentGraph",
                BranchServiceSegmentMessages.ServiceNotAssigned,
                ErrorType.Infrastructure));
        }

        var eligibleIndexes = Enumerable.Range(0, services.Count)
            .Where(index =>
                services[index].IsTicketIssuable &&
                !services.Any(candidate =>
                    ReferenceEquals(
                        candidate.ParentService,
                        services[index])))
            .ToArray();
        if (eligibleIndexes.Length == 0)
        {
            return Result<List<BranchServiceSegment>>.Ok(new());
        }

        var defaultSegment = await segments.Query()
            .AsTracking()
            .SingleOrDefaultAsync(
                x => x.IsSystemDefault,
                cancellationToken);
        if (defaultSegment is null)
        {
            return Result<List<BranchServiceSegment>>.Fail(new Error(
                "BranchServiceSegments.DefaultSegmentNotFound",
                BranchServiceSegmentMessages.SegmentNotFound,
                ErrorType.Infrastructure));
        }

        var result = new List<BranchServiceSegment>(
            eligibleIndexes.Length);
        foreach (var index in eligibleIndexes)
        {
            var service = services[index];
            var capacity =
                BranchServiceSegmentQuotaCalculator.CalculateCapacity(
                    service.RangeStartNumber,
                    service.RangeEndNumber);
            if (capacity.IsFailure)
            {
                return Result<List<BranchServiceSegment>>.Fail(
                    capacity.Errors);
            }

            result.Add(BranchServiceSegment.Create(
                assignments[index],
                defaultSegment,
                capacity.Value,
                createdByApplicationUserId));
        }

        return Result<List<BranchServiceSegment>>.Ok(result);
    }
}
