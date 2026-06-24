using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Auth.Command.Login;

internal sealed class LoginCommandValidator
    : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty()
            .WithMessage(ErrorMessage.Login_UserNameOrEmail_Required)
            .MaximumLength(200)
            .WithMessage(ErrorMessage.Login_UserNameOrEmail_MaxLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(ErrorMessage.Login_Password_Required)
            .MaximumLength(200)
            .WithMessage(ErrorMessage.Login_Password_MaxLength);
    }
}