using FluentValidation;
using Qcontrol.Application.Features.BranchConfigurations.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.CreateBranchConfiguration;

internal sealed class CreateBranchConfigurationCommandValidator
    : AbstractValidator<CreateBranchConfigurationCommand>
{
    public CreateBranchConfigurationCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required)
            .WithErrorCode("BranchConfigurations.Create.BranchIdInvalid");

        RuleFor(x => x.AllowedTime)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(
                BranchConfigurationFeatureMessages.AllowedTimeRequired)
            .WithErrorCode(
                "BranchConfigurations.Create.AllowedTimeRequired")
            .Must(value =>
                value >= TimeSpan.Zero &&
                value < TimeSpan.FromDays(1))
            .WithMessage(
                BranchConfigurationFeatureMessages.AllowedTimeOutOfRange)
            .WithErrorCode(
                "BranchConfigurations.Create.AllowedTimeOutOfRange");

        RuleFor(x => x.MaximumTicketCallAttempts).GreaterThan(0);
        RuleFor(x => x.TicketNoShowAutoCancellationMinutes).GreaterThan(0);
        RuleFor(x => x.TicketArchiveRetentionDays).GreaterThan(0);
    }
}
