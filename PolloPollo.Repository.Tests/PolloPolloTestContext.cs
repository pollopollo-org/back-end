using Microsoft.EntityFrameworkCore;
using PolloPollo.Entities;
using PolloPollo.Services;

namespace PolloPollo.Services.Tests
{
    public class PolloPolloTestContext : PolloPolloContext
    {
        public PolloPolloTestContext(DbContextOptions<PolloPolloContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // V Seeded data goes here V
            modelBuilder.Entity<Donor>(entity =>
            {
                entity.HasData(new[] {
                    new Donor
                    {
                        AaAccount = "seeded-test-donor-1",
                        Password = "12345678",
                        UID = "guid-1",
                        Email = "test@test1.com",
                        DeviceAddress = "12345678",
                        WalletAddress = "12345678",
                    },

                    new Donor
                    {
                        AaAccount = "seeded-test-donor-2",
                        Password = "12345678",
                        UID = "guid-2",
                        Email = "test@test2.com",
                        DeviceAddress = "87654321",
                        WalletAddress = "87654321",
                    },

                    new Donor
                    {
                        AaAccount = "seeded-test-donor-3",
                        Password = "12345678",
                        UID = "guid-3",
                        Email = "test@test3.com",
                        DeviceAddress = "123456789",
                        WalletAddress = "123456789",
                    },

                    new Donor
                    {
                        AaAccount = "seeded-test-donor-4",
                        Password = "12345678",
                        UID = "guid-4",
                        Email = "test@test4.com",
                        DeviceAddress = "987654321",
                        WalletAddress = "987654321",
                    }
                });
            });
        }
    }
}