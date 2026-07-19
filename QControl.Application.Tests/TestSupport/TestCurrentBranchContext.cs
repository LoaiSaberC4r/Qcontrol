using QControl.Application.Abstraction.Security;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestCurrentBranchContext : ICurrentBranchContext
{
    public UserType? UserType { get; init; } = QControl.Domain.Enums.UserType.TechnicalAdmin;

    public int? ActiveBranchId { get; init; }

    public bool IsSystemLevelActor =>
        UserType == QControl.Domain.Enums.UserType.TechnicalAdmin;

    public bool IsBranchActor =>
        UserType == QControl.Domain.Enums.UserType.BranchAdmin;
}
