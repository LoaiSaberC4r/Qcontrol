using BuildingBlock.Domain.Results;
using QControl.Application.Abstraction.Security;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestBranchAccessValidator : IBranchAccessValidator
{
    public Result Result { get; init; } = Result.Ok();

    public int? LastBranchId { get; private set; }

    public string? LastCodePrefix { get; private set; }

    public Result EnsureCanAccessBranch(
        int branchId,
        string codePrefix)
    {
        LastBranchId = branchId;
        LastCodePrefix = codePrefix;

        return Result;
    }

    public Result EnsureBranchContext(string codePrefix)
    {
        LastCodePrefix = codePrefix;

        return Result;
    }
}
