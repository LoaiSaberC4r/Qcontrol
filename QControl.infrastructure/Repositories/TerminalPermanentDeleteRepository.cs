using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class TerminalPermanentDeleteRepository
    : ITerminalPermanentDeleteRepository
{
    private readonly PlatformWriteDbContext _dbContext;

    public TerminalPermanentDeleteRepository(
        PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<int> DeletePermanentlyAsync(
        int terminalId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.Set<Terminal>()
                .IgnoreQueryFilters()
                .Where(x => x.Id == terminalId && !x.IsActive)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (IsForeignKeyViolation(ex))
        {
            throw new TerminalPermanentDeleteConflictException(ex);
        }
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
        => exception.InnerException is SqlException { Number: 547 };
}
