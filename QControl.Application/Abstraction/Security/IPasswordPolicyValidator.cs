using BuildingBlock.Domain.Results;

namespace QControl.Application.Abstraction.Security;

public interface IPasswordPolicyValidator
{
    Result Validate(string password, string codePrefix);
}
