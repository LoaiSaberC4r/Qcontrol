using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Shared.Security;

internal sealed class ServiceVisibilityPolicy : IServiceVisibilityPolicy
{
    private readonly ICurrentBranchContext _currentBranchContext;

    public ServiceVisibilityPolicy(ICurrentBranchContext currentBranchContext)
    {
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
    }

    public Result EnsureCanUseVisibilityContext(string codePrefix)
    {
        if (_currentBranchContext.IsSystemLevelActor)
        {
            return Result.Ok();
        }

        if (!_currentBranchContext.IsBranchActor)
        {
            return Result.Fail(new Error(
                $"{codePrefix}.UnsupportedActorType",
                ServiceFeatureMessages.UnsupportedActorType,
                ErrorType.Security));
        }

        if (!_currentBranchContext.ActiveBranchId.HasValue)
        {
            return Result.Fail(new Error(
                $"{codePrefix}.ActiveBranchRequired",
                ServiceFeatureMessages.ActiveBranchRequired,
                ErrorType.Security));
        }

        return Result.Ok();
    }

    public IQueryable<Service> ApplyVisibleServices(IQueryable<Service> query)
    {
        if (_currentBranchContext.IsSystemLevelActor)
        {
            return query;
        }

        if (!_currentBranchContext.IsBranchActor ||
            !_currentBranchContext.ActiveBranchId.HasValue)
        {
            return query.Where(_ => false);
        }

        var branchId = _currentBranchContext.ActiveBranchId.Value;
        return query.Where(x =>
            x.Scope == ServiceScope.Global ||
            (x.Scope == ServiceScope.BranchScoped &&
             x.OwnerBranchId == branchId));
    }

    public bool CanView(ServiceScope scope, int? ownerBranchId)
    {
        if (_currentBranchContext.IsSystemLevelActor)
        {
            return true;
        }

        return
            _currentBranchContext.IsBranchActor &&
            _currentBranchContext.ActiveBranchId.HasValue &&
            (scope == ServiceScope.Global ||
             (scope == ServiceScope.BranchScoped &&
              ownerBranchId == _currentBranchContext.ActiveBranchId.Value));
    }

    public Result EnsureCanView(
        ServiceScope scope,
        int? ownerBranchId,
        string codePrefix)
    {
        var context = EnsureCanUseVisibilityContext(codePrefix);
        if (context.IsFailure)
        {
            return context;
        }

        return CanView(scope, ownerBranchId)
            ? Result.Ok()
            : Result.Fail(new Error(
                $"{codePrefix}.ForeignBranchServiceForbidden",
                ServiceFeatureMessages.ForeignBranchServiceForbidden,
                ErrorType.Security));
    }
}
