using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Windows.Command.UpdateWindow;

internal sealed class UpdateWindowCommandValidator
    : AbstractValidator<UpdateWindowCommand>
{
    public UpdateWindowCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Window_Id_Required);

        RuleFor(x => x.Number)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Window_Number_Required)
            .MaximumLength(20)
            .WithMessage(ErrorMessage.Window_Number_MaxLength);

        RuleFor(x => x.DescriptiveName)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Window_DescriptiveName_MaxLength);

        RuleFor(x => x.IPAddress)
            .MaximumLength(45)
            .WithMessage(ErrorMessage.Window_IPAddress_MaxLength)
            .Must(value =>
                string.IsNullOrWhiteSpace(value) ||
                IPAddressNormalizer.TryNormalize(value, out _))
            .WithMessage(ErrorMessage.Window_IPAddress_Invalid);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
