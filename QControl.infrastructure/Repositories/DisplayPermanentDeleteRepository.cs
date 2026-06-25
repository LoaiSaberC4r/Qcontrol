using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class DisplayPermanentDeleteRepository
    : IDisplayPermanentDeleteRepository
{
    private readonly PlatformWriteDbContext _dbContext;

    public DisplayPermanentDeleteRepository(
        PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<int> DeletePermanentlyAsync(
        int displayId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.Set<Display>()
                .IgnoreQueryFilters()
                .Where(x => x.Id == displayId && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (IsForeignKeyViolation(ex))
        {
            throw new DisplayPermanentDeleteConflictException(ex);
        }
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
        => exception.InnerException is SqlException { Number: 547 };
}
