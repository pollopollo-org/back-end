using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Services.Utils;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Services.Tests
{
    public class ProductRepositoryTests
    {
        [Fact]
        public async Task CreateAsync_given_null_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var result = await repository.CreateAsync(default(ProductCreateDTO));

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_given_empty_DTO_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var productDTO = new ProductCreateDTO
                {
                    //Nothing
                };

                var result = await repository.CreateAsync(productDTO);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_given_invalid_DTO_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var productDTO = new ProductCreateDTO
                {
                    Price = 10,
                };

                var result = await repository.CreateAsync(productDTO);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_returns_DTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var productDTO = new ProductCreateDTO
                {
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "Test",
                    Location = "Test",
                    Country = "Test",
                    Rank = 2,
                };

                var result = await repository.CreateAsync(productDTO);

                Assert.Equal(productDTO.Title, result.Title);
                Assert.Equal(productDTO.UserId, result.UserId);
                Assert.Equal(productDTO.Price, result.Price);
                Assert.Equal(productDTO.Description, result.Description);
                Assert.Equal(productDTO.Location, result.Location);
                Assert.Equal(productDTO.Country, result.Country);
                Assert.Equal(productDTO.Rank, result.Rank);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_sets_Timestamp_in_database()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var productDTO = new ProductCreateDTO
                {
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "Test",
                    Location = "Test",
                    Country = "Test",
                    Rank = 2,
                };

                var result = await repository.CreateAsync(productDTO);

                var now = DateTime.UtcNow;
                var dbTimestamp = context.Products.Find(result.ProductId).TimeStamp;

                // These checks are to assume the timestamp is set on creation.
                // The now timestamp is some ticks off from the database timestamp.
                Assert.Equal(dbTimestamp.Date, now.Date);
                Assert.Equal(dbTimestamp.Hour, now.Hour);
                Assert.Equal(dbTimestamp.Minute, now.Minute);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_returns_DTO_with_Id()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var productDTO = new ProductCreateDTO
                {
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "test",
                    Location = "tst",
                };

                var result = await repository.CreateAsync(productDTO);

                var expectedId = 1;

                Assert.Equal(expectedId, result.ProductId);
            }
        }

        [Fact]
        public async Task FindAsync_given_existing_Id_returns_ProductDTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var entity = new Product
                {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = ""
                };

                context.Products.Add(entity);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var product = await repository.FindAsync(entity.Id);

                Assert.Equal(entity.Id, product.ProductId);
                Assert.Equal(entity.Title, product.Title);
                Assert.Empty(entity.Thumbnail);
            }
        }

        [Fact]
        public async Task FindAsync_given_existing_Id_with_thumbnail_returns_ProductDTO_with_thumbnail()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = "static";
                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var entity = new Product
                {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png"
                };

                context.Products.Add(entity);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var product = await repository.FindAsync(entity.Id);

                Assert.Equal(entity.Id, product.ProductId);
                Assert.Equal(entity.Title, product.Title);
                Assert.Equal($"{folder}/{entity.Thumbnail}", product.Thumbnail);
            }
        }

        [Fact]
        public async Task FindAsync_given_nonExisting_Id_returns_null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var result = await repository.FindAsync(42);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task Read_returns_all_available_products()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = "static";
                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var product1 = new Product {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Available = true
                };
                var product2 = new Product {
                    Title = "Eggs",
                    UserId = id,
                    Available = false
                };

                context.Products.AddRange(product1, product2);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var products = repository.Read();

                // There should only be one product in the returned list
                // since one of the created products is not available
                var count = products.ToList().Count;
                Assert.Equal(1, count);

                var product = products.First();

                Assert.Equal(1, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
                Assert.Equal($"{folder}/{product1.Thumbnail}", product.Thumbnail);
            }
        }

        [Fact]
        public async Task Read_given_existing_id_returns_all_products_by_specified_user_id()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = "static";
                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                var otherId = 2; //

                var otherUser = new User
                {
                    Id = otherId,
                    Email = "other@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var otherUserEnumRole = new UserRole
                {
                    UserId = otherId,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var otherReceiver = new Receiver
                {
                    UserId = otherId,
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                context.Users.Add(otherUser);
                context.UserRoles.Add(otherUserEnumRole);
                context.Receivers.Add(otherReceiver);
                await context.SaveChangesAsync();

                var product1 = new Product {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Available = true
                };
                var product2 = new Product {
                    Title = "Eggs",
                    UserId = id,
                    Thumbnail = "",
                    Available = false
                };
                var product3 = new Product {
                    Title = "Chickens",
                    UserId = otherId,
                    Available = true
                };
                context.Products.AddRange(product1, product2, product3);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var products = repository.Read(id);

                // There should only be two products in the returned list
                // since one of the created products is by another producer
                var count = products.ToList().Count;
                Assert.Equal(2, count);

                var product = products.First();
                var secondProduct = products.Last();

                Assert.Equal(id, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
                Assert.Equal($"{folder}/{product1.Thumbnail}", product.Thumbnail);
                Assert.Null(secondProduct.Thumbnail);
            }
        }

        [Fact]
        public async Task Read_given_nonExisting_id_returns_emptyCollection()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var result = repository.Read(42);
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_existing_id_returns_True()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = false,
                    User = user,
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = false
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var update = await repository.UpdateAsync(expectedProduct);

                Assert.True(update);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_existing_id_updates_product()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = false,
                    UserId = id,
                };

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = true,
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                await repository.UpdateAsync(expectedProduct);

                var products = await context.Products.FindAsync(product.Id);

                Assert.Equal(expectedProduct.Id, products.Id);
                Assert.Equal(expectedProduct.Available, products.Available);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_non_existing_id_returns_False()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var updateProductDTO = new ProductUpdateDTO
                {
                    Id = 42,
                    Available = false,
                };

                var result = await repository.UpdateAsync(updateProductDTO);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task UpdateImageAsync_given_folder_existing_id_and_image_updates_user_thumbnail()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = "static";
                var id = 1;
                var fileName = "file.png";
                var formFile = new Mock<IFormFile>();

                var imageWriter = new Mock<IImageWriter>();
                imageWriter.Setup(i => i.UploadImageAsync(folder, formFile.Object)).ReturnsAsync(fileName);

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    FirstName = "Test",
                    SurName = "Test",
                    Password = PasswordHasher.HashPassword("test@Test", "12345678"),
                    Country = "CountryCode",
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var producer = new Producer
                {
                    UserId = id
                };

                var product = new Product
                {
                    Id = id,
                    Title = "Test",
                    Country = "Test",
                    Available = true,
                    Price = 0,
                    Description = "Test",
                    UserId = user.Id,
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var repository = new ProductRepository(imageWriter.Object, context);

                var update = await repository.UpdateImageAsync(folder, id, formFile.Object);

                var updatedProduct = await context.Products.FindAsync(id);

                Assert.Equal(fileName, updatedProduct.Thumbnail);
                Assert.Equal(fileName, update);
            }
        }

        [Fact]
        public async Task UpdateImageAsync_given_folder_existing_id_and_image_and_existing_image_Creates_new_image_and_Removes_old_thumbnail()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = "static";
                var id = 1;
                var oldFile = "oldFile.jpg";
                var fileName = "file.png";
                var formFile = new Mock<IFormFile>();

                var imageWriter = new Mock<IImageWriter>();
                imageWriter.Setup(i => i.UploadImageAsync(folder, formFile.Object)).ReturnsAsync(fileName);
                imageWriter.Setup(i => i.DeleteImage(folder, oldFile)).Returns(true);

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    FirstName = "Test",
                    SurName = "Test",
                    Password = PasswordHasher.HashPassword("test@Test", "12345678"),
                    Country = "CountryCode",
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var producer = new Producer
                {
                    UserId = id
                };

                var product = new Product
                {
                    Id = id,
                    Title = "Test",
                    Country = "Test",
                    Available = true,
                    Price = 0,
                    Description = "Test",
                    UserId = user.Id,
                    Thumbnail = oldFile,
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var repository = new ProductRepository(imageWriter.Object, context);

                var update = await repository.UpdateImageAsync(folder, id, formFile.Object);

                imageWriter.Verify(i => i.UploadImageAsync(folder, formFile.Object));
                imageWriter.Verify(i => i.DeleteImage(folder, oldFile));
            }
        }

        [Fact]
        public async Task UpdateImageAsync_given_folder_existing_id_invalid_file_returns_Exception_with_error_message()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = "static";
                var id = 1;
                var oldFile = "oldFile.jpg";
                var error = "Invalid image file";
                var formFile = new Mock<IFormFile>();

                var imageWriter = new Mock<IImageWriter>();
                imageWriter.Setup(i => i.UploadImageAsync(folder, formFile.Object)).ThrowsAsync(new ArgumentException(error));

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    FirstName = "Test",
                    SurName = "Test",
                    Thumbnail = oldFile,
                    Password = PasswordHasher.HashPassword("test@Test", "12345678"),
                    Country = "CountryCode",
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var producer = new Producer
                {
                    UserId = id
                };

                var product = new Product
                {
                    Id = id,
                    Title = "Test",
                    Country = "Test",
                    Available = true,
                    Price = 0,
                    Description = "Test",
                    UserId = user.Id,
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var repository = new ProductRepository(imageWriter.Object, context);

                var ex = await Assert.ThrowsAsync<Exception>(() => repository.UpdateImageAsync(folder, id, formFile.Object));

                Assert.Equal(error, ex.Message);
            }
        }

        [Fact]
        public async Task UpdateImageAsync_given_non_existing_id_returns_null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var formFile = new Mock<IFormFile>();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new ProductRepository(imageWriter.Object, context);

                var update = await repository.UpdateImageAsync("folder", 42, formFile.Object);

                Assert.Null(update);
            }
        }

        [Fact]
        public async Task GetCount_returns_0_when_no_products()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = "static";
                var error = "Invalid image file";
                var formFile = new Mock<IFormFile>();

                var imageWriter = new Mock<IImageWriter>();
                imageWriter.Setup(i => i.UploadImageAsync(folder, formFile.Object)).ThrowsAsync(new ArgumentException(error));

                var repository = new ProductRepository(imageWriter.Object, context);

                int count = await repository.GetCountAsync();

                Assert.Equal(0, count);
            }

        }

        [Fact]
        public async Task GetCount_returns_number_of_products() 
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = "static";
                var id = 1;
                var oldFile = "oldFile.jpg";
                var error = "Invalid image file";
                var formFile = new Mock<IFormFile>();

                var imageWriter = new Mock<IImageWriter>();
                imageWriter.Setup(i => i.UploadImageAsync(folder, formFile.Object)).ThrowsAsync(new ArgumentException(error));

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    FirstName = "Test",
                    SurName = "Test",
                    Thumbnail = oldFile,
                    Password = PasswordHasher.HashPassword("test@Test", "12345678"),
                    Country = "CountryCode",
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var producer = new Producer
                {
                    UserId = id
                };

                var product = new Product
                {
                    Id = id,
                    Title = "Test",
                    Country = "Test",
                    Available = true,
                    Price = 0,
                    Description = "Test",
                    UserId = user.Id,
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var repository = new ProductRepository(imageWriter.Object, context);

                var count = await repository.GetCountAsync();

                Assert.Equal(1, count);
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
