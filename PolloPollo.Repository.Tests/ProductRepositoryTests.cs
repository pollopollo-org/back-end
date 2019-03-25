using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Repository.Tests
{
    public class ProductRepositoryTests
    {
        [Fact]
        public async Task CreateAsync_given_null_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProductRepository(context);

                var result = await repository.CreateAsync(null);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_given_empty_DTO_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProductRepository(context);

                var productDTO = new ProductCreateDTO
                {
                    //Nothing
                };

                var result = await repository.CreateAsync(null);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_returns_DTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProductRepository(context);

                var productDTO = new ProductCreateDTO
                {
                    Title = "5 chickens",
                    ProducerId = 123,
                    Price = 42,
                    Description = "test",
                    Location = "tst",
                    Available = true,
                };

                var result = await repository.CreateAsync(productDTO);

                Assert.Equal(productDTO.Title, result.Title);
                Assert.Equal(productDTO.ProducerId, result.ProducerId);
                Assert.Equal(productDTO.Price, result.Price);
                Assert.Equal(productDTO.Description, result.Description);
                Assert.Equal(productDTO.Location, result.Location);
                Assert.Equal(productDTO.Available, result.Available);

            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_returns_DTO_with_id_1()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProductRepository(context);

                var productDTO = new ProductCreateDTO
                {
                    Title = "5 chickens",
                    ProducerId = 123,
                    Price = 42,
                    Description = "test",
                    Location = "tst",
                    Available = true,
                };

                var result = await repository.CreateAsync(productDTO);

                var expectedId = 1;

                Assert.Equal(expectedId, result.ProductId);
            }
        }



        //Below are internal methods for use during testing

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
