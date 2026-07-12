using BuildingBlock.Domain.Results;
using QControl.Domain.Entities;

namespace QControl.Application.Abstraction.Security;

public interface IServiceDefinitionAccessValidator
{
    Result EnsureCanEdit(
        Service service,
        string codePrefix);

    Result EnsureCanCreateGlobal(string codePrefix);

    Result EnsureCanCreateBranchScoped(
        int branchId,
        string codePrefix);

    Result EnsureCanAccessTargetBranch(
        int branchId,
        string codePrefix);
}
