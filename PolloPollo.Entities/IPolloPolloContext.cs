using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolloPollo.Entities
{
    public interface IPolloPolloContext : IDisposable
    {
        DbSet<User> Users { get; set; }
        DbSet<Producer> Producers { get; set; }
        DbSet<Receiver> Receivers { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<Application> Applications { get; set; }
        DbSet<Contracts> Contracts { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
