using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Segments;

public sealed class SegmentGlobalizationRequestDomainTests
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void New_request_is_pending()
    {
        var request = SegmentGlobalizationRequest.Create(
            branchId: 5,
            segmentId: 8,
            UserId,
            DateTime.UtcNow);

        Assert.Equal(
            SegmentGlobalizationRequestStatus.Pending,
            request.Status);
    }

    [Fact]
    public void Approve_records_review_and_cannot_repeat()
    {
        var request = SegmentGlobalizationRequest.Create(
            5,
            8,
            UserId,
            DateTime.UtcNow);
        var reviewed = DateTime.UtcNow.AddMinutes(1);

        request.Approve(UserId, reviewed);

        Assert.Equal(
            SegmentGlobalizationRequestStatus.Approved,
            request.Status);
        Assert.Equal(UserId, request.ReviewedByApplicationUserId);
        Assert.Equal(reviewed, request.ReviewedOnUtc);
        Assert.Throws<InvalidOperationException>(() =>
            request.Approve(UserId, reviewed));
    }

    [Fact]
    public void Reject_preserves_reason_and_cannot_repeat()
    {
        var request = SegmentGlobalizationRequest.Create(
            5,
            8,
            UserId,
            DateTime.UtcNow);

        request.Reject(UserId, DateTime.UtcNow, " Duplicate ");

        Assert.Equal(
            SegmentGlobalizationRequestStatus.Rejected,
            request.Status);
        Assert.Equal("Duplicate", request.RejectionReason);
        Assert.Throws<InvalidOperationException>(() =>
            request.Reject(UserId, DateTime.UtcNow, null));
    }
}
