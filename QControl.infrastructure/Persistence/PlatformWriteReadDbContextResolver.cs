using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;

namespace QControl.infrastructure.Persistence
{
    internal sealed class PlatformWriteReadDbContextResolver : IDbContextResolver<PlatformWriteReadMarker>
    {
        private readonly PlatformWriteDbContext _db;

        public PlatformWriteReadDbContextResolver(PlatformWriteDbContext db) => _db = db;

        public DbContext Resolve() => _db;
    }
}