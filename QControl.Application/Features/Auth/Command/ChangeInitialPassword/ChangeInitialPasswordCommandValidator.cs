using FluentValidation;
using QControl.Application.Abstraction.Security;

namespace Qcontrol.Application.Features.Auth.Command.ChangeInitialPassword;

internal sealed class ChangeInitialPasswordCommandValidator
    : AbstractValidator<ChangeInitialPasswordCommand>
{
    public ChangeInitialPasswordCommandValidator(
        IPasswordPolicyValidator passwordPolicyValidator)
    {
        RuleFor(x => x.NewPassword)
            .Custom((password, context) =>
            {
                var result = passwordPolicyValidator.Validate(
                    password,
                    "Auth.ChangeInitialPassword.NewPassword");

                if (result.IsFailure)
                {
                    foreach (var error in result.Errors)
                    {
                        context.AddFailure(error.Code, error.Message);
                    }
                }
            });
    }
}
