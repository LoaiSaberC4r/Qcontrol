using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Services.Command.DeleteService;

internal sealed class DeleteServiceCommandValidator
    : AbstractValidator<DeleteServiceCommand>
{
    public DeleteServiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired);

        RuleFor(x => x.RowVersion)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
