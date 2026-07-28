using QControl.Application.Shared.Security;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Segments;

public sealed class SegmentVisibilityPolicyTests
{
    [Fact]
    public void Technical_admin_can_view_all_scopes()
    {
        var policy = new SegmentVisibilityPolicy(
            new TestCurrentBranchContext());

        Assert.True(policy.CanView(SegmentScope.Global, null));
        Assert.True(policy.CanView(SegmentScope.BranchScoped, 9));
    }

    [Fact]
    public void Branch_admin_sees_global_and_own_but_not_foreign()
    {
        var policy = new SegmentVisibilityPolicy(
            new TestCurrentBranchContext
            {
                UserType = UserType.BranchAdmin,
                ActiveBranchId = 5
            });

        Assert.True(policy.CanView(SegmentScope.Global, null));
        Assert.True(policy.CanView(SegmentScope.BranchScoped, 5));
        Assert.False(policy.CanView(SegmentScope.BranchScoped, 6));
    }
}
