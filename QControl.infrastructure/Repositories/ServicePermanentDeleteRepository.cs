using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class ServicePermanentDeleteRepository
    : IServicePermanentDeleteRepository
{
    private readonly PlatformWriteDbContext _dbContext;

    public ServicePermanentDeleteRepository(
        PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task DeletePermanentlyAsync(
        int serviceId,
        byte[] rowVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            var deletedRows = await _dbContext.Set<Service>()
                .IgnoreQueryFilters()
                .Where(service =>
                    service.Id == serviceId &&
                    service.Scope == ServiceScope.BranchScoped &&
                    service.IsDeleted &&
                    service.RowVersion == rowVersion)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows == 0)
            {
                throw new ServicePermanentDeleteConcurrencyException();
            }
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: 547 })
        {
            throw new ServicePermanentDeleteConflictException(exception);
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            throw new ServicePermanentDeleteConflictException(exception);
        }
    }
}
