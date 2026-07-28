using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;

internal static class SegmentGlobalizationRequestResponseLoader
{
    public static async Task<SegmentGlobalizationRequestResponse?> LoadAsync(
        int requestId,
        IWriteReadRepository<SegmentGlobalizationRequest> requests,
        CancellationToken cancellationToken,
        string? message = null)
    {
        var row = await requests.Query()
            .AsNoTracking()
            .Where(x => x.Id == requestId)
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.BranchId,
                BranchArabic = x.Branch.ArabicName,
                BranchEnglish = x.Branch.EnglishName,
                x.SegmentId,
                SegmentArabic = x.Segment.ArabicName,
                SegmentEnglish = x.Segment.EnglishName,
                SegmentScope = x.Segment.Scope,
                SegmentOwner = x.Segment.OwnerBranchId,
                SegmentPriority = x.Segment.Priority,
                x.RequestedByApplicationUserId,
                RequestedByName = x.RequestedByApplicationUser.NameEn,
                x.RequestedOnUtc,
                x.ReviewedByApplicationUserId,
                ReviewedByName = x.ReviewedByApplicationUser == null
                    ? null
                    : x.ReviewedByApplicationUser.NameEn,
                x.ReviewedOnUtc,
                x.RejectionReason,
                x.RowVersion
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return null;
        }

        return new SegmentGlobalizationRequestResponse
        {
            RequestId = row.Id,
            Status = row.Status,
            BranchId = row.BranchId,
            BranchArabicName = row.BranchArabic,
            BranchEnglishName = row.BranchEnglish,
            SegmentId = row.SegmentId,
            SegmentArabicName = row.SegmentArabic,
            SegmentEnglishName = row.SegmentEnglish,
            SegmentScope = row.SegmentScope,
            SegmentOwnerBranchId = row.SegmentOwner,
            SegmentPriority = row.SegmentPriority,
            RequestedByApplicationUserId = row.RequestedByApplicationUserId,
            RequestedByName = row.RequestedByName,
            RequestedOnUtc = row.RequestedOnUtc,
            ReviewedByApplicationUserId = row.ReviewedByApplicationUserId,
            ReviewedByName = row.ReviewedByName,
            ReviewedOnUtc = row.ReviewedOnUtc,
            RejectionReason = row.RejectionReason,
            RowVersion = RowVersionConverter.ToBase64(row.RowVersion),
            Message = message
        };
    }
}
