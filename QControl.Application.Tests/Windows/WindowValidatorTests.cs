using Qcontrol.Application.Features.Windows.Command.CreateWindow;
using Qcontrol.Application.Features.Windows.Command.DeleteWindow;
using Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;
using Qcontrol.Application.Features.Windows.Command.UpdateWindow;
using Qcontrol.Application.Features.Windows.Query.GetDeletedWindows;
using Qcontrol.Application.Features.Windows.Query.GetWindowById;
using Qcontrol.Application.Features.Windows.Query.GetWindows;

namespace QControl.Application.Tests.Windows;

public sealed class WindowValidatorTests
{
    [Fact]
    public void Create_validator_rejects_invalid_fields()
    {
        var validator = new CreateWindowCommandValidator();

        var result = validator.Validate(
            new CreateWindowCommand
            {
                WaitingAreaId = 0,
                Number = "   ",
                DescriptiveName = new string('a', 101),
                IPAddress = new string('1', 46)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWindowCommand.WaitingAreaId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWindowCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWindowCommand.DescriptiveName));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWindowCommand.IPAddress));
    }

    [Fact]
    public void Create_validator_accepts_valid_optional_ip_and_rejects_invalid_ip()
    {
        var validator = new CreateWindowCommandValidator();

        var nullIpResult = validator.Validate(
            new CreateWindowCommand
            {
                WaitingAreaId = 1,
                Number = "1",
                IPAddress = null
            });
        var ipv6Result = validator.Validate(
            new CreateWindowCommand
            {
                WaitingAreaId = 1,
                Number = "2",
                IPAddress = "2001:db8::1"
            });
        var invalidResult = validator.Validate(
            new CreateWindowCommand
            {
                WaitingAreaId = 1,
                Number = "3",
                IPAddress = "999.1.1.1"
            });

        Assert.True(nullIpResult.IsValid);
        Assert.True(ipv6Result.IsValid);
        Assert.False(invalidResult.IsValid);
        Assert.Contains(
            invalidResult.Errors,
            error => error.PropertyName == nameof(CreateWindowCommand.IPAddress));
    }

    [Fact]
    public void Update_validator_rejects_invalid_fields_and_id_mismatch()
    {
        var validator = new UpdateWindowCommandValidator();

        var result = validator.Validate(
            new UpdateWindowCommand
            {
                Id = 1,
                RequestId = 2,
                Number = "",
                DescriptiveName = new string('a', 101),
                IPAddress = "bad-ip"
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == string.Empty);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWindowCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWindowCommand.DescriptiveName));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWindowCommand.IPAddress));
    }

    [Fact]
    public void Update_validator_rejects_missing_ids()
    {
        var validator = new UpdateWindowCommandValidator();

        var result = validator.Validate(
            new UpdateWindowCommand
            {
                Id = 0,
                RequestId = 0,
                Number = "1"
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWindowCommand.Id));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWindowCommand.RequestId));
    }

    [Fact]
    public void Delete_validator_rejects_invalid_id()
    {
        var validator = new DeleteWindowCommandValidator();

        var result = validator.Validate(
            new DeleteWindowCommand { Id = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(DeleteWindowCommand.Id));
    }

    [Fact]
    public void Permanent_delete_validator_rejects_invalid_id()
    {
        var validator = new PermanentDeleteWindowCommandValidator();

        var result = validator.Validate(
            new PermanentDeleteWindowCommand { Id = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(PermanentDeleteWindowCommand.Id));
    }

    [Fact]
    public void Details_validator_rejects_invalid_id()
    {
        var validator = new GetWindowByIdQueryValidator();

        var result = validator.Validate(
            new GetWindowByIdQuery { Id = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWindowByIdQuery.Id));
    }

    [Fact]
    public void Active_pagination_validator_rejects_invalid_filters_and_paging()
    {
        var validator = new GetWindowsQueryValidator();

        var result = validator.Validate(
            new GetWindowsQuery
            {
                WaitingAreaId = 0,
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWindowsQuery.WaitingAreaId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWindowsQuery.PageNumber));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWindowsQuery.PageSize));
    }

    [Fact]
    public void Deleted_pagination_validator_rejects_invalid_filters_and_paging()
    {
        var validator = new GetDeletedWindowsQueryValidator();

        var result = validator.Validate(
            new GetDeletedWindowsQuery
            {
                WaitingAreaId = 0,
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetDeletedWindowsQuery.WaitingAreaId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetDeletedWindowsQuery.PageNumber));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetDeletedWindowsQuery.PageSize));
    }
}
