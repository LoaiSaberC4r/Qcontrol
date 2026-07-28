using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Shared.Security;

internal sealed class SegmentVisibilityPolicy : ISegmentVisibilityPolicy
{
    private readonly ICurrentBranchContext _branchContext;

    public SegmentVisibilityPolicy(ICurrentBranchContext branchContext)
    {
        _branchContext = branchContext;
    }

    public Result EnsureCanUseVisibilityContext(string codePrefix)
    {
        if (_branchContext.IsSystemLevelActor)
        {
            return Result.Ok();
        }

        if (!_branchContext.IsBranchActor ||
            !_branchContext.ActiveBranchId.HasValue)
        {
            return Result.Fail(new Error(
                $"{codePrefix}.BranchAdminRequired",
                SegmentFeatureMessages.BranchAdminRequired,
                ErrorType.Security));
        }

        return Result.Ok();
    }

    public IQueryable<Segment> ApplyVisibleSegments(
        IQueryable<Segment> query)
    {
        if (_branchContext.IsSystemLevelActor)
        {
            return query;
        }

        if (!_branchContext.IsBranchActor ||
            !_branchContext.ActiveBranchId.HasValue)
        {
            return query.Where(_ => false);
        }

        var branchId = _branchContext.ActiveBranchId.Value;
        return query.Where(x =>
            x.Scope == SegmentScope.Global ||
            (x.Scope == SegmentScope.BranchScoped &&
             x.OwnerBranchId == branchId));
    }

    public bool CanView(SegmentScope scope, int? ownerBranchId)
    {
        if (_branchContext.IsSystemLevelActor)
        {
            return true;
        }

        return _branchContext.IsBranchActor &&
            _branchContext.ActiveBranchId.HasValue &&
            (scope == SegmentScope.Global ||
             (scope == SegmentScope.BranchScoped &&
              ownerBranchId == _branchContext.ActiveBranchId));
    }
}
