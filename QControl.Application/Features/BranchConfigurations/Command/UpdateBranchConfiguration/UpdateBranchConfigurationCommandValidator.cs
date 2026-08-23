using FluentValidation;
using Qcontrol.Application.Features.BranchConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;

internal sealed class UpdateBranchConfigurationCommandValidator
    : AbstractValidator<UpdateBranchConfigurationCommand>
{
    public UpdateBranchConfigurationCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required)
            .WithErrorCode("BranchConfigurations.Update.BranchIdInvalid");

        RuleFor(x => x.AllowedTime)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(
                BranchConfigurationFeatureMessages.AllowedTimeRequired)
            .WithErrorCode(
                "BranchConfigurations.Update.AllowedTimeRequired")
            .Must(value =>
                value >= TimeSpan.Zero &&
                value < TimeSpan.FromDays(1))
            .WithMessage(
                BranchConfigurationFeatureMessages.AllowedTimeOutOfRange)
            .WithErrorCode(
                "BranchConfigurations.Update.AllowedTimeOutOfRange");

        RuleFor(x => x.RowVersion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                BranchConfigurationFeatureMessages.RowVersionRequired)
            .WithErrorCode(
                "BranchConfigurations.Update.RowVersionRequired")
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(
                BranchConfigurationFeatureMessages.InvalidRowVersion)
            .WithErrorCode(
                "BranchConfigurations.Update.InvalidRowVersion");
    }
}
