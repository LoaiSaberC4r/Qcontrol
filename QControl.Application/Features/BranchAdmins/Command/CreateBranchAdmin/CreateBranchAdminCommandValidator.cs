using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Security;

namespace Qcontrol.Application.Features.BranchAdmins.Command.CreateBranchAdmin;

internal sealed class CreateBranchAdminCommandValidator
    : AbstractValidator<CreateBranchAdminCommand>
{
    public CreateBranchAdminCommandValidator(
        IPasswordPolicyValidator passwordPolicyValidator)
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);

        RuleFor(x => x.UserName)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage(IdentityFeatureMessages.BranchAdminUserNameRequired)
            .MaximumLength(100)
            .WithMessage(IdentityFeatureMessages.BranchAdminUserNameMaxLength);

        RuleFor(x => x.Email)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage(IdentityFeatureMessages.BranchAdminEmailRequired)
            .EmailAddress()
            .WithMessage(IdentityFeatureMessages.BranchAdminEmailInvalid)
            .MaximumLength(200)
            .WithMessage(IdentityFeatureMessages.BranchAdminEmailMaxLength);

        RuleFor(x => x.NameEn)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage(IdentityFeatureMessages.BranchAdminNameEnRequired)
            .MaximumLength(200)
            .WithMessage(IdentityFeatureMessages.BranchAdminNameEnMaxLength);

        RuleFor(x => x.NameAr)
            .MaximumLength(200)
            .WithMessage(IdentityFeatureMessages.BranchAdminNameArMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30)
            .WithMessage(IdentityFeatureMessages.BranchAdminPhoneMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.TemporaryPassword)
            .Custom((password, context) =>
            {
                var result = passwordPolicyValidator.Validate(
                    password,
                    "BranchAdmins.Create.TemporaryPassword");

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
