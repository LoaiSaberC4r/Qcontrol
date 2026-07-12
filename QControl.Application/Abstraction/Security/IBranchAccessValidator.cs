using BuildingBlock.Domain.Results;

namespace QControl.Application.Abstraction.Security;

public interface IBranchAccessValidator
{
    Result EnsureCanAccessBranch(
        int branchId,
        string codePrefix);

    Result EnsureBranchContext(
        string codePrefix);
}
