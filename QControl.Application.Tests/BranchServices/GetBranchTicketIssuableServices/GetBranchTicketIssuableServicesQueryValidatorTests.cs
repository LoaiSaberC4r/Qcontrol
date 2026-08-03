using Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

namespace QControl.Application.Tests.BranchServices
    .GetBranchTicketIssuableServices;

public sealed class GetBranchTicketIssuableServicesQueryValidatorTests
{
    private readonly GetBranchTicketIssuableServicesQueryValidator _validator =
        new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rejects_non_positive_branch_id(int branchId)
    {
        var result = _validator.Validate(
            new GetBranchTicketIssuableServicesQuery { BranchId = branchId });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(
                GetBranchTicketIssuableServicesQuery.BranchId));
    }

    [Fact]
    public void Accepts_positive_branch_id()
    {
        var result = _validator.Validate(
            new GetBranchTicketIssuableServicesQuery { BranchId = 1 });

        Assert.True(result.IsValid);
    }
}
