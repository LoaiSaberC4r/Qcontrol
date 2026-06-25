using FluentValidation;
using Qcontrol.Application.Features.Windows.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Windows.Command.UpdateWindow;

internal sealed class UpdateWindowCommandValidator
    : AbstractValidator<UpdateWindowCommand>
{
    public UpdateWindowCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Window_Id_Required);

        RuleFor(x => x.RequestId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Window_Id_Required);

        RuleFor(x => x)
            .Must(x => x.Id == x.RequestId)
            .WithMessage(ErrorMessage.Window_Id_Mismatch);

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
            .Must(WindowIPAddressValidation.BeValidIPAddress)
            .WithMessage(ErrorMessage.Window_IPAddress_Invalid);
    }
}
