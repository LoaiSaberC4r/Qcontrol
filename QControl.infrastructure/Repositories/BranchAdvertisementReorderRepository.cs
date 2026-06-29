using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class BranchAdvertisementReorderRepository
    : IBranchAdvertisementReorderRepository
{
    private readonly PlatformWriteDbContext _dbContext;

    public BranchAdvertisementReorderRepository(
        PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<BranchAdvertisementReorderResult> ReorderAsync(
        int branchId,
        Guid lastModifiedByApplicationUserId,
        IReadOnlyCollection<BranchAdvertisementReorderItem> items,
        CancellationToken cancellationToken)
    {
        var advertisements = await _dbContext.Set<BranchAdvertisement>()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        var validationStatus = ValidateRequest(advertisements, items);
        if (validationStatus != BranchAdvertisementReorderStatus.Success)
        {
            return Failure(validationStatus);
        }

        var itemsById = items.ToDictionary(x => x.AdvertisementId);

        foreach (var advertisement in advertisements)
        {
            var requested = itemsById[advertisement.Id];
            if (!advertisement.RowVersion.SequenceEqual(requested.RowVersion))
            {
                return Failure(
                    BranchAdvertisementReorderStatus.ConcurrencyConflict);
            }

            _dbContext.Entry(advertisement)
                .Property(x => x.RowVersion)
                .OriginalValue = requested.RowVersion;
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var tempOrder = 1001;
            foreach (var advertisement in advertisements)
            {
                var displayOrder = tempOrder++;
                _dbContext.Entry(advertisement)
                    .Property(x => x.DisplayOrder)
                    .CurrentValue = displayOrder;
                _dbContext.Entry(advertisement)
                    .Property(x => x.DisplayOrder)
                    .IsModified = true;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var advertisement in advertisements)
            {
                var requested = itemsById[advertisement.Id];
                advertisement.ChangeDisplayOrder(
                    requested.DisplayOrder,
                    lastModifiedByApplicationUserId);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(
                BranchAdvertisementReorderStatus.ConcurrencyConflict);
        }
        catch (DbUpdateException ex)
            when (IsUniqueViolation(ex))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(BranchAdvertisementReorderStatus.OrderConflict);
        }

        var ordered = advertisements
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        return new BranchAdvertisementReorderResult(
            BranchAdvertisementReorderStatus.Success,
            ordered);
    }

    private static BranchAdvertisementReorderStatus ValidateRequest(
        IReadOnlyCollection<BranchAdvertisement> advertisements,
        IReadOnlyCollection<BranchAdvertisementReorderItem> items)
    {
        if (advertisements.Count != items.Count)
        {
            return BranchAdvertisementReorderStatus.Incomplete;
        }

        if (items.Select(x => x.AdvertisementId).Distinct().Count() !=
            items.Count)
        {
            return BranchAdvertisementReorderStatus.Incomplete;
        }

        if (items.Select(x => x.DisplayOrder).Distinct().Count() !=
            items.Count)
        {
            return BranchAdvertisementReorderStatus.DuplicateDisplayOrder;
        }

        if (items.Any(x => x.DisplayOrder is < 1 or > 20))
        {
            return BranchAdvertisementReorderStatus.InvalidDisplayOrder;
        }

        var expectedIds = advertisements
            .Select(x => x.Id)
            .OrderBy(x => x)
            .ToArray();

        var requestedIds = items
            .Select(x => x.AdvertisementId)
            .OrderBy(x => x)
            .ToArray();

        if (!expectedIds.SequenceEqual(requestedIds))
        {
            return BranchAdvertisementReorderStatus.Incomplete;
        }

        var expectedOrders = Enumerable
            .Range(1, items.Count)
            .ToArray();

        var requestedOrders = items
            .Select(x => x.DisplayOrder)
            .OrderBy(x => x)
            .ToArray();

        return expectedOrders.SequenceEqual(requestedOrders)
            ? BranchAdvertisementReorderStatus.Success
            : BranchAdvertisementReorderStatus.Incomplete;
    }

    private static BranchAdvertisementReorderResult Failure(
        BranchAdvertisementReorderStatus status)
    {
        return new BranchAdvertisementReorderResult(
            status,
            Array.Empty<BranchAdvertisement>());
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
