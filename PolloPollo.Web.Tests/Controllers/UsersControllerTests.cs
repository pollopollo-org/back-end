using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Shared;
using PolloPollo.Web.Controllers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Web.Tests
{
    public class UsersControllerTests
    {
        private Mock<ClaimsPrincipal> MockClaimsSecurity(int id)
        {
            //Create ClaimIdentity
            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            };
            var identity = new ClaimsIdentity(claims);

            //Mock claim to make the HttpContext contain one.
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(true);
           
            claimsPrincipalMock.Setup(m => m.Claims).Returns(claims);

            return claimsPrincipalMock;
        }

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
        public async Task Put_with_User_id_same_as_claim_calls_update()
        {
            var dto = new UserUpdateDTO
            {
                UserId = 1,
                FirstName = "test",
            };

            var repository = new Mock<IUserRepository>();

            var controller = new UsersController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(dto.UserId);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            await controller.Put(dto);

            repository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task Put_with_different_User_id_as_claim_returns_Forbidden()
        {
            var dto = new UserUpdateDTO
            {
                UserId = 1,
                FirstName = "test",
            };

            var repository = new Mock<IUserRepository>();

            var controller = new UsersController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.Put(dto);

            Assert.IsType<ForbidResult>(put);
        }

        [Fact]
        public async Task Put_with_failed_update_returns_InternalServerError()
        {
            var dto = new UserUpdateDTO
            {
                UserId = 1,
                FirstName = "test",
                NewPassword = "1234"
            };

            var repository = new Mock<IUserRepository>();
            repository.Setup(m => m.UpdateAsync(dto)).ReturnsAsync(false);

            var controller = new UsersController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(dto.UserId);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.Put(dto);
            var result = put as StatusCodeResult;

            Assert.IsType<StatusCodeResult>(put);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task Put_with_valid_dto_returns_NoContent()
        {
            var dto = new UserUpdateDTO
            {
                UserId = 1,
                FirstName = "test",
            };

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(true);


            var controller = new UsersController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(dto.UserId);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            await controller.Put(dto);

            repository.Verify(s => s.UpdateAsync(dto));
        }
    }
}
