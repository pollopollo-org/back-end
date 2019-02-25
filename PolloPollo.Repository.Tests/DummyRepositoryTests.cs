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
    public class DummyRepositoryTests
    {
        [Fact]
        public async Task CreateAsyncGivenDTOCreatesNewDummy()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new DummyRepository(context);
                var dto = new DummyCreateUpdateDTO
                {
                    Description = "I am a dummy!"
                };

                var created = await repository.CreateAsync(dto);

                var id = 1;
                Assert.Equal(id, created.Id);

                var found = await context.Dummies.FindAsync(id);
                
                Assert.Equal(dto.Description, found.Description);
            }
        }

        [Fact]
        public async Task CreateAsyncGivenDTOReturnsCreatedDummy()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new DummyRepository(context);
                var dto = new DummyCreateUpdateDTO
                {
                    Description = "I am a dummy!"
                };

                var created = await repository.CreateAsync(dto);

                Assert.Equal(1, created.Id);
                Assert.Equal(dto.Description, created.Description);
            }
        }

        [Fact]
        public async Task FindAsyncGivenExistingIdReturnsDto()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new DummyRepository(context);
                
                var dto = new DummyCreateUpdateDTO
                {
                    Description = "I am a dummy!"
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
                var repository = new DummyRepository(context);

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
                var entity1 = new DummyEntity {
                    Description = "I am the dummest dummy in the world."
                };

                var entity2 = new DummyEntity {
                    Description = "I am the second dummest dummy in the world."
                };
                
                context.Dummies.AddRange(entity1, entity2);

                await context.SaveChangesAsync();

                var repository = new DummyRepository(context);

                var result = repository.Read().ToList();

                Assert.Collection(result,
                    d => { Assert.Equal(entity1.Description, d.Description); },
                    d => { Assert.Equal(entity2.Description, d.Description); }
                );
            }
        }

        [Fact]
        public async Task ReadOnEmptyRepositoryReturnEmptyCollection()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new DummyRepository(context);
                var result = repository.Read();
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ReadGivenNoneExistingUserIdReturnEmptyCollection()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new DummyRepository(context);
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
                var entity = new DummyEntity
                {
                    Description = "I am a dummy entity",
                };
                
                context.Dummies.Add(entity);
                await context.SaveChangesAsync();

                var repository = new DummyRepository(context);
                
                var dto = new DummyCreateUpdateDTO
                {
                    Id = entity.Id,
                    Description = "This is my new description."
                };

                var updated = await repository.UpdateAsync(dto);

                Assert.True(updated);

                var updatedEntity = await context.Dummies.FirstOrDefaultAsync(d => d.Id == entity.Id);

                Assert.Equal(dto.Description, updatedEntity.Description);
            }
        }

        [Fact]
        public async Task UpdateAsyncGivenNonExistingDTOReturnsFalse()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new DummyRepository(context);
                
                var dto = new DummyCreateUpdateDTO
                {
                    Description = "I am a dummy dto"
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
                var entity = new DummyEntity { Description = "I am a entity" };
                context.Dummies.Add(entity);
                await context.SaveChangesAsync();
                
                var entityId = entity.Id;

                var repository = new DummyRepository(context);

                var deleted = await repository.DeleteAsync(entityId);

                Assert.True(deleted);
            }
        }

        [Fact]
        public async Task DeleteAsyncGivenNonExistingIdReturnsFalse()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new DummyRepository(context);

                var deleted = await repository.DeleteAsync(0);

                Assert.False(deleted);
            }
        }

        private async Task<DbConnection> CreateConnectionAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
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
