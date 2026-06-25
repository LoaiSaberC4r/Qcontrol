using Qcontrol.Application.Features.Terminals.Command.CreateTerminal;
using Qcontrol.Application.Features.Terminals.Command.DeleteTerminal;
using Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;
using Qcontrol.Application.Features.Terminals.Command.RestoreTerminal;
using Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;
using Qcontrol.Application.Features.Terminals.Query.GetDeletedTerminals;
using Qcontrol.Application.Features.Terminals.Query.GetTerminalById;
using Qcontrol.Application.Features.Terminals.Query.GetTerminals;

namespace QControl.Application.Tests.Terminals;

public sealed class TerminalValidatorTests
{
    [Fact]
    public void Create_validator_rejects_required_fields_and_non_ipv4()
    {
        var validator = new CreateTerminalCommandValidator();

        var result = validator.Validate(
            new CreateTerminalCommand
            {
                WindowId = 0,
                Number = " ",
                IPAddress = "2001:db8::1",
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

    [Theory]
    [InlineData("999.168.1.1")]
    [InlineData("192.168.1")]
    [InlineData("terminal-ip")]
    public void Create_validator_rejects_malformed_ipv4(string ipAddress)
    {
        var validator = new CreateTerminalCommandValidator();

        var result = validator.Validate(
            new CreateTerminalCommand
            {
                WindowId = 1,
                Number = "T-01",
                IPAddress = ipAddress,
                SerialNo = "ABC-100",
                Type = "Operator Module"
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateTerminalCommand.IPAddress));
    }

    [Fact]
    public void Update_validator_rejects_route_body_mismatch_and_invalid_lengths()
    {
        var validator = new UpdateTerminalCommandValidator();

        var result = validator.Validate(
            new UpdateTerminalCommand
            {
                Id = 1,
                RequestId = 2,
                Number = new string('A', 21),
                IPAddress = "192.168.1.20",
                SerialNo = new string('S', 101),
                Type = new string('T', 101)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage.Contains("match"));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateTerminalCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateTerminalCommand.SerialNo));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateTerminalCommand.Type));
    }

    [Fact]
    public void Query_validators_reject_invalid_pagination_and_window_filter()
    {
        var activeValidator = new GetTerminalsQueryValidator();
        var deletedValidator = new GetDeletedTerminalsQueryValidator();

        var activeResult = activeValidator.Validate(
            new GetTerminalsQuery
            {
                WindowId = 0,
                PageNumber = 0,
                PageSize = 101
            });
        var deletedResult = deletedValidator.Validate(
            new GetDeletedTerminalsQuery
            {
                WindowId = 0,
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(activeResult.IsValid);
        Assert.False(deletedResult.IsValid);
        Assert.Contains(
            activeResult.Errors,
            error => error.PropertyName == nameof(GetTerminalsQuery.WindowId));
        Assert.Contains(
            deletedResult.Errors,
            error => error.PropertyName == nameof(GetDeletedTerminalsQuery.WindowId));
    }

    [Fact]
    public void Id_validators_reject_invalid_ids()
    {
        Assert.False(new GetTerminalByIdQueryValidator()
            .Validate(new GetTerminalByIdQuery { Id = 0 })
            .IsValid);
        Assert.False(new DeleteTerminalCommandValidator()
            .Validate(new DeleteTerminalCommand { Id = 0 })
            .IsValid);
        Assert.False(new RestoreTerminalCommandValidator()
            .Validate(new RestoreTerminalCommand { Id = 0 })
            .IsValid);
        Assert.False(new PermanentDeleteTerminalCommandValidator()
            .Validate(new PermanentDeleteTerminalCommand { Id = 0 })
            .IsValid);
    }
}
