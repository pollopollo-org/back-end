using Microsoft.EntityFrameworkCore;

namespace PolloPollo.Entities
{
    public class PolloPolloContext : DbContext, IPolloPolloContext
    {
        public DbSet<DummyEntity> Dummies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Producer> Producers { get; set; }
        public DbSet<Receiver> Receiver { get; set; }

        public PolloPolloContext(DbContextOptions<PolloPolloContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
