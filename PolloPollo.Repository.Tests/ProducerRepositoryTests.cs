using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Data.Common;
using System.Threading.Tasks;
using PolloPollo.Shared;
using PolloPollo.Entities;
using Microsoft.Extensions.Options;

namespace PolloPollo.Repository.Tests
{
    public class ProducerRepositoryTests
    {

        private async Task<User> TestUser(IPolloPolloContext context)
        {
            var user = new User
            {
                FirstName = "Christina",
                Surname = "Steinhauer",
                Email = "stei@itu.dk",
                Country = "DK",
                Password = "verysecret123",
                Role = "Producer"
            };

            context.Users.Add(user);

            await context.SaveChangesAsync();

            return user;
        }



        [Fact]
        public async Task CreateAsyncGivenDTOCreatesNewProducer()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {

                var repository = new ProducerRepository(context);

                var user = await TestUser(context);

                var created = await repository.CreateAsync(user.Id);

                var id = 1;
                Assert.Equal(id, user.Id);

                var found = await context.Users.FindAsync(id);

                Assert.Equal(user.FirstName, found.FirstName);
            }
        }

        [Fact]
        public async Task CreateAsyncGivenDTOReturnsCreatedProducer()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);

                var user = await TestUser(context);

                var created = await repository.CreateAsync(user.Id);

                Assert.Equal(1, created.UserId);
                Assert.Equal(user.Surname, created.Surname);
            }
        }

        [Fact]
        public async Task CreateAsyncGivenDTOCreatesProducer()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProducerRepository(context);

                var user = await TestUser(context);

                var created = await repository.CreateAsync(user.Id);

                var producer = await context.Producers.FindAsync(created.UserId);

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

                var user = await TestUser(context);

                var created = await repository.CreateAsync(user.Id);

                var found = await repository.FindAsync(created.UserId);

                Assert.Equal(created.UserId, found.UserId);
                Assert.Equal(created.Surname, found.Surname);
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
        public async Task ReadReturnsProjectionOfAllProducers()
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
                    Password = "verysecret123",
                    Role = "Producer"
                };

                var user2 = new User
                {
                    FirstName = "Trine",
                    Surname = "Borre",
                    Email = "trij@itu.dk",
                    Country = "DK",
                    Password = "notsosecretpassword",
                    Role = "Producer"
                };

                context.Users.AddRange(user1, user2);

                await context.SaveChangesAsync();

                var repository = new ProducerRepository(context);

                await repository.CreateAsync(user1.Id);
                await repository.CreateAsync(user2.Id);

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
                var user = TestUser(context);

                var repository = new ProducerRepository(context);

                var dto = new UserCreateUpdateDTO
                {
                    Id = 1,
                    FirstName = "Trine",
                    Surname = "Borre",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = "verysecret123"
                };

                var updated = await repository.UpdateAsync(dto);

                Assert.True(updated);

                var updatedEntity = await context.Users.FirstOrDefaultAsync(d => d.Id == user.Id);

                Assert.Equal(dto.FirstName, updatedEntity.FirstName);
                Assert.Equal(dto.Surname, updatedEntity.Surname);
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
                var user = await TestUser(context);

                var repository = new ProducerRepository(context);

                var deleted = await repository.DeleteAsync(user.Id);

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
