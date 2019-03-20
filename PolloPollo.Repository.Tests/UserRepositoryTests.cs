using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Repository.Tests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task Authenticate_given_valid_Password_returns_Token()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();

                var repository = new UserRepository(config, context);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Test",
                    Surname = "Test",
                    Email = "Test@itu.dk",
                    Country = "DK",
                    Password = repository.HashPassword("Test@itu.dk", plainPassword)
                };

                context.Users.Add(user);
                context.SaveChanges();

                var token = repository.Authenticate(user.Email, plainPassword);

                Assert.NotNull(token);
            }
        }

        [Fact]
        public async Task Authenticate_given_non_existing_user_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);
                var givenPassword = "verysecret123";
                var email = "Test@itu.dk";

                var token = repository.Authenticate(email, givenPassword);
                Assert.Null(token);
            }
        }

        [Fact]
        public async Task Authenticate_given_invalid_Password_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Test",
                    Surname = "Test",
                    Email = "Test@itu.dk",
                    Country = "DK",
                    Password = repository.HashPassword("Test@itu.dk", plainPassword)
                };

                context.Users.Add(user);
                context.SaveChanges();

                var token = repository.Authenticate(user.Email, "wrongpassword");
                Assert.Null(token);
            }
        }

        [Fact]
        public async Task CreateAsync_with_User_invalid_role_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var dto = new UserCreateDTO
                {
                    FirstName = "Test",
                    SurName = "Test",
                    Email = "Test@itu.dk",
                    Country = "DK",
                    Role = "test",
                    Password = "secret"
                };

                var tokenDTO = await repository.CreateAsync(dto);

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task CreateAsync_with_role_Receiver_creates_Receiver_and_returns_TokenDTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var dto = new UserCreateDTO
                {
                    FirstName = "Test",
                    SurName = "Test",
                    Email = "Test@itu.dk",
                    Country = "DK",
                    Role = UserRoleEnum.Receiver.ToString(),
                    Password = "secret"
                };

                var expectedDTO = new TokenDTO
                {
                    UserDTO = new UserDTO
                    {
                        UserId = 1,
                        UserRole = UserRoleEnum.Receiver.ToString(),
                        Email = dto.Email
                    }
                };

                var tokenDTO = await repository.CreateAsync(dto);

                Assert.Equal(expectedDTO.UserDTO.UserId, tokenDTO.UserDTO.UserId);
                Assert.Equal(expectedDTO.UserDTO.UserRole, tokenDTO.UserDTO.UserRole);
                Assert.Equal(expectedDTO.UserDTO.Email, tokenDTO.UserDTO.Email);
            }
        }

        [Fact]
        public async Task CreateAsync_with_role_Producer_creates_Producer_and_returns_TokenDTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var dto = new UserCreateDTO
                {
                    FirstName = "Test",
                    SurName = "Test",
                    Email = "Test@itu.dk",
                    Country = "DK",
                    Role = UserRoleEnum.Producer.ToString(),
                    Password = "secret"
                };

                var expectedDTO = new TokenDTO
                {
                    UserDTO = new UserDTO
                    {
                        UserId = 1,
                        UserRole = UserRoleEnum.Producer.ToString(),
                        Email = dto.Email
                    }
                };

                var tokenDTO = await repository.CreateAsync(dto);

                Assert.Equal(expectedDTO.UserDTO.UserId, tokenDTO.UserDTO.UserId);
                Assert.Equal(expectedDTO.UserDTO.UserRole, tokenDTO.UserDTO.UserRole);
                Assert.Equal(expectedDTO.UserDTO.Email, tokenDTO.UserDTO.Email);
            }
        }

        [Fact]
        public async Task CreateAsync_with_empty_DTO_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var dto = new UserCreateDTO();

                var tokenDTO = await repository.CreateAsync(dto);

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task CreateAsync_with_Null_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var tokenDTO = await repository.CreateAsync(default(UserCreateDTO));

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task FindAsync_with_existing_id_returns_User()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    Surname = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var expected = new UserDTO
                {
                    UserId = 1,
                    Email = user.Email
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                await context.SaveChangesAsync();

                var userDTO = await repository.FindAsync(id);

                Assert.Equal(expected.UserId, userDTO.UserId);
                Assert.Equal(expected.Email, userDTO.Email);
            }
        }

        [Fact]
        public async Task FindAsync_with_existing_id_for_User_with_invalid_Role_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    Surname = "test",
                    Country = "DK"
                };

                var expected = new UserDTO
                {
                    UserId = 1,
                    Email = user.Email
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                var userDTO = await repository.FindAsync(id);

                Assert.Null(userDTO);
            }
        }

        [Fact]
        public async Task FindAsync_with_existing_id_for_Receiver_returns_Receiver()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    Surname = "test",
                    Country = "DK"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                var expected = new ReceiverDTO
                {
                    UserId = 1,
                    Email = user.Email,
                    UserRole = userEnumRole.UserRoleEnum.ToString(),
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var userDTO = await repository.FindAsync(id);

                Assert.Equal(expected.UserId, userDTO.UserId);
                Assert.Equal(expected.Email, userDTO.Email);
                Assert.Equal(expected.UserRole, userDTO.UserRole);
            }
        }

        [Fact]
        public async Task FindAsync_with_existing_id_for_Producer_returns_Producer()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@itu.dk",
                    Password = "1234",
                    FirstName = "test",
                    Surname = "test",
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

                var expected = new ProducerDTO
                {
                    UserId = 1,
                    Email = user.Email,
                    UserRole = userEnumRole.UserRoleEnum.ToString(),
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var userDTO = await repository.FindAsync(id);

                Assert.Equal(expected.UserId, userDTO.UserId);
                Assert.Equal(expected.Email, userDTO.Email);
                Assert.Equal(expected.UserRole, userDTO.UserRole);
            }
        }

        [Fact]
        public async Task FindAsync_with_non_existing_id_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var repository = new UserRepository(config, context);

                var id = 1;

                var userDTO = await repository.FindAsync(id);

                Assert.Null(userDTO);
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
