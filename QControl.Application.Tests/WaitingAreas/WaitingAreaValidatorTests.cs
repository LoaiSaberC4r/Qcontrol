using Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.DeleteWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreaById;
using Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;

namespace QControl.Application.Tests.WaitingAreas;

public sealed class WaitingAreaValidatorTests
{
    [Fact]
    public void Create_validator_rejects_invalid_fields()
    {
        var validator = new CreateWaitingAreaCommandValidator();

        var result = validator.Validate(
            new CreateWaitingAreaCommand
            {
                BranchId = 0,
                Number = 0,
                AudioDevice = new string('a', 101),
                ControlDevice = new string('b', 101),
                DescriptiveName = new string('c', 101)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWaitingAreaCommand.BranchId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWaitingAreaCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWaitingAreaCommand.AudioDevice));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWaitingAreaCommand.ControlDevice));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateWaitingAreaCommand.DescriptiveName));
    }

    [Fact]
    public void Create_validator_accepts_valid_optional_fields()
    {
        var validator = new CreateWaitingAreaCommandValidator();

        var result = validator.Validate(
            new CreateWaitingAreaCommand
            {
                BranchId = 1,
                Number = 1,
                AudioDevice = null,
                ControlDevice = null,
                DescriptiveName = null
            });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_validator_rejects_invalid_fields_and_id_mismatch()
    {
        var validator = new UpdateWaitingAreaCommandValidator();

        var result = validator.Validate(
            new UpdateWaitingAreaCommand
            {
                Id = 1,
                RequestId = 2,
                BranchId = 0,
                Number = 0,
                AudioDevice = new string('a', 101),
                ControlDevice = new string('b', 101),
                DescriptiveName = new string('c', 101)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == string.Empty);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.BranchId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.Number));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.AudioDevice));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.ControlDevice));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.DescriptiveName));
    }

    [Fact]
    public void Update_validator_rejects_missing_ids()
    {
        var validator = new UpdateWaitingAreaCommandValidator();

        var result = validator.Validate(
            new UpdateWaitingAreaCommand
            {
                Id = 0,
                RequestId = 0,
                BranchId = 1,
                Number = 1
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.Id));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.RequestId));
    }

    [Fact]
    public void Delete_validator_rejects_invalid_id()
    {
        var validator = new DeleteWaitingAreaCommandValidator();

        var result = validator.Validate(
            new DeleteWaitingAreaCommand { Id = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(DeleteWaitingAreaCommand.Id));
    }

    [Fact]
    public void GetById_validator_rejects_invalid_id()
    {
        var validator = new GetWaitingAreaByIdQueryValidator();

        var result = validator.Validate(
            new GetWaitingAreaByIdQuery { Id = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWaitingAreaByIdQuery.Id));
    }

    [Fact]
    public void List_validator_rejects_invalid_filters_and_paging()
    {
        var validator = new GetWaitingAreasQueryValidator();

        var result = validator.Validate(
            new GetWaitingAreasQuery
            {
                BranchId = 0,
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWaitingAreasQuery.BranchId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWaitingAreasQuery.PageNumber));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWaitingAreasQuery.PageSize));
    }
}
