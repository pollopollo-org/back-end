using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Repository.Tests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task AuthenticateGivenValidUserReturnsUserWithToken()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();

                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var repository = new UserRepository(config, context, producerRepo, receiverRepo);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = repository.HashPassword("stei@itu.dk", plainPassword)
                };

                context.Users.Add(user);
                context.SaveChanges();

                var token = repository.Authenticate(user.Email, plainPassword);

                Assert.NotNull(token);
            }
        }

        [Fact]
        public async Task AuthenticateGivenNotExistingUserReturnsNull()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context, producerRepo, receiverRepo);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = repository.HashPassword("stei@itu.dk", plainPassword)
                };

                var token = repository.Authenticate(user.Email, user.Password);
                Assert.Null(token);
            }
        }

        [Fact]
        public async Task AuthenticateGivenWrongPasswordAndEmailReturnsNull()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context, producerRepo, receiverRepo);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = repository.HashPassword("stei@itu.dk", plainPassword)
                };

                context.Users.Add(user);
                context.SaveChanges();

                var token = repository.Authenticate("wrongemail@itu.dk", "wrongpassword");
                Assert.Null(token);
            }
        }

        [Fact]
        public async Task CreateAsyncWhenRoleReceiverCreatesReceiverAndReturnsId()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context, producerRepo, receiverRepo);

                var dto = new UserCreateDTO
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Role = "Receiver",
                    Password = "secret"
                };

                var token = await repository.CreateAsync(dto);

                var receiver = await repository.FindAsync(token.UserId);

                Assert.Equal(token.UserId, receiver.UserId);
            }
        }

        [Fact]
        public async Task CreateAsyncWhenRoleProducerCreatesProducerAndReturnsId()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context, producerRepo, receiverRepo);

                var dto = new UserCreateDTO
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Role = "Producer",
                    Password = "secret"
                };

                var token = await repository.CreateAsync(dto);

                var producer = await repository.FindAsync(token.UserId);

                Assert.Equal(token.UserId, producer.UserId);
            }
        }



        private async Task<DbConnection> CreateConnectionAsync()
        {
            var connection = new SqliteConnection("datasource=:memory:");
            await connection.OpenAsync();

            return connection;
        }

        private async Task<PolloPolloContext> CreateContextAsync(DbConnection connection)
        {
            var builder = new DbContextOptionsBuilder<PolloPolloContext>().UseSqlite(connection);

            var context = new PolloPolloContext(builder.Options);
            await context.Database.EnsureCreatedAsync();

            return context;
        }

        private IOptions<SecurityConfig> GetSecurityConfig()
        {
            SecurityConfig config = new SecurityConfig
            {
                Secret = "0d797046248eeb96eb32a0e5fdc674f5ad862cad",
            };
            return Options.Create(config as SecurityConfig);
        }
    }
}
