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

                var dummy = await repository.CreateAsync(dto);

                Assert.Equal(1, dummy.Id);

                var entity = await context.Dummies.FindAsync(1);
                
                Assert.Equal(dto.Description, entity.Description);
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

                var dummy = await repository.CreateAsync(dto);

                Assert.Equal(1, dummy.Id);
                Assert.Equal(dto.Description, dummy.Description);
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

                var createdDummy = await repository.CreateAsync(dto);

                var foundDummy = await repository.FindAsync(createdDummy.Id);

                Assert.Equal(createdDummy.Id, foundDummy.Id);
                Assert.Equal(createdDummy.Description, foundDummy.Description);
            }
        }

        [Fact]
        public async Task FindAsyncGivenNonExistingIdReturnsNull()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new DummyRepository(context);

                var dummy = await repository.FindAsync(0);

                Assert.Null(dummy);
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
