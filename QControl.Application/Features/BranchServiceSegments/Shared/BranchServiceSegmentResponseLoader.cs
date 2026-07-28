using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServiceSegments.Shared;

internal static class BranchServiceSegmentResponseLoader
{
    public static async Task<AssignedBranchServiceSegmentsResponse> LoadAsync(
        BranchServiceSegmentContext context,
        IWriteReadRepository<BranchServiceSegment> assignments,
        CancellationToken cancellationToken,
        string? message = null)
    {
        var rows = await assignments.Query()
            .AsNoTracking()
            .Where(x => x.BranchServiceId == context.BranchServiceId)
            .OrderByDescending(x => x.Segment.Priority)
            .ThenBy(x => x.SegmentId)
            .Select(x => new
            {
                x.Id,
                x.SegmentId,
                x.Segment.ArabicName,
                x.Segment.EnglishName,
                x.Segment.Scope,
                x.Segment.OwnerBranchId,
                x.Segment.Priority,
                x.Quota,
                x.Segment.IsSystemDefault,
                x.RowVersion
            })
            .ToListAsync(cancellationToken);

        var nonDefault = rows
            .Where(x => !x.IsSystemDefault)
            .Sum(x => x.Quota);

        return new AssignedBranchServiceSegmentsResponse
        {
            BranchId = context.BranchId,
            LeafServiceId = context.ServiceId,
            RangePrefix = context.RangePrefix,
            RangeStartNumber = context.RangeStartNumber,
            RangeEndNumber = context.RangeEndNumber,
            Capacity = context.Capacity,
            AllocatedNonDefaultQuota = nonDefault,
            RemainingQuota = context.Capacity - nonDefault,
            Segments = rows.Select(x =>
                new AssignedBranchServiceSegmentItemResponse
                {
                    BranchServiceSegmentId = x.Id,
                    SegmentId = x.SegmentId,
                    ArabicName = x.ArabicName,
                    EnglishName = x.EnglishName,
                    Scope = x.Scope,
                    OwnerBranchId = x.OwnerBranchId,
                    Priority = x.Priority,
                    Quota = x.Quota,
                    IsSystemDefault = x.IsSystemDefault,
                    RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
                }).ToList(),
            Message = message
        };
    }
}
