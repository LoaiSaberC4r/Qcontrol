using BuildingBlock.Domain.Results;
using QControl.Application.Abstraction.Security;

namespace QControl.Application.Shared.Security;

internal sealed class BranchAccessValidator : IBranchAccessValidator
{
    private readonly ICurrentBranchContext _currentBranchContext;

    public BranchAccessValidator(ICurrentBranchContext currentBranchContext)
    {
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
    }

    public Result EnsureCanAccessBranch(
        int branchId,
        string codePrefix)
    {
        if (_currentBranchContext.IsSystemLevelActor)
        {
            return Result.Ok();
        }

        var branchContextResult = EnsureBranchContext(codePrefix);
        if (branchContextResult.IsFailure)
        {
            return branchContextResult;
        }

        return _currentBranchContext.ActiveBranchId == branchId
            ? Result.Ok()
            : Result.Fail(new Error(
                Code: $"{codePrefix}.BranchScopeForbidden",
                Message: IdentityFeatureMessages.BranchScopeForbidden,
                Type: ErrorType.Security));
    }

    public Result EnsureBranchContext(string codePrefix)
    {
        if (_currentBranchContext.IsSystemLevelActor)
        {
            return Result.Ok();
        }

        if (!_currentBranchContext.IsBranchActor)
        {
            return Result.Fail(new Error(
                Code: $"{codePrefix}.UnsupportedActorType",
                Message: IdentityFeatureMessages.UnsupportedActorType,
                Type: ErrorType.Security));
        }

        if (!_currentBranchContext.ActiveBranchId.HasValue)
        {
            return Result.Fail(new Error(
                Code: $"{codePrefix}.ActiveBranchRequired",
                Message: IdentityFeatureMessages.ActiveBranchRequired,
                Type: ErrorType.Security));
        }

        return Result.Ok();
    }
}
