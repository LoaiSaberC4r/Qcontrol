using QControl.Application.Abstraction.Presistence;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class ConcurrencyTokenManager : IConcurrencyTokenManager
{
    private readonly PlatformWriteDbContext _dbContext;

    public ConcurrencyTokenManager(PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void SetOriginalRowVersion<TEntity>(
        TEntity entity,
        byte[] rowVersion)
        where TEntity : class
    {
        _dbContext.Entry(entity)
            .Property("RowVersion")
            .OriginalValue = rowVersion;
    }
}
