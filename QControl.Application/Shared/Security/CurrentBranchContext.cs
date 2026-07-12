using BuildingBlock.Application.Abstraction.Security;
using QControl.Application.Abstraction.Security;

namespace QControl.Application.Shared.Security;

internal sealed class CurrentBranchContext : ICurrentBranchContext
{
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTokenContext _currentTokenContext;

    public CurrentBranchContext(
        ICurrentUser currentUser,
        ICurrentTokenContext currentTokenContext)
    {
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentTokenContext = currentTokenContext
            ?? throw new ArgumentNullException(nameof(currentTokenContext));
    }

    public QControl.Domain.Enums.UserType? UserType =>
        _currentUser.UserTypeValue.HasValue &&
        Enum.IsDefined(typeof(QControl.Domain.Enums.UserType), _currentUser.UserTypeValue.Value)
            ? (QControl.Domain.Enums.UserType)_currentUser.UserTypeValue.Value
            : null;

    public int? ActiveBranchId => _currentTokenContext.ActiveBranchId;

    public bool IsSystemLevelActor => UserType == QControl.Domain.Enums.UserType.TechnicalAdmin;

    public bool IsBranchActor => UserType == QControl.Domain.Enums.UserType.BranchAdmin;
}
