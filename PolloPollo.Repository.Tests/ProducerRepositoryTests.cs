using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Data.Common;
using System.Threading.Tasks;
using PolloPollo.Shared;
using PolloPollo.Entities;

namespace PolloPollo.Repository.Tests
{
    public class ProducerRepositoryTests
    {
        [Fact]
        public async Task CreateAsyncGivenDTOCreatesNewUser()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);
                var dto = new UserCreateDTO
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                var created = await repository.CreateAsync(dto);

                var id = 1;
                Assert.Equal(id, created.Id);

                var found = await context.Users.FindAsync(id);

                Assert.Equal(dto.FirstName, found.FirstName);
            }
        }

        [Fact]
        public async Task CreateAsyncGivenDTOReturnsCreatedUser()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);
                var dto = new UserCreateDTO
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                var created = await repository.CreateAsync(dto);

                Assert.Equal(1, created.Id);
                Assert.Equal(dto.Surname, created.Surname);
            }
        }

        [Fact]
        public async Task CreateAsyncGivenDTOCreatesProducer()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);
                var dto = new UserCreateDTO
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                var created = await repository.CreateAsync(dto);

                var producer = await context.Producers.FindAsync(created.Id);

                Assert.NotNull(producer);
            }
        }

        [Fact]
        public async Task FindAsyncGivenExistingIdReturnsDto()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);

                var dto = new UserCreateDTO
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                var created = await repository.CreateAsync(dto);

                var found = await repository.FindAsync(created.Id);

                Assert.Equal(created.Id, found.Id);
                Assert.Equal(created.Description, found.Description);
            }
        }

        [Fact]
        public async Task FindAsyncGivenNonExistingIdReturnsNull()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);

                var result = await repository.FindAsync(0);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task ReadReturnsProjectionOfAllDummies()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var user1 = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                var user2 = new User
                {
                    FirstName = "Trine",
                    Surname = "Borre",
                    Email = "trij@itu.dk",
                    Country = "DK",
                    Password = "notsosecretpassword"
                };

                context.Users.AddRange(user1, user2);

                await context.SaveChangesAsync();

                var repository = new ProducerRepository(context);

                var result = repository.Read().ToList();

                Assert.Collection(result,
                    d => { Assert.Equal(user1.Email, d.Email); },
                    d => { Assert.Equal(user2.Email, d.Email); }
                );
            }
        }

        [Fact]
        public async Task ReadOnEmptyRepositoryReturnEmptyCollection()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);
                var result = repository.Read();
                Assert.Empty(result);
            }
        }


        [Fact]
        public async Task UpdateAsyncGivenExistingDTOUpdatesEntity()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var entity = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                context.Users.Add(entity);
                await context.SaveChangesAsync();

                var repository = new ProducerRepository(context);

                var dto = new UserCreateUpdateDTO
                {
                    Id = 1,
                    FirstName = "Trine",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                var updated = await repository.UpdateAsync(dto);

                Assert.True(updated);

                var updatedEntity = await context.Users.FirstOrDefaultAsync(d => d.Id == entity.Id);

                Assert.Equal(dto.FirstName, updatedEntity.FirstName);
            }
        }

        [Fact]
        public async Task UpdateAsyncGivenNonExistingDTOReturnsFalse()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);

                var dto = new UserCreateUpdateDTO
                {
                    Id = 0
                };

                var updated = await repository.UpdateAsync(dto);

                Assert.False(updated);
            }
        }

        [Fact]
        public async Task DeleteAsyncGivenExistingIdReturnsTrue()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var user = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var userId = user.Id;

                var repository = new ProducerRepository(context);

                var deleted = await repository.DeleteAsync(userId);

                Assert.True(deleted);
            }
        }

        [Fact]
        public async Task DeleteAsyncGivenNonExistingIdReturnsFalse()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);

                var deleted = await repository.DeleteAsync(0);

                Assert.False(deleted);
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
    }
}
