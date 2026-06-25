using Qcontrol.Application.Features.Displays.Command.CreateDisplay;
using Qcontrol.Application.Features.Displays.Command.DeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.RestoreDisplay;
using Qcontrol.Application.Features.Displays.Command.UpdateDisplay;
using Qcontrol.Application.Features.Displays.Query.GetDeletedDisplays;
using Qcontrol.Application.Features.Displays.Query.GetDisplayById;
using Qcontrol.Application.Features.Displays.Query.GetDisplays;

namespace QControl.Application.Tests.Displays;

public sealed class DisplayValidatorTests
{
    [Fact]
    public void Create_validator_rejects_required_fields_and_non_ipv4()
    {
        var validator = new CreateDisplayCommandValidator();

        var result = validator.Validate(
            new CreateDisplayCommand
            {
                BranchId = 0,
                Number = " ",
                IPAddress = "2001:db8::1",
                SerialNo = " ",
                Type = " "
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateDisplayCommand.BranchId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateDisplayCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateDisplayCommand.IPAddress));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateDisplayCommand.SerialNo));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateDisplayCommand.Type));
    }

    [Theory]
    [InlineData("999.168.1.30")]
    [InlineData("192.168.1")]
    [InlineData("display-ip")]
    public void Create_validator_rejects_malformed_ipv4(string ipAddress)
    {
        var validator = new CreateDisplayCommandValidator();

        var result = validator.Validate(
            new CreateDisplayCommand
            {
                BranchId = 1,
                Number = "D-01",
                IPAddress = ipAddress,
                SerialNo = "DISPLAY-SN-001",
                Type = "LED Display"
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateDisplayCommand.IPAddress));
    }

    [Fact]
    public void Update_validator_rejects_route_body_mismatch_and_invalid_lengths()
    {
        var validator = new UpdateDisplayCommandValidator();

        var result = validator.Validate(
            new UpdateDisplayCommand
            {
                Id = 1,
                RequestId = 2,
                Number = new string('A', 21),
                IPAddress = "192.168.1.30",
                SerialNo = new string('S', 101),
                Type = new string('T', 101)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage.Contains("match"));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateDisplayCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateDisplayCommand.SerialNo));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateDisplayCommand.Type));
    }

    [Fact]
    public void Query_validators_reject_invalid_pagination_and_branch_filter()
    {
        var activeValidator = new GetDisplaysQueryValidator();
        var deletedValidator = new GetDeletedDisplaysQueryValidator();

        var activeResult = activeValidator.Validate(
            new GetDisplaysQuery
            {
                BranchId = 0,
                PageNumber = 0,
                PageSize = 101
            });
        var deletedResult = deletedValidator.Validate(
            new GetDeletedDisplaysQuery
            {
                BranchId = 0,
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(activeResult.IsValid);
        Assert.False(deletedResult.IsValid);
        Assert.Contains(
            activeResult.Errors,
            error => error.PropertyName == nameof(GetDisplaysQuery.BranchId));
        Assert.Contains(
            deletedResult.Errors,
            error => error.PropertyName == nameof(GetDeletedDisplaysQuery.BranchId));
    }

    [Fact]
    public void Id_validators_reject_invalid_ids()
    {
        Assert.False(new GetDisplayByIdQueryValidator()
            .Validate(new GetDisplayByIdQuery { Id = 0 })
            .IsValid);
        Assert.False(new DeleteDisplayCommandValidator()
            .Validate(new DeleteDisplayCommand { Id = 0 })
            .IsValid);
        Assert.False(new RestoreDisplayCommandValidator()
            .Validate(new RestoreDisplayCommand { Id = 0 })
            .IsValid);
        Assert.False(new PermanentDeleteDisplayCommandValidator()
            .Validate(new PermanentDeleteDisplayCommand { Id = 0 })
            .IsValid);
    }
}
