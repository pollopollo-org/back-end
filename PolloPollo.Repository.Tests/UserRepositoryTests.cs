using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Repository.Utils;
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
        public async Task Authenticate_given_valid_Password_returns_Token()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Test",
                    SurName = "Test",
                    Email = "Test@Test",
                    Country = "CountryCode",
                    Password = PasswordHasher.HashPassword("Test@Test", plainPassword)
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
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);
                var givenPassword = "verysecret123";
                var email = "Test@Test";

                var (userDTO, token) = await repository.Authenticate(email, givenPassword);

                Assert.Null(token);
                Assert.Null(userDTO);
            }
        }

        [Fact]
        public async Task Authenticate_given_invalid_Password_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);
                var plainPassword = "verysecret123";
                var user = new User
                {
                    FirstName = "Test",
                    SurName = "Test",
                    Email = "Test@Test",
                    Country = "CountryCode",
                    Password = PasswordHasher.HashPassword("Test@Test", plainPassword)
                };

                context.Users.Add(user);
                context.SaveChanges();

                var (id, token) = await repository.Authenticate(user.Email, "wrongpassword");
                Assert.Null(token);
            }
        }

        [Fact]
        public async Task CreateAsync_given_User_invalid_role_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var dto = new UserCreateDTO
                {
                    FirstName = "Test",
                    SurName = "Test",
                    Email = "Test@Test",
                    Country = "CountryCode",
                    UserRole = "test",
                    Password = "12345678"
                };

                var tokenDTO = await repository.CreateAsync(dto);

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task CreateAsync_given_role_Receiver_creates_Receiver_and_returns_TokenDTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var dto = new UserCreateDTO
                {
                    FirstName = "Test",
                    SurName = "Test",
                    Email = "Test@Test",
                    Country = "CountryCode",
                    UserRole = UserRoleEnum.Receiver.ToString(),
                    Password = "12345678"
                };

                var expectedDTO = new TokenDTO
                {
                    UserDTO = new DetailedUserDTO
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
        public async Task CreateAsync_given_role_Producer_creates_Producer_and_returns_TokenDTO()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var dto = new UserCreateDTO
                {
                    FirstName = "Test",
                    SurName = "Test",
                    Email = "Test@Test",
                    Country = "CountryCode",
                    UserRole = UserRoleEnum.Producer.ToString(),
                    Password = "12345678"
                };

                var expectedDTO = new TokenDTO
                {
                    UserDTO = new DetailedUserDTO
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
        public async Task CreateAsync_given_empty_DTO_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var dto = new UserCreateDTO();

                var tokenDTO = await repository.CreateAsync(dto);

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task CreateAsync_given_Null_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var tokenDTO = await repository.CreateAsync(default(UserCreateDTO));

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task CreateAsync_given_no_password_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var userCreateDTO = new UserCreateDTO
                {
                    Password = ""
                };

                var tokenDTO = await repository.CreateAsync(userCreateDTO);

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task CreateAsync_given_Password_under_8_length_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var userCreateDTO = new UserCreateDTO
                {
                    Password = "1234"
                };

                var tokenDTO = await repository.CreateAsync(userCreateDTO);

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task CreateAsync_given_existing_user_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var user = new User
                {
                    Email = "test@test",
                    Password = "12345678",
                };

                var userCreateDTO = new UserCreateDTO
                {
                    Email = "test@test",
                    Password = "87654321"
                };

                context.Users.Add(user);

                var tokenDTO = await repository.CreateAsync(userCreateDTO);

                Assert.Null(tokenDTO);
            }
        }

        [Fact]
        public async Task FindAsync_given_existing_id_returns_User()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var expected = new DetailedUserDTO
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
        public async Task FindAsync_given_existing_id_for_User_with_invalid_Role_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
                };

                var expected = new DetailedUserDTO
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
        public async Task FindAsync_given_existing_id_for_Receiver_returns_Receiver()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
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

                var expected = new DetailedReceiverDTO
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
        public async Task FindAsync_given_existing_id_for_Producer_returns_Producer()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = "1234",
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
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

                var expected = new DetailedProducerDTO
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
        public async Task FindAsync_given_non_existing_id_returns_Null()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var userDTO = await repository.FindAsync(id);

                Assert.Null(userDTO);
            }
        }

        /* 
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
                var config = GetSecurityConfig();

                var userRepo = new UserRepository(config, context);

                var result = await userRepo.StoreImageAsync(file.Object);
            }
        }
        */
   
        [Fact]
        public async Task UpdateAsync_given_Receiver_User_returns_True()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = PasswordHasher.HashPassword("test@Test", "12345678"),
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
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

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var dto = new UserUpdateDTO
                {
                    UserId = id,
                    FirstName = "Test",
                    SurName = "test",
                    Email = "test@Test",
                    Country = "CountryCode",
                    Password = "12345678",
                    UserRole = userEnumRole.UserRoleEnum.ToString(),
                };

                var result = await repository.UpdateAsync(dto);

                Assert.True(result);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_Producer_User_returns_True()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = PasswordHasher.HashPassword("test@Test", "12345678"),
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
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

                var dto = new UserUpdateDTO
                {
                    UserId = id,
                    FirstName = "Test",
                    SurName = "test",
                    Email = "test@Test",
                    Country = "CountryCode",
                    Password = "12345678",
                    UserRole = userEnumRole.UserRoleEnum.ToString(),
                };

                var result = await repository.UpdateAsync(dto);

                Assert.True(result);
            }
        }

          [Fact]
        public async Task UpdateAsync_given_User_no_role_returns_False()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = "12345678",
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
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

                var dto = new UserUpdateDTO
                {
                    UserId = id,
                    FirstName = "Test",
                    SurName = "test",
                    Email = "test@Test",
                    Country = "CountryCode",
                    Password = "12345678",
                    UserRole = "",
                };

                var result = await repository.UpdateAsync(dto);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_User_wrong_role_returns_False()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = "12345678",
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
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

                var dto = new UserUpdateDTO
                {
                    UserId = id,
                    FirstName = "Test",
                    SurName = "test",
                    Email = "test@Test",
                    Country = "CountryCode",
                    Password = "12345678",
                    UserRole = "Customer",
                };

                var result = await repository.UpdateAsync(dto);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_DTO_updates_User_information()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = PasswordHasher.HashPassword("test@Test", "1234"),
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
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

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var dto = new UserUpdateDTO
                {
                    UserId = id,
                    FirstName = "Test test",
                    SurName = "test Test",
                    Email = user.Email,
                    Country = "UK",
                    Password = "1234",
                    NewPassword = "123456789",
                    Description = "Test Test",
                    UserRole = userEnumRole.UserRoleEnum.ToString(),
                };

                var update = await repository.UpdateAsync(dto);

                var updatedUser = await repository.FindAsync(id);

                var updatedPassword = (await context.Users.FindAsync(dto.UserId)).Password;
                var passwordCheck = PasswordHasher.VerifyPassword(dto.Email, updatedPassword, dto.NewPassword);

                Assert.Equal(dto.FirstName, updatedUser.FirstName);
                Assert.Equal(dto.SurName, updatedUser.SurName);
                Assert.Equal(dto.Country, updatedUser.Country);
                Assert.Equal(dto.Description, updatedUser.Description);

                Assert.True(passwordCheck);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_NewPassword_under_8_Length_returns_False()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = PasswordHasher.HashPassword("test@Test", "12345678"),
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
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

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var dto = new UserUpdateDTO
                {
                    UserId = id,
                    FirstName = "Test test",
                    SurName = "test Test",
                    Email = user.Email,
                    Country = "UK",
                    Password = "12345678",
                    NewPassword = "12345",
                    Description = "Test Test",
                    City = "test",
                    UserRole = userEnumRole.UserRoleEnum.ToString(),
                };

                var update = await repository.UpdateAsync(dto);
                
                Assert.False(update);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_Producer_change_wallet_updates_Wallet()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

                var user = new User
                {
                    Id = id,
                    Email = "test@Test",
                    Password = PasswordHasher.HashPassword("test@Test", "12345678"),
                    FirstName = "test",
                    SurName = "test",
                    Country = "CountryCode"
                };

                var userEnumRole = new UserRole
                {
                    UserId = id,
                    UserRoleEnum = UserRoleEnum.Producer
                };

                var Producer = new Producer
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(Producer);
                await context.SaveChangesAsync();

                var dto = new UserUpdateDTO
                {
                    UserId = id,
                    FirstName = "Test",
                    SurName = "test",
                    Email = "test@Test",
                    Country = "CountryCode",
                    Password = "12345678",
                    UserRole = userEnumRole.UserRoleEnum.ToString(),
                    Wallet = "Test Test Wallet",
                };

                await repository.UpdateAsync(dto);

                var updated = await repository.FindAsync(id);
                var newDTO = updated as DetailedProducerDTO;

                Assert.Equal(dto.Wallet, newDTO.Wallet);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_non_existing_id_returns_False()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var nonExistingUser = new UserUpdateDTO
                {
                    UserId = 0,
                    FirstName = "test",
                    SurName = "tst",
                    Email = "test@Test",
                    Country = "CountryCode",
                    Password = "1234",
                };

                var result = await repository.UpdateAsync(nonExistingUser);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task UpdateAsync_given_invalid_dto_returns_False()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var id = 1;

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
                    UserRoleEnum = UserRoleEnum.Receiver
                };

                var receiver = new Receiver
                {
                    UserId = id
                };

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Receivers.Add(receiver);
                await context.SaveChangesAsync();

                var dto = new UserUpdateDTO
                {
                    UserId = id,
                    Email = "test@Test",
                    Country = "CountryCode",
                    Password = "12345678",
                    UserRole = userEnumRole.UserRoleEnum.ToString(),
                };

                var result = await repository.UpdateAsync(dto);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task UpdateImageAsync_given_folder_existing_id_and_image_updates_user_thumbnail()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();

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

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                await context.SaveChangesAsync();

                var repository = new UserRepository(config, imageWriter.Object, context);

                var update = await repository.UpdateImageAsync(folder, id, formFile.Object);

                var updatedUser = await context.Users.FindAsync(id);

                Assert.Equal(fileName, updatedUser.Thumbnail);
                Assert.Equal(fileName, update);
            }
        }

        [Fact]
        public async Task UpdateImageAsync_given_folder_existing_id_and_image_and_existing_image_Creates_new_image_and_Removes_old_thumbnail()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
                var config = GetSecurityConfig();

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

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                await context.SaveChangesAsync();

                var repository = new UserRepository(config, imageWriter.Object, context);

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
                var config = GetSecurityConfig();

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

                context.Users.Add(user);
                context.UserRoles.Add(userEnumRole);
                context.Producers.Add(producer);
                await context.SaveChangesAsync();

                var repository = new UserRepository(config, imageWriter.Object, context);

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
                var config = GetSecurityConfig();
                var imageWriter = new Mock<IImageWriter>();
                var repository = new UserRepository(config, imageWriter.Object, context);

                var update = await repository.UpdateImageAsync("folder", 42, formFile.Object);

                Assert.Null(update);
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
