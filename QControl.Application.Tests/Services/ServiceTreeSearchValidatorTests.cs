using Qcontrol.Application.Features.BranchServices.Query.GetBranchServiceTree;
using Qcontrol.Application.Features.Services.Query.GetServicesTree;
using Qcontrol.Domain.Resources;

namespace QControl.Application.Tests.Services;

public sealed class ServiceTreeSearchValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validators_allow_optional_or_whitespace_search(string? searchText)
    {
        var branchResult = new GetBranchServiceTreeQueryValidator().Validate(
            new GetBranchServiceTreeQuery
            {
                BranchId = 1,
                SearchText = searchText
            });
        var servicesResult = new GetServicesTreeQueryValidator().Validate(
            new GetServicesTreeQuery { SearchText = searchText });

        Assert.True(branchResult.IsValid);
        Assert.True(servicesResult.IsValid);
    }

    [Fact]
    public void Validators_allow_200_characters()
    {
        var searchText = new string('A', 200);

        Assert.True(new GetBranchServiceTreeQueryValidator().Validate(
            new GetBranchServiceTreeQuery
            {
                BranchId = 1,
                SearchText = searchText
            }).IsValid);
        Assert.True(new GetServicesTreeQueryValidator().Validate(
            new GetServicesTreeQuery { SearchText = searchText }).IsValid);
    }

    [Fact]
    public void Validators_reject_more_than_200_characters_with_localized_message()
    {
        var searchText = new string('A', 201);
        var branchResult = new GetBranchServiceTreeQueryValidator().Validate(
            new GetBranchServiceTreeQuery
            {
                BranchId = 1,
                SearchText = searchText
            });
        var servicesResult = new GetServicesTreeQueryValidator().Validate(
            new GetServicesTreeQuery { SearchText = searchText });

        Assert.Contains(
            branchResult.Errors,
            x => x.ErrorMessage == ErrorMessage.SearchTerm_MaxLength);
        Assert.Contains(
            servicesResult.Errors,
            x => x.ErrorMessage == ErrorMessage.SearchTerm_MaxLength);
    }
}
