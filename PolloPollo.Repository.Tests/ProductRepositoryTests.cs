using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Services.Utils;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using System;
using System.Collections.Generic;
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
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var (result, message) = await repository.CreateAsync(default(ProductCreateDTO));

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
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var productDTO = new ProductCreateDTO
                {
                    //Nothing
                };

                var (result, message) = await repository.CreateAsync(productDTO);

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
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var productDTO = new ProductCreateDTO
                {
                    Price = 10,
                };

                var (result, message) = await repository.CreateAsync(productDTO);

                Assert.Null(result);
                Assert.Equal("Producer not found", message);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_returns_DTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    WalletAddress = "address",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
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

                var (result, message) = await repository.CreateAsync(productDTO);

                Assert.Equal(productDTO.Title, result.Title);
                Assert.Equal(productDTO.UserId, result.UserId);
                Assert.Equal(productDTO.Price, result.Price);
                Assert.Equal(productDTO.Description, result.Description);
                Assert.Equal(productDTO.Location, result.Location);
                Assert.Equal(productDTO.Country, result.Country);
                Assert.Equal(productDTO.Rank, result.Rank);
                Assert.Equal("Created", message);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_sets_Timestamp_in_database()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    WalletAddress = "address",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
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

                var (result, message) = await repository.CreateAsync(productDTO);

                var now = DateTime.UtcNow;
                var dbTimestamp = context.Products.Find(result.ProductId).Created;

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
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    WalletAddress = "address",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                await context.SaveChangesAsync();

                var productDTO = new ProductCreateDTO
                {
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "test",
                    Location = "tst",
                };

                var (result, message) = await repository.CreateAsync(productDTO);

                var expectedId = 1;

                Assert.Equal(expectedId, result.ProductId);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_no_wallet_address_returns_no_wallet_address()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                await context.SaveChangesAsync();

                var productDTO = new ProductCreateDTO
                {
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "test",
                    Location = "tst",
                };

                var (result, message) = await repository.CreateAsync(productDTO);

                Assert.Null(result);
                Assert.Equal("No wallet address", message);
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var entity = new Product
                {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = ""
                };

                context.Products.Add(entity);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var entity = new Product
                {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png"
                };

                context.Products.Add(entity);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var product = await repository.FindAsync(entity.Id);

                Assert.Equal(entity.Id, product.ProductId);
                Assert.Equal(entity.Title, product.Title);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(entity.Thumbnail), product.Thumbnail);
            }
        }

        [Fact]
        public async Task FindAsync_given_nonExisting_Id_returns_null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var result = await repository.FindAsync(42);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task ReadOpen_returns_all_available_products()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

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
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = repository.ReadOpen();

                // There should only be one product in the returned list
                // since one of the created products is not available
                var count = products.ToList().Count;
                Assert.Equal(1, count);

                var product = products.First();

                Assert.Equal(1, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(product1.Thumbnail), product.Thumbnail);
            }
        }

        [Fact]
        public async Task ReadOpen_returns_all_available_products_descending_order_by_rank_descending()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product1 = new Product
                {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Rank = 0,
                    Created = new DateTime(2000, 1, 1, 1, 1, 1),
                    Available = true
                };
                var product2 = new Product
                {
                    Title = "Eggs",
                    UserId = id,
                    Rank = 1,
                    Created = new DateTime(2000, 1, 1, 10, 1, 1),
                    Available = true
                };

                context.Products.AddRange(product1, product2);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = await repository.ReadOpen().ToListAsync();

                var product = products.ElementAt(0);
                var secondProduct = products.ElementAt(1);

                Assert.Equal(1, product.Rank);
                Assert.Equal(product2.Id, product.ProductId);
                Assert.Equal(product2.Title, product.Title);
                Assert.Equal(product1.Id, secondProduct.ProductId);
                Assert.Equal(product1.Title, secondProduct.Title);
                Assert.Equal(0, secondProduct.Rank);
            }
        }

        [Fact]
        public async Task ReadOpen_returns_all_available_products_descending_order_by_rank_then_by_timestamp_descending()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product1 = new Product
                {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Rank = 0,
                    Created = new DateTime(2000, 1, 1, 1, 1, 1),
                    Available = true
                };
                var product2 = new Product
                {
                    Title = "Eggs",
                    UserId = id,
                    Rank = 1,
                    Created = new DateTime(2000, 1, 1, 10, 1, 1),
                    Available = true
                };
                var product3 = new Product
                {
                    Title = "Something",
                    UserId = id,
                    Rank = 1,
                    Created = new DateTime(2000, 1, 1, 10, 10, 1),
                    Available = true
                };

                context.Products.AddRange(product1, product2, product3);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = await repository.ReadOpen().ToListAsync();

                var product = products.ElementAt(0);
                var secondProduct = products.ElementAt(1);
                var thirdProduct = products.ElementAt(2);

                Assert.Equal(1, product.Rank);
                Assert.Equal(product3.Id, product.ProductId);
                Assert.Equal(product3.Title, product.Title);
                Assert.Equal(1, secondProduct.Rank);
                Assert.Equal(product2.Id, secondProduct.ProductId);
                Assert.Equal(product2.Title, secondProduct.Title);
                Assert.Equal(0, thirdProduct.Rank);
                Assert.Equal(product1.Id, thirdProduct.ProductId);
                Assert.Equal(product1.Title, thirdProduct.Title);
            }
        }

        [Fact]
        public async Task ReadOpen_with_application_open_returns_all_available_products_and_open_application_count_1()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product1 = new Product
                {
                    Id = 1,
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Available = true
                };
                var product2 = new Product
                {
                    Title = "Eggs",
                    UserId = id,
                    Available = false
                };

                var application1 = new Application
                {
                    Id = 1,
                    UserId = id,
                    ProductId = product1.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Open
                };

                var expectedDTO = new ApplicationDTO
                {
                    ApplicationId = application1.Id,
                    ProductId = application1.ProductId,
                    ReceiverId = application1.UserId,
                    Motivation = application1.Motivation,
                    Status = application1.Status
                };

                context.Products.AddRange(product1, product2);
                context.Applications.Add(application1);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = repository.ReadOpen();

                // There should only be one product in the returned list
                // since one of the created products is not available
                var count = products.ToList().Count;
                Assert.Equal(1, count);

                var product = products.First();

                Assert.Equal(1, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(product1.Thumbnail), product.Thumbnail);
                Assert.Equal(expectedDTO.ApplicationId, product.OpenApplications.ElementAt(0).ApplicationId);
                Assert.Equal(expectedDTO.Status, product.OpenApplications.ElementAt(0).Status);
                Assert.Equal(expectedDTO.Motivation, product.OpenApplications.ElementAt(0).Motivation);
                Assert.Equal(expectedDTO.ProductId, product.OpenApplications.ElementAt(0).ProductId);
                Assert.Equal(product.ProductId, product.OpenApplications.ElementAt(0).ProductId);
                Assert.Equal(expectedDTO.ReceiverId, product.OpenApplications.ElementAt(0).ReceiverId);
            }
        }

        [Fact]
        public async Task ReadOpen_with_application_pending_returns_all_available_products_and_pending_application_count_1()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product1 = new Product
                {
                    Id = 1,
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Available = true
                };
                var product2 = new Product
                {
                    Title = "Eggs",
                    UserId = id,
                    Available = false
                };

                var application1 = new Application
                {
                    Id = 1,
                    UserId = id,
                    ProductId = product1.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Pending
                };

                var expectedDTO = new ApplicationDTO
                {
                    ApplicationId = application1.Id,
                    ProductId = application1.ProductId,
                    ReceiverId = application1.UserId,
                    Motivation = application1.Motivation,
                    Status = application1.Status
                };

                context.Products.AddRange(product1, product2);
                context.Applications.Add(application1);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = repository.ReadOpen();

                // There should only be one product in the returned list
                // since one of the created products is not available
                var count = products.ToList().Count;
                Assert.Equal(1, count);

                var product = products.First();

                Assert.Equal(1, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(product1.Thumbnail), product.Thumbnail);
                Assert.Equal(expectedDTO.ApplicationId, product.PendingApplications.ElementAt(0).ApplicationId);
                Assert.Equal(expectedDTO.Status, product.PendingApplications.ElementAt(0).Status);
                Assert.Equal(expectedDTO.Motivation, product.PendingApplications.ElementAt(0).Motivation);
                Assert.Equal(expectedDTO.ProductId, product.PendingApplications.ElementAt(0).ProductId);
                Assert.Equal(product.ProductId, product.PendingApplications.ElementAt(0).ProductId);
                Assert.Equal(expectedDTO.ReceiverId, product.PendingApplications.ElementAt(0).ReceiverId);
            }
        }

        [Fact]
        public async Task ReadOpen_with_application_closed_returns_all_available_products_and_closed_application_count_1()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product1 = new Product
                {
                    Id = 1,
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Available = true
                };
                var product2 = new Product
                {
                    Title = "Eggs",
                    UserId = id,
                    Available = false
                };

                var application1 = new Application
                {
                    Id = 1,
                    UserId = id,
                    ProductId = product1.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Unavailable
                };

                var expectedDTO = new ApplicationDTO
                {
                    ApplicationId = application1.Id,
                    ProductId = application1.ProductId,
                    ReceiverId = application1.UserId,
                    Motivation = application1.Motivation,
                    Status = application1.Status
                };

                context.Products.AddRange(product1, product2);
                context.Applications.Add(application1);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = repository.ReadOpen();

                // There should only be one product in the returned list
                // since one of the created products is not available
                var count = products.ToList().Count;
                Assert.Equal(1, count);

                var product = products.First();

                Assert.Equal(1, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(product1.Thumbnail), product.Thumbnail);
                Assert.Equal(expectedDTO.ApplicationId, product.ClosedApplications.ElementAt(0).ApplicationId);
                Assert.Equal(expectedDTO.Status, product.ClosedApplications.ElementAt(0).Status);
                Assert.Equal(expectedDTO.Motivation, product.ClosedApplications.ElementAt(0).Motivation);
                Assert.Equal(expectedDTO.ProductId, product.ClosedApplications.ElementAt(0).ProductId);
                Assert.Equal(product.ProductId, product.ClosedApplications.ElementAt(0).ProductId);
                Assert.Equal(expectedDTO.ReceiverId, product.ClosedApplications.ElementAt(0).ReceiverId);
            }
        }

        [Fact]
        public async Task ReadOpen_given_existing_id_returns_all_products_by_specified_user_id()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
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

                var otherProducer = new Producer
                {
                    UserId = otherId,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                context.Users.Add(otherUser);
                context.UserRoles.Add(otherUserEnumRole);
                context.Producers.Add(producer);

                var product1 = new Product {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Rank = 0,
                    Available = true
                };
                var product2 = new Product {
                    Title = "Eggs",
                    UserId = id,
                    Thumbnail = "",
                    Rank = 0,
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
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = await repository.Read(id).ToListAsync();

                // There should only be two products in the returned list
                // since one of the created products is by another producer
                var count = products.ToList().Count;
                Assert.Equal(2, count);

                var product = products.ElementAt(0);
                var secondProduct = products.ElementAt(1);

                Assert.Equal(id, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(product1.Available, product.Available);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(product1.Thumbnail), product.Thumbnail);
                Assert.Null(secondProduct.Thumbnail);
            }
        }

        [Fact]
        public async Task Read_given_existing_id_returns_all_products_in_descending_order_by_rank_descending()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
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

                var otherProducer = new Producer
                {
                    UserId = otherId,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                context.Users.Add(otherUser);
                context.UserRoles.Add(otherUserEnumRole);
                context.Producers.Add(otherProducer);

                var product1 = new Product
                {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Rank = 1,
                    Available = true
                };
                var product2 = new Product
                {
                    Title = "Eggs",
                    UserId = id,
                    Thumbnail = "",
                    Rank = 0,
                    Available = true
                };
                var product3 = new Product
                {
                    Title = "Chickens",
                    UserId = otherId,
                    Available = true
                };
                context.Products.AddRange(product1, product2, product3);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = repository.Read(id);

                var product = products.First();
                var secondProduct = products.Last();

                Assert.Equal(1, product.Rank);
                Assert.Equal(product1.Id, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(0, secondProduct.Rank);
                Assert.Equal(product2.Id, secondProduct.ProductId);
                Assert.Equal(product2.Title, secondProduct.Title);
            }
        }

        [Fact]
        public async Task Read_given_existing_id_returns_all_products_in_descending_order_by_rank_then_by_timestamp_descending()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
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

                var otherProducer = new Producer
                {
                    UserId = otherId,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                context.Users.Add(otherUser);
                context.UserRoles.Add(otherUserEnumRole);
                context.Producers.Add(otherProducer);

                var product1 = new Product
                {
                    Title = "Chickens",
                    UserId = id,
                    Thumbnail = "test.png",
                    Rank = 1,
                    Created = new DateTime(2000, 1, 1, 1, 10, 1),
                    Available = true
                };
                var product2 = new Product
                {
                    Title = "Eggs",
                    UserId = id,
                    Thumbnail = "",
                    Rank = 0,
                    Created = new DateTime(2000, 1, 1, 1, 1, 1),
                    Available = true
                };
                var product3 = new Product
                {
                    Title = "Something",
                    UserId = id,
                    Thumbnail = "",
                    Rank = 1,
                    Created = new DateTime(2000, 1, 1, 1, 1, 1),
                    Available = true
                };
                var product4 = new Product
                {
                    Title = "Chickens",
                    UserId = otherId,
                    Available = true
                };
                context.Products.AddRange(product1, product2, product3, product4);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var products = await repository.Read(id).ToListAsync();

                var product = products.ElementAt(0);
                var secondProduct = products.ElementAt(1);
                var thirdProduct = products.ElementAt(2);

                Assert.Equal(1, product.Rank);
                Assert.Equal(product1.Id, product.ProductId);
                Assert.Equal(product1.Title, product.Title);
                Assert.Equal(1, secondProduct.Rank);
                Assert.Equal(product3.Id, secondProduct.ProductId);
                Assert.Equal(product3.Title, secondProduct.Title);
                Assert.Equal(0, thirdProduct.Rank);
                Assert.Equal(product2.Id, thirdProduct.ProductId);
                Assert.Equal(product2.Title, thirdProduct.Title);
            }
        }

        [Fact]
        public async Task Read_given_nonExisting_id_returns_emptyCollection()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
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
                context.Producers.Add(producer);

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = false
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var (status, pendingApplications, sent) = await repository.UpdateAsync(expectedProduct);

                Assert.True(status);
                Assert.Equal(0, pendingApplications);
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

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
                    Rank = 0,
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                await repository.UpdateAsync(expectedProduct);

                var products = await context.Products.FindAsync(product.Id);

                Assert.Equal(expectedProduct.Id, products.Id);
                Assert.Equal(expectedProduct.Available, products.Available);
                Assert.Equal(expectedProduct.Rank, products.Rank);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_existing_id_with_application_open_and_application_closed_updates_product()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = false,
                    UserId = id,
                };

                var application = new Application
                {
                    Id = 1,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Open
                };

                var application1 = new Application
                {
                    Id = 2,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Unavailable,
                };

                context.Products.Add(product);
                context.Applications.AddRange(application, application1);
                await context.SaveChangesAsync();

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = true,
                    Rank = 0,
                };

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var (status, pendingApplications, sent) = await repository.UpdateAsync(expectedProduct);

                var products = await context.Products.FindAsync(product.Id);

                Assert.True(status);
                Assert.Equal(0, pendingApplications);
                Assert.Equal(expectedProduct.Id, products.Id);
                Assert.Equal(expectedProduct.Available, products.Available);
                Assert.Equal(expectedProduct.Rank, products.Rank);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_existing_id_with_application_open_and_application_closed_closes_open_application_and_updates_product()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = true,
                    UserId = id,
                };

                var application = new Application
                {
                    Id = 1,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Open
                };

                var application1 = new Application
                {
                    Id = 2,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Unavailable,
                };

                context.Products.Add(product);
                context.Applications.AddRange(application, application1);
                await context.SaveChangesAsync();

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = false,
                    Rank = 0,
                };

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                await repository.UpdateAsync(expectedProduct);

                var products = await context.Products.FindAsync(product.Id);

                var resultApplication = await context.Applications.FindAsync(application.Id);
                var resultApplication1 = await context.Applications.FindAsync(application1.Id);

                Assert.Equal(expectedProduct.Id, products.Id);
                Assert.Equal(expectedProduct.Available, products.Available);
                Assert.Equal(expectedProduct.Rank, products.Rank);
                Assert.Equal(ApplicationStatusEnum.Unavailable, resultApplication.Status);
                Assert.Equal(ApplicationStatusEnum.Unavailable, resultApplication1.Status);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_existing_id_with_application_open_and_application_open_and_application_closed_closes_open_application_and_updates_product()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = true,
                    UserId = id,
                };

                var application = new Application
                {
                    Id = 1,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Open
                };

                var application1 = new Application
                {
                    Id = 2,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Unavailable,
                };


                var application2 = new Application
                {
                    Id = 3,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Open,
                };

                context.Products.Add(product);
                context.Applications.AddRange(application, application1, application2);
                await context.SaveChangesAsync();

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = false,
                    Rank = 0,
                };

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var (status, pendingApplications, sent) = await repository.UpdateAsync(expectedProduct);

                var products = await context.Products.FindAsync(product.Id);

                var resultApplication = await context.Applications.FindAsync(application.Id);
                var resultApplication1 = await context.Applications.FindAsync(application1.Id);
                var resultApplication2 = await context.Applications.FindAsync(application2.Id);

                Assert.True(status);
                Assert.Equal(0, pendingApplications);
                Assert.Equal(expectedProduct.Id, products.Id);
                Assert.Equal(expectedProduct.Available, products.Available);
                Assert.Equal(expectedProduct.Rank, products.Rank);
                Assert.Equal(ApplicationStatusEnum.Unavailable, resultApplication.Status);
                Assert.Equal(ApplicationStatusEnum.Unavailable, resultApplication1.Status);
                Assert.Equal(ApplicationStatusEnum.Unavailable, resultApplication2.Status);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_existing_id_with_application_setting_unavilable_product_sends_application_cancel_email()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = true,
                    UserId = id,
                };

                var application = new Application
                {
                    Id = 1,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Open
                };

                var application1 = new Application
                {
                    Id = 2,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Unavailable,
                };

                context.Products.Add(product);
                context.Applications.AddRange(application, application1);
                await context.SaveChangesAsync();

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = false,
                    Rank = 0,
                };

                string subject = "PolloPollo application cancelled";
                string body = $"You had an open application for {product.Title} but the Producer has removed the product from the PolloPollo platform, and your application for it has therefore been cancelled.You may log on to the PolloPollo platform to see if the product has been replaced by another product, you want to apply for instead.\n\nSincerely,\nThe PolloPollo Project";

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                emailClient.Setup(e => e.SendEmail(user.Email, subject, body)).Returns((true, null));

                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var (status, pending, (emailSent, emailError)) = await repository.UpdateAsync(expectedProduct);

                emailClient.Verify(e => e.SendEmail(user.Email, subject, body));
                Assert.True(emailSent);

            }
        }

        [Fact]
        public async Task UpdateAsync_given_existing_id_with_application_setting_unavilable_product_propagates_emailError()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = true,
                    UserId = id,
                };

                var application = new Application
                {
                    Id = 1,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Open
                };

                var application1 = new Application
                {
                    Id = 2,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Unavailable,
                };

                context.Products.Add(product);
                context.Applications.AddRange(application, application1);
                await context.SaveChangesAsync();

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = false,
                    Rank = 0,
                };

                string subject = "PolloPollo application cancelled";
                string body = $"You had an open application for {product.Title} but the Producer has removed the product from the PolloPollo platform, and your application for it has therefore been cancelled.You may log on to the PolloPollo platform to see if the product has been replaced by another product, you want to apply for instead.\n\nSincerely,\nThe PolloPollo Project";

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                emailClient.Setup(e => e.SendEmail(user.Email, subject, body)).Returns((false, "Email error"));

                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var (status, pending, (emailSent, emailError)) = await repository.UpdateAsync(expectedProduct);

                emailClient.Verify(e => e.SendEmail(user.Email, subject, body));
                Assert.False(emailSent);
                Assert.Equal("Email error", emailError);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_non_existing_id_returns_false()
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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = false,
                    UserId = id,
                };

                var expectedProduct = new ProductUpdateDTO
                {
                    Id = 42,
                    Available = true,
                    Rank = 0,
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var (status, pendingApplications, sent) = await repository.UpdateAsync(expectedProduct);

                Assert.False(status);
                Assert.Equal(0, pendingApplications);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_existing_id_with_application_pending_returns_PendingApplicationCount()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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

                var producer = new Producer
                {
                    UserId = id,
                    PairingSecret = "secret",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);

                var product = new Product
                {
                    Id = 1,
                    Title = "Eggs",
                    Available = false,
                    UserId = id,
                };

                var application = new Application
                {
                    Id = 1,
                    ProductId = product.Id,
                    UserId = user.Id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Pending,
                };

                context.Products.Add(product);
                context.Applications.Add(application);
                await context.SaveChangesAsync();

                var updateProductDTO = new ProductUpdateDTO
                {
                    Id = product.Id,
                    Available = true,
                };

                var (status, pendingApplications, sent) = await repository.UpdateAsync(updateProductDTO);

                Assert.True(status);
                Assert.Equal(1, pendingApplications);
            }
        }

        [Fact]
        public async Task UpdateImageAsync_given_folder_existing_id_and_image_updates_user_thumbnail()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = ImageFolderEnum.@static.ToString();
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
                    UserId = id,
                    PairingSecret = "ABCD",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
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

                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var update = await repository.UpdateImageAsync(id, formFile.Object);

                var updatedProduct = await context.Products.FindAsync(id);

                Assert.Equal(fileName, updatedProduct.Thumbnail);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(fileName), update);
            }
        }

        [Fact]
        public async Task UpdateImageAsync_given_folder_existing_id_and_image_and_existing_image_Creates_new_image_and_Removes_old_thumbnail()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var folder = ImageFolderEnum.@static.ToString();
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
                    UserId = id,
                    PairingSecret = "ABCD",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
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

                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var update = await repository.UpdateImageAsync(id, formFile.Object);

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
                var folder = ImageFolderEnum.@static.ToString();
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
                    UserId = id,
                    PairingSecret = "ABCD",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
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

                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var ex = await Assert.ThrowsAsync<Exception>(() => repository.UpdateImageAsync(id, formFile.Object));

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
                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

                var update = await repository.UpdateImageAsync(42, formFile.Object);

                Assert.Null(update);
            }
        }

        [Fact]
        public async Task GetCount_returns_0_when_no_products()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var imageWriter = new Mock<IImageWriter>();

                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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
                var id = 1;

                var imageWriter = new Mock<IImageWriter>();

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
                    UserId = id,
                    PairingSecret = "ABCD",
                    Street = "Test",
                    StreetNumber = "Some number",
                    City = "City"
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

                var emailClient = new Mock<IEmailClient>();
                var repository = new ProductRepository(imageWriter.Object, emailClient.Object, context);

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
