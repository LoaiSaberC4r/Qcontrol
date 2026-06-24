using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QControl.Application.Abstraction.Seeding
{
    public interface IEnsureSeeding
    {
        Task SeedDatabaseAsync();
    }
}