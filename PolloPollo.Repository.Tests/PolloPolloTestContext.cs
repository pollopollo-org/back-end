using System;
using Microsoft.EntityFrameworkCore;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Repository.Utils;

namespace PolloPollo.Repository.Tests
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
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasData(new[]
                {
                    new User
                    {
                        Id = -1,
                        Email = "receiver@test.com",
                        Password = PasswordHasher.HashPassword("receiver@test.com", "12345678"),
                        FirstName = "test",
                        SurName = "test",
                        Country = "CountryCode",
                        Created = new DateTime(1, 1, 1, 1, 1, 1)
                    },
                    new User
                    {
                        Id = -2,
                        Email = "producer@test.com",
                        Password = PasswordHasher.HashPassword("producer@test.com", "12345678"),
                        FirstName = "test",
                        SurName = "test",
                        Country = "CountryCode",
                        Created = new DateTime(1, 1, 1, 1, 1, 1)
                    },
                    new User
                    {
                        Id = -3,
                        Email = "producer1@test.com",
                        Password = PasswordHasher.HashPassword("producer1@test.com", "12345678"),
                        FirstName = "test",
                        SurName = "test",
                        Country = "CountryCode",
                        Created = new DateTime(1, 1, 1, 1, 1, 1)
                    },
                });
            });
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasData(new[]
                {
                    new UserRole
                    {
                        UserId = -1,
                        UserRoleEnum = Shared.UserRoleEnum.Receiver
                    },
                    new UserRole
                    {
                        UserId = -2,
                        UserRoleEnum = Shared.UserRoleEnum.Producer
                    },
                    new UserRole
                    {
                        UserId = -3,
                        UserRoleEnum = Shared.UserRoleEnum.Producer
                    },
                });
            });
            modelBuilder.Entity<Receiver>(entity =>
            {
                entity.HasData(new[]
                {
                    new Receiver
                    {
                        Id = -1,
                        UserId = -1
                    }
                });
            });
            modelBuilder.Entity<Producer>(entity =>
            {
                entity.HasData(new[]
                {
                    new Producer
                    {
                        Id = -2,
                        UserId = -2,
                        WalletAddress = "test",
                        PairingSecret = "secret",
                        Street = "teststreet",
                        StreetNumber = "testnumber",
                        City = "testcity",
                        Zipcode = "1234"
                    },
                    new Producer
                    {
                        Id = -3,
                        UserId = -3,
                        WalletAddress = "test",
                        PairingSecret = "",
                        Street = "teststreet",
                        StreetNumber = "testnumber",
                        City = "testcity",
                        Zipcode = "1234"
                    },
                });
            });
        }
    }
}
