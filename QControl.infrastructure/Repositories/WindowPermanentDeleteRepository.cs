using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class WindowPermanentDeleteRepository
    : IWindowPermanentDeleteRepository
{
    private readonly PlatformWriteDbContext _dbContext;

    public WindowPermanentDeleteRepository(
        PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<int> DeletePermanentlyAsync(
        int windowId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.Set<Window>()
                .IgnoreQueryFilters()
                .Where(x => x.Id == windowId && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (IsForeignKeyViolation(ex))
        {
            throw new WindowPermanentDeleteConflictException(ex);
        }
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
        => exception.InnerException is SqlException { Number: 547 };
}
