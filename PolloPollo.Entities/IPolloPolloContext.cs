using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolloPollo.Entities
{
    public interface IPolloPolloContext : IDisposable
    {
        DbSet<DummyEntity> Dummies { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
