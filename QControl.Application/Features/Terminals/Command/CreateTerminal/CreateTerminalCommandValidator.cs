using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Terminals.Command.CreateTerminal;

internal sealed class CreateTerminalCommandValidator
    : AbstractValidator<CreateTerminalCommand>
{
    public CreateTerminalCommandValidator()
    {
        RuleFor(x => x.WindowId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_WindowId_Required);

        RuleFor(x => x.Number)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Terminal_Number_Required)
            .MaximumLength(20)
            .WithMessage(ErrorMessage.Terminal_Number_MaxLength);

        RuleFor(x => x.IPAddress)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Terminal_IPAddress_Required)
            .MaximumLength(45)
            .WithMessage(ErrorMessage.Terminal_IPAddress_MaxLength)
            .Must(value => IPAddressNormalizer.TryNormalize(value, out _))
            .WithMessage(ErrorMessage.Terminal_IPAddress_Invalid);

        RuleFor(x => x.SerialNo)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Terminal_SerialNo_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Terminal_SerialNo_MaxLength);

        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(ErrorMessage.Terminal_Type_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Terminal_Type_MaxLength);
    }
}
