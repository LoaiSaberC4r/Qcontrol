using QControl.Domain.Enums;

namespace QControl.Application.Abstraction.Security;

public interface ICurrentBranchContext
{
    UserType? UserType { get; }

    int? ActiveBranchId { get; }

    bool IsSystemLevelActor { get; }

    bool IsBranchActor { get; }
}
