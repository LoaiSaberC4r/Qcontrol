using Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.DeactivateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.PermanentDeleteWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.ReactivateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;
using Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreaById;
using Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;

namespace QControl.Application.Tests.WaitingAreas;

public sealed class WaitingAreaValidatorTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

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
    public void Update_validator_rejects_invalid_fields_and_missing_rowversion()
    {
        var validator = new UpdateWaitingAreaCommandValidator();

        var result = validator.Validate(
            new UpdateWaitingAreaCommand
            {
                Id = 0,
                Number = 0,
                AudioDevice = new string('a', 101),
                ControlDevice = new string('b', 101),
                DescriptiveName = new string('c', 101)
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.Id));
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
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(UpdateWaitingAreaCommand.RowVersion));
    }

    [Fact]
    public void Lifecycle_validators_reject_invalid_id_or_rowversion()
    {
        Assert.False(new DeactivateWaitingAreaCommandValidator()
            .Validate(new DeactivateWaitingAreaCommand
            {
                Id = 0,
                RowVersion = ValidRowVersion
            })
            .IsValid);
        Assert.False(new ReactivateWaitingAreaCommandValidator()
            .Validate(new ReactivateWaitingAreaCommand
            {
                Id = 0,
                RowVersion = ValidRowVersion
            })
            .IsValid);
        Assert.False(new PermanentDeleteWaitingAreaCommandValidator()
            .Validate(new PermanentDeleteWaitingAreaCommand
            {
                Id = 1,
                RowVersion = "not-base64"
            })
            .IsValid);
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
    public void List_validator_rejects_invalid_filters_paging_and_long_search()
    {
        var validator = new GetWaitingAreasQueryValidator();

        var result = validator.Validate(
            new GetWaitingAreasQuery
            {
                BranchId = 0,
                Search = new string('x', 201),
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWaitingAreasQuery.BranchId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWaitingAreasQuery.Search));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWaitingAreasQuery.PageNumber));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetWaitingAreasQuery.PageSize));
    }
}
