using Qcontrol.Application.Features.Displays.Command.CreateDisplay;
using Qcontrol.Application.Features.Displays.Command.DeactivateDisplay;
using Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;
using Qcontrol.Application.Features.Displays.Command.ReactivateDisplay;
using Qcontrol.Application.Features.Displays.Command.UpdateDisplay;
using Qcontrol.Application.Features.Displays.Query.GetDisplayById;
using Qcontrol.Application.Features.Displays.Query.GetDisplays;

namespace QControl.Application.Tests.Displays;

public sealed class DisplayValidatorTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public void Create_validator_rejects_required_fields_and_malformed_ip()
    {
        var validator = new CreateDisplayCommandValidator();

        var result = validator.Validate(
            new CreateDisplayCommand
            {
                BranchId = 0,
                Number = " ",
                IPAddress = "display-ip",
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

    [Fact]
    public void Update_validator_rejects_invalid_lengths_and_missing_rowversion()
    {
        var validator = new UpdateDisplayCommandValidator();

        var result = validator.Validate(
            new UpdateDisplayCommand
            {
                Id = 0,
                Number = new string('A', 21),
                IPAddress = "192.168.1.30",
                SerialNo = new string('S', 101),
                Type = new string('T', 101)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateDisplayCommand.Id));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateDisplayCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateDisplayCommand.SerialNo));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateDisplayCommand.Type));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateDisplayCommand.RowVersion));
    }

    [Fact]
    public void Query_validator_rejects_invalid_pagination_branch_filter_and_long_search()
    {
        var validator = new GetDisplaysQueryValidator();

        var result = validator.Validate(
            new GetDisplaysQuery
            {
                BranchId = 0,
                Search = new string('x', 201),
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetDisplaysQuery.BranchId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetDisplaysQuery.Search));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetDisplaysQuery.PageNumber));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetDisplaysQuery.PageSize));
    }

    [Fact]
    public void Id_and_lifecycle_validators_reject_invalid_ids_or_rowversions()
    {
        Assert.False(new GetDisplayByIdQueryValidator()
            .Validate(new GetDisplayByIdQuery { Id = 0 })
            .IsValid);
        Assert.False(new DeactivateDisplayCommandValidator()
            .Validate(new DeactivateDisplayCommand
            {
                Id = 0,
                RowVersion = ValidRowVersion
            })
            .IsValid);
        Assert.False(new ReactivateDisplayCommandValidator()
            .Validate(new ReactivateDisplayCommand
            {
                Id = 0,
                RowVersion = ValidRowVersion
            })
            .IsValid);
        Assert.False(new PermanentDeleteDisplayCommandValidator()
            .Validate(new PermanentDeleteDisplayCommand
            {
                Id = 1,
                RowVersion = "not-base64"
            })
            .IsValid);
    }
}
