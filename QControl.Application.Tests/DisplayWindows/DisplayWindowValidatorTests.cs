using Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;
using Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;
using Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;
using Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;

namespace QControl.Application.Tests.DisplayWindows;

public sealed class DisplayWindowValidatorTests
{
    [Fact]
    public void Assign_validator_rejects_invalid_ids_and_accepts_valid_command()
    {
        var validator = new AssignWindowToDisplayCommandValidator();

        var invalid = validator.Validate(
            new AssignWindowToDisplayCommand
            {
                DisplayId = 0,
                WindowId = 0
            });
        var valid = validator.Validate(
            new AssignWindowToDisplayCommand
            {
                DisplayId = 1,
                WindowId = 2
            });

        Assert.False(invalid.IsValid);
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(AssignWindowToDisplayCommand.DisplayId));
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(AssignWindowToDisplayCommand.WindowId));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void Unassign_validator_rejects_invalid_ids_and_accepts_valid_command()
    {
        var validator = new UnassignWindowFromDisplayCommandValidator();

        var invalid = validator.Validate(
            new UnassignWindowFromDisplayCommand
            {
                DisplayId = -1,
                WindowId = -1
            });
        var valid = validator.Validate(
            new UnassignWindowFromDisplayCommand
            {
                DisplayId = 1,
                WindowId = 2
            });

        Assert.False(invalid.IsValid);
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(UnassignWindowFromDisplayCommand.DisplayId));
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(UnassignWindowFromDisplayCommand.WindowId));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void Linked_query_validator_rejects_invalid_pagination_and_accepts_valid_query()
    {
        var validator = new GetDisplayLinkedWindowsQueryValidator();

        var invalid = validator.Validate(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = 0,
                PageNumber = 0,
                PageSize = 101
            });
        var invalidPageSize = validator.Validate(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = 1,
                PageNumber = 1,
                PageSize = 0
            });
        var valid = validator.Validate(
            new GetDisplayLinkedWindowsQuery
            {
                DisplayId = 1,
                PageNumber = 1,
                PageSize = 100
            });

        Assert.False(invalid.IsValid);
        Assert.False(invalidPageSize.IsValid);
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(GetDisplayLinkedWindowsQuery.DisplayId));
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(GetDisplayLinkedWindowsQuery.PageNumber));
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(GetDisplayLinkedWindowsQuery.PageSize));
        Assert.Contains(
            invalidPageSize.Errors,
            error => error.PropertyName == nameof(GetDisplayLinkedWindowsQuery.PageSize));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void Available_query_validator_rejects_invalid_pagination_and_accepts_valid_query()
    {
        var validator = new GetDisplayAvailableWindowsQueryValidator();

        var invalid = validator.Validate(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = 0,
                PageNumber = 0,
                PageSize = 101
            });
        var invalidPageSize = validator.Validate(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = 1,
                PageNumber = 1,
                PageSize = 0
            });
        var valid = validator.Validate(
            new GetDisplayAvailableWindowsQuery
            {
                DisplayId = 1,
                PageNumber = 1,
                PageSize = 100
            });

        Assert.False(invalid.IsValid);
        Assert.False(invalidPageSize.IsValid);
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(GetDisplayAvailableWindowsQuery.DisplayId));
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(GetDisplayAvailableWindowsQuery.PageNumber));
        Assert.Contains(
            invalid.Errors,
            error => error.PropertyName == nameof(GetDisplayAvailableWindowsQuery.PageSize));
        Assert.Contains(
            invalidPageSize.Errors,
            error => error.PropertyName == nameof(GetDisplayAvailableWindowsQuery.PageSize));
        Assert.True(valid.IsValid);
    }
}
