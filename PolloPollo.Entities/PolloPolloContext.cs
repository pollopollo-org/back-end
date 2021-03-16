using Microsoft.EntityFrameworkCore;

namespace PolloPollo.Entities
{
    public class PolloPolloContext : DbContext, IPolloPolloContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<Producer> Producers { get; set; }
        public DbSet<Receiver> Receivers { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Application> Applications { get; set; }

        public virtual DbSet<Contracts> Contracts { get; set; }
        public virtual DbSet<ByteExchangeRate> ByteExchangeRate { get; set; }


        public PolloPolloContext(DbContextOptions<PolloPolloContext> options) : base(options)
        {
        }

        public PolloPolloContext() {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<UserRole>()
                .Property(e => e.UserRoleEnum)
                .HasConversion<int>();
            modelBuilder
                .Entity<UserRole>()
                .HasKey(e => new { e.UserId, e.UserRoleEnum });
            modelBuilder
                .Entity<User>()
                .HasAlternateKey(c => c.Email)
                .HasName("AlternateKey_UserEmail");

            modelBuilder 
                .Entity<User>()
                .HasMany(u => u.Applications)
                .WithOne(a => a.User)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Application>()
                .HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Application>()
                .Property(e => e.Status)
                .HasConversion<int>();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }
    }
}
