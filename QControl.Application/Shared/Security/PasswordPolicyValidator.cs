using BuildingBlock.Domain.Results;
using Microsoft.Extensions.Options;
using QControl.Application.Abstraction.Security;
using QControl.Application.Options;

namespace QControl.Application.Shared.Security;

internal sealed class PasswordPolicyValidator : IPasswordPolicyValidator
{
    private readonly PasswordPolicyOptions _options;

    public PasswordPolicyValidator(IOptions<PasswordPolicyOptions> options)
    {
        _options = options?.Value
            ?? throw new ArgumentNullException(nameof(options));
    }

    public Result Validate(string password, string codePrefix)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Fail(codePrefix, IdentityFeatureMessages.PasswordRequired);
        }

        if (password.Length > _options.MaxLength)
        {
            return Fail(codePrefix, IdentityFeatureMessages.PasswordMaxLength);
        }

        if (password.Length < _options.RequiredLength)
        {
            return Fail(codePrefix, IdentityFeatureMessages.PasswordPolicy);
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            return Fail(codePrefix, IdentityFeatureMessages.PasswordPolicy);
        }

        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            return Fail(codePrefix, IdentityFeatureMessages.PasswordPolicy);
        }

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            return Fail(codePrefix, IdentityFeatureMessages.PasswordPolicy);
        }

        if (_options.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
        {
            return Fail(codePrefix, IdentityFeatureMessages.PasswordPolicy);
        }

        return Result.Ok();
    }

    private static Result Fail(string codePrefix, string message) =>
        Result.Fail(new Error(
            Code: $"{codePrefix}.PasswordPolicy",
            Message: message,
            Type: ErrorType.Validation));
}
