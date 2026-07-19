using BuildingBlock.Domain.Results;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestServiceVisibilityPolicy : IServiceVisibilityPolicy
{
    public bool CanViewResult { get; init; } = true;

    public Result EnsureCanUseVisibilityContext(string codePrefix) =>
        Result.Ok();

    public IQueryable<Service> ApplyVisibleServices(IQueryable<Service> query) =>
        CanViewResult ? query : query.Where(_ => false);

    public bool CanView(ServiceScope scope, int? ownerBranchId) =>
        CanViewResult;

    public Result EnsureCanView(
        ServiceScope scope,
        int? ownerBranchId,
        string codePrefix)
    {
        return CanViewResult
            ? Result.Ok()
            : Result.Fail(new Error(
                $"{codePrefix}.Forbidden",
                "Forbidden.",
                ErrorType.Security));
    }
}
