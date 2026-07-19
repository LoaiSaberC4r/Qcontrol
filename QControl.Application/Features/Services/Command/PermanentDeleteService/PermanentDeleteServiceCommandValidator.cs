using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Services.Command.PermanentDeleteService;

internal sealed class PermanentDeleteServiceCommandValidator
    : AbstractValidator<PermanentDeleteServiceCommand>
{
    public PermanentDeleteServiceCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired);

        RuleFor(command => command.RowVersion)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
