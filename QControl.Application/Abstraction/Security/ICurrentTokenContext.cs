namespace QControl.Application.Abstraction.Security;

public interface ICurrentTokenContext
{
    int? ActiveBranchId { get; }

    bool PasswordChangeRequired { get; }
}
