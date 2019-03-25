using Microsoft.AspNetCore.Mvc;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Shared;
using PolloPollo.Web.Controllers;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Web.Tests
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task Authenticate_returns_authenticated_tuple()
        {
            var token = "verysecrettoken";
            var id = 1;

            var dto = new AuthenticateDTO
            {
                Email = "test@itu.dk",
                Password = "1234",
            };

            var userDTO = new UserDTO
            {
                UserId = id,
                Email = dto.Email,
                UserRole = UserRoleEnum.Receiver.ToString(),
                FirstName = "test",
                SurName = "test"
            }; 

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.Authenticate(dto.Email, dto.Password)).ReturnsAsync((userDTO, token));

            var controller = new UsersController(repository.Object);

            var result = await controller.Authenticate(dto);

            Assert.Equal("verysecrettoken", result.Value.Token);
            Assert.Equal(userDTO.UserId, result.Value.UserDTO.UserId);
            Assert.Equal(userDTO.Email, result.Value.UserDTO.Email);
            Assert.Equal(userDTO.UserRole, result.Value.UserDTO.UserRole);
            Assert.Equal(userDTO.FirstName, result.Value.UserDTO.FirstName);
            Assert.Equal(userDTO.SurName, result.Value.UserDTO.SurName);
        }

        [Fact]
        public async Task Authenticate_wrong_password_Returns_BadRequest()
        {
            var token = "verysecrettoken";
            var id = 1;

            var user = new User
            {
                Email = "test@itu.dk",
                Password = "1234",
            };
            var dto = new AuthenticateDTO
            {
                Email = "wrong@itu.dk",
                Password = "wrongpassword",
            };

            var userDTO = new UserDTO
            {
                UserId = id,
                Email = dto.Email,
                UserRole = UserRoleEnum.Receiver.ToString(),
                FirstName = "test",
                SurName = "test"
            };

            var responseText = "Username or password is incorrect";

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.Authenticate(user.Email, user.Password)).ReturnsAsync((userDTO,token));

            var controller = new UsersController(repository.Object);

            var authenticate = await controller.Authenticate(dto);

            var result = authenticate.Result as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(responseText, result.Value);
        }

        [Fact]
        public async Task Post_With_Role_Receiver_Creates_and_returns_Receiver()
        {
            var id = 1;
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@itu.dk",
                Password = "1234",
                Role = UserRoleEnum.Receiver.ToString(),
            };

            var expected = new TokenDTO {
                UserDTO = new UserDTO
                {
                    UserId = id,
                    UserRole = dto.Role
                },
            };

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.CreateAsync(It.IsAny<UserCreateDTO>())).ReturnsAsync(expected);

            var controller = new UsersController(repository.Object);

            var post = await controller.Post(dto);
            var result = post.Result as CreatedAtActionResult;
            var resultValue = result.Value as TokenDTO;

            repository.Verify(s => s.CreateAsync(dto));

            Assert.Equal("Get", result.ActionName);
            Assert.Equal(id, result.RouteValues["id"]);
            Assert.Equal(dto.Role, resultValue.UserDTO.UserRole);
            Assert.Equal(id, resultValue.UserDTO.UserId);
        }

        [Fact]
        public async Task Post_with_Role_Producer_creates_and_returns_Producer()
        {
            var id = 1;
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@itu.dk",
                Password = "1234",
                Role = UserRoleEnum.Producer.ToString(),

            };

            var expected = new TokenDTO
            {
                UserDTO = new UserDTO
                {
                    UserId = id,
                    UserRole = dto.Role
                }
            };

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.CreateAsync(It.IsAny<UserCreateDTO>())).ReturnsAsync(expected);

            var controller = new UsersController(repository.Object);

            var post = await controller.Post(dto);
            var result = post.Result as CreatedAtActionResult;
            var resultValue = result.Value as TokenDTO;

            repository.Verify(s => s.CreateAsync(dto));

            Assert.Equal("Get", result.ActionName);
            Assert.Equal(id, result.RouteValues["id"]);
            Assert.Equal(dto.Role, resultValue.UserDTO.UserRole);
            Assert.Equal(id, resultValue.UserDTO.UserId);
        }

        [Fact]
        public async Task Post_With_no_Role_returns_BadRequest_with_error_message()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@itu.dk",
                Password = "1234",
            };

            var responseText = "Users must have a assigned a valid role";

            var repository = new Mock<IUserRepository>();

            var controller = new UsersController(repository.Object);

            var post = await controller.Post(dto);
            var result = post.Result as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(post.Result);
            Assert.Equal(responseText, result.Value);
        }

        [Fact]
        public async Task Post_With_invalid_Role_returns_BadRequest_with_error_message()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@itu.dk",
                Password = "1234",
                Role = "test"
            };

            var responseText = "Users must have a assigned a valid role";

            var repository = new Mock<IUserRepository>();

            var controller = new UsersController(repository.Object);

            var post = await controller.Post(dto);
            var result = post.Result as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(post.Result);
            Assert.Equal(responseText, result.Value);
        }

        [Fact]
        public async Task Post_With_existing_user_returns_Conflict()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@itu.dk",
                Password = "1234",
                Role = UserRoleEnum.Producer.ToString()
            };

            var repository = new Mock<IUserRepository>();

            var controller = new UsersController(repository.Object);

            var post = await controller.Post(dto);

            Assert.IsType<ConflictResult>(post.Result);
        }

        [Fact]
        public async Task Get_with_existing_id_returns_user()
        {
            var input = 1;
            
            var expected = new UserDTO
            {
                UserId = input
            };

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            var controller = new UsersController(repository.Object);

            var get = await controller.Get(input);

            Assert.Equal(expected.UserId, get.Value.UserId);
        }

        [Fact]
        public async Task Get_with_existing_id_and_role_receiver_returns_receiver()
        {
            var input = 1;

            var expected = new ReceiverDTO
            {
                UserId = input,
                UserRole = UserRoleEnum.Receiver.ToString()
            };

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            var controller = new UsersController(repository.Object);

            var get = await controller.Get(input);
            var result = get.Value as ReceiverDTO;

            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.UserRole, result.UserRole);
        }

        [Fact]
        public async Task Get_with_existing_id_and_role_receiver_returns_producer()
        {
            var input = 1;

            var expected = new ProducerDTO
            {
                UserId = input,
                UserRole = UserRoleEnum.Producer.ToString(),
                Wallet = "test"
            };

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            var controller = new UsersController(repository.Object);

            var get = await controller.Get(input);
            var result = get.Value as ProducerDTO;

            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.UserRole, result.UserRole);
            Assert.Equal(expected.Wallet, result.Wallet);
        }

        [Fact]
        public async Task Get_with_non_existing_id_returns_NotFound()
        {
            var input = 1;

            var repository = new Mock<IUserRepository>();

            var controller = new UsersController(repository.Object);

            var get = await controller.Get(input);

            Assert.IsType<NotFoundResult>(get.Result);
        }

        [Fact]
        public async Task GetWhenNotExistingInputIdReturnsNotFound()
        {
            var repository = new Mock<IUserRepository>();

            var controller = new UsersController(repository.Object);

            var get = await controller.Get(0);

            Assert.IsType<NotFoundResult>(get.Result);
        }





        /*
                [Fact]
        public async Task PutGivenDtoUpdatesEntity()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var dto = new ProducerUpdateDTO
            {
                Email = "non_existing_user@itu.dk"
            };

            await controller.Put(dto);

            repository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task PutReturnsNoContent()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var dto = new ProducerUpdateDTO
            {
                Email = "non_existing_user@itu.dk"
            };

            repository.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(true);

            var put = await controller.Put(dto);

            Assert.IsType<NoContentResult>(put);
        }


        [Fact]
        public async Task PutGivenRepositoryReturnsFalseReturnsNotFound()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var dto = new ProducerUpdateDTO
            {
                Email = "non_existing_user@itu.dk"
            };

            var put = await controller.Put(dto);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task PutGivenRepositoryReturnsUnauthorizedResult()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var dto = new ProducerUpdateDTO();

            var put = await controller.Put(dto);

            Assert.IsType<UnauthorizedResult>(put);
        }
         */



        

    }
}
