using FluentValidation;
using Qcontrol.Application.Features.Terminals.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;

internal sealed class UpdateTerminalCommandValidator
    : AbstractValidator<UpdateTerminalCommand>
{
    public UpdateTerminalCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_Id_Required);

        RuleFor(x => x.RequestId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_Id_Required);

        RuleFor(x => x)
            .Must(x => x.Id == x.RequestId)
            .WithMessage(ErrorMessage.Terminal_Id_Mismatch);

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
            .Must(TerminalIPAddressValidation.BeValidIPv4)
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
