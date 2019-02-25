using Microsoft.EntityFrameworkCore;

namespace PolloPollo.Entities
{
    public class PolloPolloContext : DbContext, IPolloPolloContext
    {
        public DbSet<DummyEntity> Dummies { get; set; }

        public PolloPolloContext(DbContextOptions<PolloPolloContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}