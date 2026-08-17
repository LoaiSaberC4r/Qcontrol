using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class BranchVideoReorderRepository
    : IBranchVideoReorderRepository
{
    private readonly PlatformWriteDbContext _dbContext;

    public BranchVideoReorderRepository(PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BranchVideoReorderResult> ReorderAsync(
        int branchId,
        Guid lastModifiedByApplicationUserId,
        IReadOnlyCollection<BranchVideoReorderItem> items,
        CancellationToken cancellationToken)
    {
        var basicStatus = ValidateBasic(items);
        if (basicStatus != BranchVideoReorderStatus.Success)
        {
            return Failure(basicStatus);
        }

        var requestedIds = items.Select(x => x.VideoId).ToArray();
        var requestedVideos = await _dbContext.Set<BranchVideo>()
            .Where(x => requestedIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        if (requestedVideos.Count != items.Count)
        {
            return Failure(BranchVideoReorderStatus.NotFound);
        }
        if (requestedVideos.Any(x => x.BranchId != branchId))
        {
            return Failure(BranchVideoReorderStatus.WrongBranch);
        }

        var allBranchVideos = await _dbContext.Set<BranchVideo>()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
        var itemsById = items.ToDictionary(x => x.VideoId);

        var finalOrders = allBranchVideos
            .Select(x => itemsById.TryGetValue(x.Id, out var item)
                ? item.DisplayOrder
                : x.DisplayOrder)
            .ToArray();
        if (finalOrders.Distinct().Count() != finalOrders.Length)
        {
            return Failure(BranchVideoReorderStatus.OrderConflict);
        }

        foreach (var video in requestedVideos)
        {
            var requested = itemsById[video.Id];
            if (!video.RowVersion.SequenceEqual(requested.RowVersion))
            {
                return Failure(BranchVideoReorderStatus.ConcurrencyConflict);
            }
            _dbContext.Entry(video).Property(x => x.RowVersion).OriginalValue = requested.RowVersion;
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var reservedOrders = allBranchVideos.Select(x => x.DisplayOrder).ToHashSet();
            var temporaryOrder = int.MaxValue;
            foreach (var video in requestedVideos)
            {
                while (reservedOrders.Contains(temporaryOrder))
                {
                    temporaryOrder--;
                }
                _dbContext.Entry(video).Property(x => x.DisplayOrder).CurrentValue = temporaryOrder;
                _dbContext.Entry(video).Property(x => x.DisplayOrder).IsModified = true;
                reservedOrders.Add(temporaryOrder--);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            foreach (var video in requestedVideos)
            {
                video.ChangeDisplayOrder(
                    itemsById[video.Id].DisplayOrder,
                    lastModifiedByApplicationUserId);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(BranchVideoReorderStatus.ConcurrencyConflict);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(BranchVideoReorderStatus.OrderConflict);
        }

        return new BranchVideoReorderResult(
            BranchVideoReorderStatus.Success,
            allBranchVideos.OrderBy(x => x.DisplayOrder).ToList());
    }

    private static BranchVideoReorderStatus ValidateBasic(
        IReadOnlyCollection<BranchVideoReorderItem> items)
    {
        if (items.Count == 0 || items.Any(x => x.DisplayOrder <= 0))
        {
            return BranchVideoReorderStatus.InvalidDisplayOrder;
        }
        if (items.Select(x => x.VideoId).Distinct().Count() != items.Count)
        {
            return BranchVideoReorderStatus.DuplicateVideoId;
        }
        if (items.Select(x => x.DisplayOrder).Distinct().Count() != items.Count)
        {
            return BranchVideoReorderStatus.DuplicateDisplayOrder;
        }
        return BranchVideoReorderStatus.Success;
    }

    private static BranchVideoReorderResult Failure(BranchVideoReorderStatus status) =>
        new(status, Array.Empty<BranchVideo>());

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
