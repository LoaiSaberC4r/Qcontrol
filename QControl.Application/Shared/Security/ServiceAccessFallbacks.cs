using BuildingBlock.Domain.Results;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Shared.Security;

internal sealed class SystemLevelServiceBranchContext : ICurrentBranchContext
{
    public static SystemLevelServiceBranchContext Instance { get; } = new();

    private SystemLevelServiceBranchContext()
    {
    }

    public UserType? UserType => QControl.Domain.Enums.UserType.TechnicalAdmin;

    public int? ActiveBranchId => null;

    public bool IsSystemLevelActor => true;

    public bool IsBranchActor => false;
}

internal sealed class AllowAllServiceDefinitionAccessValidator
    : IServiceDefinitionAccessValidator
{
    public static AllowAllServiceDefinitionAccessValidator Instance { get; } =
        new();

    private AllowAllServiceDefinitionAccessValidator()
    {
    }

    public Result EnsureCanEdit(
        Service service,
        string codePrefix)
        => Result.Ok();

    public Result EnsureCanCreateGlobal(string codePrefix)
        => Result.Ok();

    public Result EnsureCanCreateBranchScoped(
        int branchId,
        string codePrefix)
        => Result.Ok();

    public Result EnsureCanAccessTargetBranch(
        int branchId,
        string codePrefix)
        => Result.Ok();
}
