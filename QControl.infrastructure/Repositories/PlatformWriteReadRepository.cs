using BuildingBlock.Application.Abstraction.Persistence;
using QControl.Application.Abstraction.Presistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QControl.infrastructure.Repositories
{
    internal sealed class PlatformWriteReadRepository<TEntity>
    : BuildingBlock.Infrastracture.Repositories.EfReadRepository<TEntity, PlatformWriteReadMarker>,
      IWriteReadRepository<TEntity>
    where TEntity : class
    {
        public PlatformWriteReadRepository(IDbContextResolver<PlatformWriteReadMarker> resolver)
            : base(resolver) { }
    }
}