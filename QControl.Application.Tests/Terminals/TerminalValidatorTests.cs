using Qcontrol.Application.Features.Terminals.Command.CreateTerminal;
using Qcontrol.Application.Features.Terminals.Command.DeactivateTerminal;
using Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;
using Qcontrol.Application.Features.Terminals.Command.ReactivateTerminal;
using Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;
using Qcontrol.Application.Features.Terminals.Query.GetTerminalById;
using Qcontrol.Application.Features.Terminals.Query.GetTerminals;

namespace QControl.Application.Tests.Terminals;

public sealed class TerminalValidatorTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public void Create_validator_rejects_required_fields_and_malformed_ip()
    {
        var validator = new CreateTerminalCommandValidator();

        var result = validator.Validate(
            new CreateTerminalCommand
            {
                WindowId = 0,
                Number = " ",
                IPAddress = "terminal-ip",
                SerialNo = " ",
                Type = " "
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateTerminalCommand.WindowId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateTerminalCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateTerminalCommand.IPAddress));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateTerminalCommand.SerialNo));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateTerminalCommand.Type));
    }

    [Fact]
    public void Update_validator_rejects_invalid_lengths_and_missing_rowversion()
    {
        var validator = new UpdateTerminalCommandValidator();

        var result = validator.Validate(
            new UpdateTerminalCommand
            {
                Id = 0,
                Number = new string('A', 21),
                IPAddress = "192.168.1.20",
                SerialNo = new string('S', 101),
                Type = new string('T', 101)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateTerminalCommand.Id));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateTerminalCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateTerminalCommand.SerialNo));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateTerminalCommand.Type));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateTerminalCommand.RowVersion));
    }

    [Fact]
    public void Query_validator_rejects_invalid_pagination_window_filter_and_long_search()
    {
        var validator = new GetTerminalsQueryValidator();

        var result = validator.Validate(
            new GetTerminalsQuery
            {
                WindowId = 0,
                Search = new string('x', 201),
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetTerminalsQuery.WindowId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetTerminalsQuery.Search));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetTerminalsQuery.PageNumber));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetTerminalsQuery.PageSize));
    }

    [Fact]
    public void Id_and_lifecycle_validators_reject_invalid_ids_or_rowversions()
    {
        Assert.False(new GetTerminalByIdQueryValidator()
            .Validate(new GetTerminalByIdQuery { Id = 0 })
            .IsValid);
        Assert.False(new DeactivateTerminalCommandValidator()
            .Validate(new DeactivateTerminalCommand
            {
                Id = 0,
                RowVersion = ValidRowVersion
            })
            .IsValid);
        Assert.False(new ReactivateTerminalCommandValidator()
            .Validate(new ReactivateTerminalCommand
            {
                Id = 0,
                RowVersion = ValidRowVersion
            })
            .IsValid);
        Assert.False(new PermanentDeleteTerminalCommandValidator()
            .Validate(new PermanentDeleteTerminalCommand
            {
                Id = 1,
                RowVersion = "not-base64"
            })
            .IsValid);
    }
}
