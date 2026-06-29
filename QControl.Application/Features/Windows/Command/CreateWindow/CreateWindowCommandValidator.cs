using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Windows.Command.CreateWindow;

internal sealed class CreateWindowCommandValidator
    : AbstractValidator<CreateWindowCommand>
{
    public CreateWindowCommandValidator()
    {
        RuleFor(x => x.WaitingAreaId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Window_WaitingAreaId_Required);

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
    }
}
