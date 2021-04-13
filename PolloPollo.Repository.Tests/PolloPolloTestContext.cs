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
                        Password = "ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f",
                        UID = "guid-1",
                        Email = "test@test1.com",
                        DeviceAddress = "12345678",
                        WalletAddress = "12345678",
                    },

                    new Donor
                    {
                        AaAccount = "seeded-test-donor-2",
                        Password = "ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f",
                        UID = "guid-2",
                        Email = "test@test2.com",
                        DeviceAddress = "87654321",
                        WalletAddress = "87654321",
                    },

                    new Donor
                    {
                        AaAccount = "seeded-test-donor-3",
                        Password = "ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f",
                        UID = "guid-3",
                        Email = "test@test3.com",
                        DeviceAddress = "123456789",
                        WalletAddress = "123456789",
                    },

                    new Donor
                    {
                        AaAccount = "seeded-test-donor-4",
                        Password = "ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f",
                        UID = "guid-4",
                        Email = "test@test4.com",
                        DeviceAddress = "987654321",
                        WalletAddress = "987654321",
                    },

                    //Password: asdasdasd
                    new Donor
                    {
                        AaAccount = "seeded-test-donor-5",
                        Password = "AQAAAAEAACcQAAAAEI54+xzcaNz3WEUDaa/qpimtJjg9jvJCikowsNTO3tnQ/SC+",
                        UID = "guid-5",
                        Email = "lol@lol.com",
                        DeviceAddress = "987654321",
                        WalletAddress = "987654321",
                    }
                });
            });
            modelBuilder.Entity<ByteExchangeRate>(entity =>
            {
                entity.HasData(new[]
                {
                    new ByteExchangeRate
                    {
                        Id = 1,
                        GBYTE_USD = 7
                    },
                });
            });
        }
    }
}
