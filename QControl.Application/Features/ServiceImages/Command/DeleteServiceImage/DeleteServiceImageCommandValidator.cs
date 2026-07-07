using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceImages.Command.DeleteServiceImage;

internal sealed class DeleteServiceImageCommandValidator
    : AbstractValidator<DeleteServiceImageCommand>
{
    public DeleteServiceImageCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired);

        RuleFor(x => x.ImageId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.ImageIdRequired);

        RuleFor(x => x.RowVersion)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
