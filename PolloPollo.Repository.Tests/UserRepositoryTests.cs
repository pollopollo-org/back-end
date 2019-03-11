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
        public async Task CreateAsyncGivenDTOCreatesNewUser()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);
                var dto = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                // Remember to add user beforehand..

                var created = repository.Authenticate(dto.Email, dto.Password);

                var id = 1;
                Assert.Equal(id, created.Id);

                var found = await context.Users.FindAsync(id);

                Assert.Equal(dto.FirstName, found.FirstName);
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
                Secret = "12345",
            };
            return Options.Create(config as SecurityConfig);
        }
    }
}
