using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
    public class ApplicationRepositoryTests
    {
        [Fact]
        public async Task CreateAsync_given_null_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

                var result = await repository.CreateAsync(default(ApplicationCreateDTO));

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_given_empty_DTO_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

                var applicationDTO = new ApplicationCreateDTO
                {
                    //Nothing
                };

                var result = await repository.CreateAsync(applicationDTO);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_given_invalid_DTO_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

                var applicationDTO = new ApplicationCreateDTO
                {
                    Motivation = "This is not a very good motivation.",
                };

                var result = await repository.CreateAsync(applicationDTO);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_returns_DTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

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
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var product = new Product
                {
                    Id = 42,
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "Test",
                    Location = "Test",
                    Country = "Test",
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var applicationDTO = new ApplicationCreateDTO
                {
                    UserId = user.Id,
                    ProductId = product.Id,
                    Motivation = "Test",
                };

                var result = await repository.CreateAsync(applicationDTO);

                Assert.Equal(applicationDTO.UserId, result.ReceiverId);
                Assert.Equal(applicationDTO.Motivation, result.Motivation);
                Assert.Equal(ApplicationStatusEnum.Open, result.Status);
            }
        }

        [Fact]
        public async Task CreateAsync_given_DTO_sets_Timestamp_in_database()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

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
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var product = new Product
                {
                    Id = 42,
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "Test",
                    Location = "Test",
                    Country = "Test",
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var applicationDTO = new ApplicationCreateDTO
                {
                    UserId = user.Id,
                    ProductId = product.Id,
                    Motivation = "Test",
                };

                var result = await repository.CreateAsync(applicationDTO);

                var now = DateTime.UtcNow;
                var dbTimestamp = context.Applications.Find(result.ApplicationId).TimeStamp;

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
                var repository = new ApplicationRepository(context);

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
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var product = new Product
                {
                    Id = 42,
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "Test",
                    Location = "Test",
                    Country = "Test",
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var applicationDTO = new ApplicationCreateDTO
                {
                    UserId = user.Id,
                    ProductId = product.Id,
                    Motivation = "Test",
                };

                var result = await repository.CreateAsync(applicationDTO);

                var expectedId = 1;

                Assert.Equal(expectedId, result.ApplicationId);
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
                    Country = "DK",
                    Thumbnail = "test"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var product = new Product
                {
                    Id = id,
                    Title = "5 chickens",
                    UserId = 1,
                    Price = 42,
                    Description = "Test",
                    Location = "Test",
                    Country = "Test",
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var entity = new Application
                {
                    UserId = id,
                    ProductId = id,
                    Motivation = "Test",
                    TimeStamp = new DateTime(2019, 04, 08),
                    Status = ApplicationStatusEnum.Open
                };

                context.Applications.Add(entity);
                await context.SaveChangesAsync();

                var repository = new ApplicationRepository(context);

                var application = await repository.FindAsync(entity.Id);

                Assert.Equal(entity.Id, application.ApplicationId);
                Assert.Equal(entity.UserId, application.ReceiverId);
                Assert.Equal($"{user.FirstName} {user.SurName}", application.ReceiverName);
                Assert.Equal(user.Country, application.Country);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(user.Thumbnail), application.Thumbnail);
                Assert.Equal(product.Title, application.ProductTitle);
                Assert.Equal(product.Price, application.ProductPrice);
                Assert.Equal(product.UserId, application.ProducerId);
                Assert.Equal(entity.Motivation, application.Motivation);
                Assert.Equal(entity.Status, application.Status);
            }
        }

        [Fact]
        public async Task FindAsync_given_nonExisting_Id_returns_null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

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
                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "DK",
                    Thumbnail = "test"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var product = new Product
                {
                    Id = id,
                    Title = "5 chickens",
                    UserId = id,
                    Price = 42,
                    Description = "Test",
                    Location = "Test",
                    Country = "Test",
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var entity1 = new Application
                {
                    UserId = id,
                    ProductId = id,
                    Motivation = "Test",
                    TimeStamp = new DateTime(2019, 04, 08),
                    Status = ApplicationStatusEnum.Open
                };

                var entity2 = new Application
                {
                    UserId = id,
                    ProductId = id,
                    Motivation = "Test",
                    TimeStamp = new DateTime(2019, 03, 08),
                    Status = ApplicationStatusEnum.Pending
                };


                context.Applications.AddRange(entity1, entity2);
                await context.SaveChangesAsync();

                var repository = new ApplicationRepository(context);

                var applications = repository.ReadOpen();

                // There should only be one application in the returned list
                // since one of the created applications is not open
                var count = applications.ToList().Count;
                Assert.Equal(1, count);

                var application = applications.First();



                Assert.Equal(entity1.Id, application.ApplicationId);
                Assert.Equal(entity1.UserId, application.ReceiverId);
                Assert.Equal(user.FirstName + " " + user.SurName, application.ReceiverName);
                Assert.Equal(user.Country, application.Country);
                Assert.Equal("static/" + user.Thumbnail, application.Thumbnail);
                Assert.Equal(product.Title, application.ProductTitle);
                Assert.Equal(product.Price, application.ProductPrice);
                Assert.Equal(product.UserId, application.ProducerId);
                Assert.Equal(entity1.Motivation, application.Motivation);
                Assert.Equal(entity1.Status, application.Status);
            }
        }

        [Fact]
        public async Task Read_given_existing_id_returns_all_products_by_specified_user_id()
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
                    Country = "DK",
                    Thumbnail = "test"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Receiver
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

                var product = new Product
                {
                    Id = 1,
                    Title = "5 chickens",
                    UserId = id,
                    Price = 42,
                    Description = "Test",
                    Location = "Test",
                    Country = "Test",
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Users.Add(otherUser);
                context.UserRoles.Add(otherUserEnumRole);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var entity1 = new Application
                {
                    UserId = id,
                    ProductId = product.Id,
                    Motivation = "Test",
                    TimeStamp = new DateTime(2019, 04, 08),
                    Status = ApplicationStatusEnum.Open
                };

                var entity2 = new Application
                {
                    UserId = id,
                    ProductId = product.Id,
                    Motivation = "Test",
                    TimeStamp = new DateTime(2019, 03, 08),
                    Status = ApplicationStatusEnum.Pending
                };

                var entity3 = new Application
                {
                    UserId = otherId,
                    ProductId = product.Id,
                    Motivation = "Test",
                    TimeStamp = new DateTime(2019, 03, 08),
                    Status = ApplicationStatusEnum.Pending
                };

                context.Applications.AddRange(entity1, entity2, entity3);
                await context.SaveChangesAsync();

                var repository = new  ApplicationRepository(context);

                var applications = repository.Read(id);

                // There should only be two products in the returned list
                // since one of the created products is by another producer
                var count = applications.ToList().Count;
                Assert.Equal(2, count);

                var application = applications.First();
                var secondApplication = applications.Last();

                Assert.Equal(entity1.Id, application.ApplicationId);
                Assert.Equal(entity1.UserId, application.ReceiverId);
                Assert.Equal($"{user.FirstName} {user.SurName}", application.ReceiverName);
                Assert.Equal(user.Country, application.Country);
                Assert.Equal(ImageHelper.GetRelativeStaticFolderImagePath(user.Thumbnail), application.Thumbnail);
                Assert.Equal(product.Title, application.ProductTitle);
                Assert.Equal(product.Price, application.ProductPrice);
                Assert.Equal(product.UserId, application.ProducerId);
                Assert.Equal(entity1.Motivation, application.Motivation);
                Assert.Equal(entity1.Status, application.Status);

                Assert.Equal(entity2.Id, secondApplication.ApplicationId);
                Assert.Equal(entity2.UserId, secondApplication.ReceiverId);
                Assert.Equal(product.UserId, secondApplication.ProducerId);
            }
        }

        [Fact]
        public async Task Read_given_nonExisting_id_returns_emptyCollection()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

                var result = repository.Read(42);
                Assert.Empty(result);
            }
        }

        [Fact]
        private async Task DeleteAsync_given_invalid_id_returns_false()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

                var result = await repository.DeleteAsync(42, 42);

                Assert.False(result);
            }
        }

        [Fact]
        private async Task DeleteAsync_given_existing_id_deletes()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

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
                    Id = id,
                    Title = "test",
                    UserId = id,
                    Thumbnail = "",
                };

                var application = new Application
                {
                    Id = id,
                    UserId = id,
                    ProductId = id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Open
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                context.Products.Add(product);
                context.Applications.Add(application);
                await context.SaveChangesAsync();

                var deletion = await repository.DeleteAsync(id, id);

                var find = await repository.FindAsync(id);

                Assert.True(deletion);
                Assert.Null(find);
            }
        }

        [Fact]
        private async Task DeleteAsync_deleting_not_open_application_returns_false()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var repository = new ApplicationRepository(context);

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
                    Id = id,
                    Title = "test",
                    UserId = id,
                    Thumbnail = "",
                };

                var application = new Application
                {
                    Id = id,
                    UserId = id,
                    ProductId = id,
                    Motivation = "test",
                    Status = ApplicationStatusEnum.Pending
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                context.Products.Add(product);
                context.Applications.Add(application);
                await context.SaveChangesAsync();

                var deletion = await repository.DeleteAsync(id, id);

                var find = await repository.FindAsync(id);

                Assert.False(deletion);
                Assert.NotNull(find);
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
