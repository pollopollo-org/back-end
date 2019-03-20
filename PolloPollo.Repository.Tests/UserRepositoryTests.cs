using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Repository.Tests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task AuthenticateGivenValidUserReturnsUserWithToken()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();

                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var repository = new UserRepository(config, context, producerRepo, receiverRepo);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = repository.HashPassword("stei@itu.dk", plainPassword)
                };

                context.Users.Add(user);
                context.SaveChanges();

                var token = repository.Authenticate(user.Email, plainPassword);

                Assert.NotNull(token);
            }
        }

        [Fact]
        public async Task AuthenticateGivenNotExistingUserReturnsNull()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context, producerRepo, receiverRepo);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = repository.HashPassword("stei@itu.dk", plainPassword)
                };

                var token = repository.Authenticate(user.Email, user.Password);
                Assert.Null(token);
            }
        }

        [Fact]
        public async Task AuthenticateGivenWrongPasswordAndEmailReturnsNull()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context, producerRepo, receiverRepo);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Password = repository.HashPassword("stei@itu.dk", plainPassword)
                };

                context.Users.Add(user);
                context.SaveChanges();

                var token = repository.Authenticate("wrongemail@itu.dk", "wrongpassword");
                Assert.Null(token);
            }
        }

        [Fact]
        public async Task CreateAsyncWhenRoleReceiverCreatesReceiverAndReturnsId()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context, producerRepo, receiverRepo);

                var dto = new UserCreateDTO
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Role = "Receiver",
                    Password = "secret"
                };

                var token = await repository.CreateAsync(dto);

          //      var receiver = await repository.FindAsync(token.UserDTO);

          //      Assert.Equal(token.UserDTO, receiver.UserId);
            }
        }

        [Fact]
        public async Task CreateAsyncWhenRoleProducerCreatesProducerAndReturnsId()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);

                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context, producerRepo, receiverRepo);

                var userRole = new UserRole
                {
                    UserId = 1,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var dto = new UserCreateDTO
                {
                    FirstName = "Christina",
                    Surname = "Steinhauer",
                    Email = "stei@itu.dk",
                    Country = "DK",
                    Role = "Producer",
                    Password = "secret"
                };

                var token = await repository.CreateAsync(dto);

                Assert.Equal(dto.Email, token.UserDTO.Email);
                Assert.Equal(userRole.UserId, token.UserDTO.UserId);

       //         var producer = await repository.FindAsync(token.UserDTO);

         //       Assert.Equal(token.UserDTO, producer.UserId);
            }
        }

        [Fact]
        public async Task StoreImageAsyncShouldStoreImageOnFileSystemAndReturnPath()
        {
            var imagePath = Path.Combine(ApplicationRoot.getWebRoot(), "static", "1.jpg");

            var image = Image.FromFile(imagePath);

            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(imagePath);
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "1.jpg";
            file.Setup(f => f.ContentType).Returns("jpg");
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(f => f.Length).Returns(ms.Length);
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
                .Verifiable();


            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {

                var producerRepo = new ProducerRepository(context);
                var receiverRepo = new ReceiverRepository(context);
                var config = GetSecurityConfig();

                var userRepo = new UserRepository(config, context, producerRepo, receiverRepo);

                var result = await userRepo.StoreImageAsync(file.Object);
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
                Secret = "0d797046248eeb96eb32a0e5fdc674f5ad862cad",
            };
            return Options.Create(config as SecurityConfig);
        }
    }
}
