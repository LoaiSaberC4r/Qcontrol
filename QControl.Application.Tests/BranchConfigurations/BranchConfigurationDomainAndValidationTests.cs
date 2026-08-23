using Qcontrol.Application.Features.BranchConfigurations.Command.CreateBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Query.GetBranchConfiguration;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.BranchConfigurations;

public sealed class BranchConfigurationDomainAndValidationTests
{
    public static IEnumerable<object?[]> InvalidAllowedTimes()
    {
        yield return new object?[] { null };
        yield return new object?[] { TimeSpan.FromTicks(-1) };
        yield return new object?[] { TimeSpan.FromHours(24) };
        yield return new object?[] { TimeSpan.FromHours(25) };
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 30, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(23, 59, 59)]
    public void Domain_accepts_confirmed_allowed_time_range(
        int hours,
        int minutes,
        int seconds)
    {
        var allowedTime = new TimeSpan(hours, minutes, seconds);

        var configuration = BranchConfiguration.Create(1, allowedTime);

        Assert.Equal(allowedTime, configuration.AllowedTime);
        Assert.Equal(1, configuration.BranchId);
    }

    [Theory]
    [MemberData(nameof(InvalidAllowedTimes))]
    public void Create_validator_rejects_missing_or_out_of_range_time(
        TimeSpan? allowedTime)
    {
        var validator = new CreateBranchConfigurationCommandValidator();

        var result = validator.Validate(new CreateBranchConfigurationCommand
        {
            BranchId = 1,
            AllowedTime = allowedTime
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.ErrorCode.Contains("AllowedTime", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(InvalidAllowedTimes))]
    public void Update_validator_rejects_missing_or_out_of_range_time(
        TimeSpan? allowedTime)
    {
        var validator = new UpdateBranchConfigurationCommandValidator();

        var result = validator.Validate(new UpdateBranchConfigurationCommand
        {
            BranchId = 1,
            AllowedTime = allowedTime,
            RowVersion = Convert.ToBase64String(new byte[8])
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.ErrorCode.Contains("AllowedTime", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-base64")]
    [InlineData("AQ==")]
    public void Update_validator_requires_valid_sql_row_version(string value)
    {
        var validator = new UpdateBranchConfigurationCommandValidator();

        var result = validator.Validate(new UpdateBranchConfigurationCommand
        {
            BranchId = 1,
            AllowedTime = TimeSpan.Zero,
            RowVersion = value
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.ErrorCode.Contains("RowVersion", StringComparison.Ordinal));
    }

    [Fact]
    public void Query_validator_rejects_non_positive_branch_id()
    {
        var validator = new GetBranchConfigurationQueryValidator();

        var result = validator.Validate(
            new GetBranchConfigurationQuery { BranchId = 0 });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Domain_rejects_invalid_branch_and_time_invariants()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BranchConfiguration.Create(0, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BranchConfiguration.Create(1, TimeSpan.FromHours(24)));
    }
}
