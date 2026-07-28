using BuildingBlock.Domain.Results;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Abstraction.Security;

public interface ISegmentVisibilityPolicy
{
    Result EnsureCanUseVisibilityContext(string codePrefix);

    IQueryable<Segment> ApplyVisibleSegments(IQueryable<Segment> query);

    bool CanView(SegmentScope scope, int? ownerBranchId);
}
