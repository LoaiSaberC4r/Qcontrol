using BuildingBlock.Domain.Results;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Abstraction.Security;

public interface IServiceVisibilityPolicy
{
    Result EnsureCanUseVisibilityContext(string codePrefix);

    IQueryable<Service> ApplyVisibleServices(IQueryable<Service> query);

    bool CanView(ServiceScope scope, int? ownerBranchId);

    Result EnsureCanView(
        ServiceScope scope,
        int? ownerBranchId,
        string codePrefix);
}
