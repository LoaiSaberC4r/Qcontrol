using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class BranchDisplayMessageReorderRepository
    : IBranchDisplayMessageReorderRepository
{
    private readonly PlatformWriteDbContext _dbContext;

    public BranchDisplayMessageReorderRepository(PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BranchDisplayMessageReorderResult> ReorderAsync(
        int branchId,
        Guid lastModifiedByApplicationUserId,
        IReadOnlyCollection<BranchDisplayMessageReorderItem> items,
        CancellationToken cancellationToken)
    {
        var basicStatus = ValidateBasic(items);
        if (basicStatus != BranchDisplayMessageReorderStatus.Success)
        {
            return Failure(basicStatus);
        }

        var requestedIds = items.Select(x => x.MessageId).ToArray();
        var requestedMessages = await _dbContext.Set<BranchDisplayMessage>()
            .Where(x => requestedIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        if (requestedMessages.Count != items.Count)
        {
            return Failure(BranchDisplayMessageReorderStatus.NotFound);
        }
        if (requestedMessages.Any(x => x.BranchId != branchId))
        {
            return Failure(BranchDisplayMessageReorderStatus.WrongBranch);
        }

        var allBranchMessages = await _dbContext.Set<BranchDisplayMessage>()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
        var itemsById = items.ToDictionary(x => x.MessageId);
        var finalOrders = allBranchMessages
            .Select(x => itemsById.TryGetValue(x.Id, out var item)
                ? item.DisplayOrder
                : x.DisplayOrder)
            .ToArray();
        if (finalOrders.Distinct().Count() != finalOrders.Length)
        {
            return Failure(BranchDisplayMessageReorderStatus.OrderConflict);
        }

        foreach (var message in requestedMessages)
        {
            var requested = itemsById[message.Id];
            if (!message.RowVersion.SequenceEqual(requested.RowVersion))
            {
                return Failure(BranchDisplayMessageReorderStatus.ConcurrencyConflict);
            }
            _dbContext.Entry(message).Property(x => x.RowVersion).OriginalValue = requested.RowVersion;
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var reservedOrders = allBranchMessages.Select(x => x.DisplayOrder).ToHashSet();
            var temporaryOrder = int.MaxValue;
            foreach (var message in requestedMessages)
            {
                while (reservedOrders.Contains(temporaryOrder))
                {
                    temporaryOrder--;
                }
                _dbContext.Entry(message).Property(x => x.DisplayOrder).CurrentValue = temporaryOrder;
                _dbContext.Entry(message).Property(x => x.DisplayOrder).IsModified = true;
                reservedOrders.Add(temporaryOrder--);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            foreach (var message in requestedMessages)
            {
                message.ChangeDisplayOrder(
                    itemsById[message.Id].DisplayOrder,
                    lastModifiedByApplicationUserId);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(BranchDisplayMessageReorderStatus.ConcurrencyConflict);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(BranchDisplayMessageReorderStatus.OrderConflict);
        }

        return new BranchDisplayMessageReorderResult(
            BranchDisplayMessageReorderStatus.Success,
            allBranchMessages.OrderBy(x => x.DisplayOrder).ToList());
    }

    private static BranchDisplayMessageReorderStatus ValidateBasic(
        IReadOnlyCollection<BranchDisplayMessageReorderItem> items)
    {
        if (items.Count == 0 || items.Any(x => x.DisplayOrder <= 0))
        {
            return BranchDisplayMessageReorderStatus.InvalidDisplayOrder;
        }
        if (items.Select(x => x.MessageId).Distinct().Count() != items.Count)
        {
            return BranchDisplayMessageReorderStatus.DuplicateMessageId;
        }
        if (items.Select(x => x.DisplayOrder).Distinct().Count() != items.Count)
        {
            return BranchDisplayMessageReorderStatus.DuplicateDisplayOrder;
        }
        return BranchDisplayMessageReorderStatus.Success;
    }

    private static BranchDisplayMessageReorderResult Failure(
        BranchDisplayMessageReorderStatus status) =>
        new(status, Array.Empty<BranchDisplayMessage>());

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
