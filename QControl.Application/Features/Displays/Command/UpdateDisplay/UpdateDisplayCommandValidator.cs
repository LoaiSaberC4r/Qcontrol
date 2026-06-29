using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Displays.Command.UpdateDisplay;

internal sealed class UpdateDisplayCommandValidator
    : AbstractValidator<UpdateDisplayCommand>
{
    public UpdateDisplayCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Display_Id_Required);

        RuleFor(x => x.Number)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Display_Number_Required)
            .MaximumLength(20)
            .WithMessage(ErrorMessage.Display_Number_MaxLength);

        RuleFor(x => x.IPAddress)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Display_IPAddress_Required)
            .MaximumLength(45)
            .WithMessage(ErrorMessage.Display_IPAddress_MaxLength)
            .Must(value => IPAddressNormalizer.TryNormalize(value, out _))
            .WithMessage(ErrorMessage.Display_IPAddress_Invalid);

        RuleFor(x => x.SerialNo)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Display_SerialNo_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Display_SerialNo_MaxLength);

        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Display_Type_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Display_Type_MaxLength);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
