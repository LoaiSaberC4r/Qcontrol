using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServiceSegments.Shared;

internal sealed record BranchServiceSegmentContext(
    int BranchId,
    int ServiceId,
    int BranchServiceId,
    string? RangePrefix,
    int RangeStartNumber,
    int RangeEndNumber,
    int Capacity);

internal static class BranchServiceSegmentContextResolver
{
    public static async Task<Result<BranchServiceSegmentContext>> ResolveAsync(
        int branchId,
        int serviceId,
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<Service> services,
        IWriteReadRepository<BranchService> branchServices,
        ICurrentBranchContext branchContext,
        CancellationToken cancellationToken)
    {
        if (!branchContext.IsSystemLevelActor &&
            (!branchContext.IsBranchActor ||
             branchContext.ActiveBranchId != branchId))
        {
            return Failure(
                "BranchServiceSegments.BranchAccessForbidden",
                BranchServiceSegmentMessages.BranchAccessForbidden,
                ErrorType.Security);
        }

        var branchExists = await branches.Query()
            .AsNoTracking()
            .AnyAsync(x => x.Id == branchId, cancellationToken);
        if (!branchExists)
        {
            return Failure(
                "BranchServiceSegments.BranchNotFound",
                BranchServiceSegmentMessages.BranchNotFound,
                ErrorType.NotFound);
        }

        var service = await services.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == serviceId)
            .Select(x => new ServiceContextProjection
            {
                Id = x.Id,
                ParentServiceId = x.ParentServiceId,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                IsTicketIssuable = x.IsTicketIssuable,
                HasChildren = x.Children.Any(),
                RangePrefix = x.RangePrefix,
                RangeStartNumber = x.RangeStartNumber,
                RangeEndNumber = x.RangeEndNumber
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (service is null)
        {
            return Failure(
                "BranchServiceSegments.ServiceNotFound",
                BranchServiceSegmentMessages.ServiceNotFound,
                ErrorType.NotFound);
        }

        var branchServiceId = await branchServices.Query()
            .AsNoTracking()
            .Where(x =>
                x.BranchId == branchId &&
                x.ServiceId == serviceId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (branchServiceId <= 0)
        {
            return Failure(
                "BranchServiceSegments.ServiceNotAssigned",
                BranchServiceSegmentMessages.ServiceNotAssigned,
                ErrorType.Conflict);
        }

        if (service.HasChildren)
        {
            return Failure(
                "BranchServiceSegments.ServiceNotLeaf",
                BranchServiceSegmentMessages.ServiceNotLeaf,
                ErrorType.Validation);
        }

        if (!service.IsTicketIssuable)
        {
            return Failure(
                "BranchServiceSegments.ServiceNotTicketIssuable",
                BranchServiceSegmentMessages.ServiceNotTicketIssuable,
                ErrorType.Validation);
        }

        if (!service.IsActive || service.IsDeleted ||
            !await AreAncestorsAvailableAsync(
                service.ParentServiceId,
                services,
                cancellationToken))
        {
            return Failure(
                "BranchServiceSegments.ServiceUnavailable",
                BranchServiceSegmentMessages.ServiceUnavailable,
                ErrorType.Conflict);
        }

        var capacity =
            BranchServiceSegmentQuotaCalculator.CalculateCapacity(
                service.RangeStartNumber,
                service.RangeEndNumber);
        if (capacity.IsFailure)
        {
            return Result<BranchServiceSegmentContext>.Fail(
                capacity.Errors);
        }

        return Result<BranchServiceSegmentContext>.Ok(new(
            branchId,
            serviceId,
            branchServiceId,
            service.RangePrefix,
            service.RangeStartNumber.GetValueOrDefault(),
            service.RangeEndNumber.GetValueOrDefault(),
            capacity.Value));
    }

    private static async Task<bool> AreAncestorsAvailableAsync(
        int? parentServiceId,
        IWriteReadRepository<Service> services,
        CancellationToken cancellationToken)
    {
        if (!parentServiceId.HasValue)
        {
            return true;
        }

        var ancestors = await services.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.ParentServiceId,
                x.IsActive,
                x.IsDeleted
            })
            .ToListAsync(cancellationToken);
        var byId = ancestors.ToDictionary(x => x.Id);
        var current = parentServiceId;
        var visited = new HashSet<int>();
        while (current.HasValue)
        {
            if (!visited.Add(current.Value) ||
                !byId.TryGetValue(current.Value, out var parent) ||
                !parent.IsActive ||
                parent.IsDeleted)
            {
                return false;
            }

            current = parent.ParentServiceId;
        }

        return true;
    }

    private static Result<BranchServiceSegmentContext> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<BranchServiceSegmentContext>.Fail(
            new Error(code, message, type));

    private sealed class ServiceContextProjection
    {
        public int Id { get; init; }
        public int? ParentServiceId { get; init; }
        public bool IsActive { get; init; }
        public bool IsDeleted { get; init; }
        public bool IsTicketIssuable { get; init; }
        public bool HasChildren { get; init; }
        public string? RangePrefix { get; init; }
        public int? RangeStartNumber { get; init; }
        public int? RangeEndNumber { get; init; }
    }
}
