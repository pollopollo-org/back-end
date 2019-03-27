using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PolloPollo.Entities;
using PolloPollo.Shared;
using System.Data.Common;
using System.Linq;
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

        [Fact]
        public async Task FindAsync_given_null_returns_null()
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
        public async Task FindAsync_given_existing_Id_returns_ProductDTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProductRepository(context);

                //TODO testing
            }
        }

        [Fact]
        public async Task Read_returns_projection_of_all_products()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var product1 = new Product { Title = "Chickens", Available = true };
                var product2 = new Product { Title = "Eggs", Available = false};
                context.Products.AddRange(product1, product2);
                await context.SaveChangesAsync();

                var repository = new ProductRepository(context);

                var products = repository.Read();

                // There should only be one product in the returned list 
                // since one of the created products is not available
                var count = products.ToList().Count;
                Assert.Equal(1, count);

                var product = products.First();

                Assert.Equal(1, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
            }
        }

        [Fact]
        public async Task Read_given_existing_id_returns_projection_of_all_products_by_specified_id()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var product1 = new Product { Title = "Chickens", ProducerId = 1, Available = true };
                var product2 = new Product { Title = "Eggs", ProducerId = 1, Available = false };
                var product3 = new Product { Title = "Chickens", ProducerId = 2, Available = true };
                context.Products.AddRange(product1, product2, product3);
                await context.SaveChangesAsync();

                var repository = new ProductRepository(context);

                var products = repository.Read(1);

                // There should only be two products in the returned list 
                // since one of the created products is by another producer
                var count = products.ToList().Count;
                Assert.Equal(2, count);

                var product = products.First();

                Assert.Equal(1, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
            }
        }

        [Fact]
        public async Task Read_given_nonExisting_id_returns_emptyCollection()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ProductRepository(context);
                var result = repository.Read(1);
                Assert.Empty(result);
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
