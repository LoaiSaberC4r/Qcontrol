using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Shared.Security;

internal sealed class ServiceDefinitionAccessValidator
    : IServiceDefinitionAccessValidator
{
    private readonly ICurrentBranchContext _currentBranchContext;

    public ServiceDefinitionAccessValidator(
        ICurrentBranchContext currentBranchContext)
    {
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
    }

    public Result EnsureCanEdit(
        Service service,
        string codePrefix)
    {
        if (_currentBranchContext.IsSystemLevelActor)
        {
            return Result.Ok();
        }

        var branchContext = EnsureBranchActorContext(codePrefix);
        if (branchContext.IsFailure)
        {
            return branchContext;
        }

        if (service.Scope == ServiceScope.Global)
        {
            return Result.Fail(new Error(
                $"{codePrefix}.GlobalServiceForbiddenForBranchAdmin",
                ServiceFeatureMessages.GlobalServiceForbiddenForBranchAdmin,
                ErrorType.Security));
        }

        return service.OwnerBranchId == _currentBranchContext.ActiveBranchId
            ? Result.Ok()
            : Result.Fail(new Error(
                $"{codePrefix}.ForeignBranchServiceForbidden",
                ServiceFeatureMessages.ForeignBranchServiceForbidden,
                ErrorType.Security));
    }

    public Result EnsureCanCreateGlobal(string codePrefix)
    {
        return _currentBranchContext.IsSystemLevelActor
            ? Result.Ok()
            : Result.Fail(new Error(
                $"{codePrefix}.TechnicalAdminRequired",
                ServiceFeatureMessages.TechnicalAdminRequired,
                ErrorType.Security));
    }

    public Result EnsureCanCreateBranchScoped(
        int branchId,
        string codePrefix)
    {
        var branchContext = EnsureBranchActorContext(codePrefix);
        if (branchContext.IsFailure)
        {
            return branchContext;
        }

        return _currentBranchContext.ActiveBranchId == branchId
            ? Result.Ok()
            : Result.Fail(new Error(
                $"{codePrefix}.BranchAccessForbidden",
                ServiceFeatureMessages.BranchAccessForbidden,
                ErrorType.Security));
    }

    public Result EnsureCanAccessTargetBranch(
        int branchId,
        string codePrefix)
    {
        if (_currentBranchContext.IsSystemLevelActor)
        {
            return Result.Ok();
        }

        var branchContext = EnsureBranchActorContext(codePrefix);
        if (branchContext.IsFailure)
        {
            return branchContext;
        }

        return _currentBranchContext.ActiveBranchId == branchId
            ? Result.Ok()
            : Result.Fail(new Error(
                $"{codePrefix}.BranchAccessForbidden",
                ServiceFeatureMessages.BranchAccessForbidden,
                ErrorType.Security));
    }

    private Result EnsureBranchActorContext(string codePrefix)
    {
        if (!_currentBranchContext.IsBranchActor)
        {
            return Result.Fail(new Error(
                $"{codePrefix}.BranchAdminRequired",
                ServiceFeatureMessages.BranchAdminRequired,
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
}
