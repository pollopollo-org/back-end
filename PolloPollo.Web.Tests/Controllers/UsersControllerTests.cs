using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Services;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace PolloPollo.Web.Controllers.Tests
{
    public class UsersControllerTests
    {
        private Mock<IWebHostEnvironment> env;
        private Mock<ILogger<UsersController>> logger;
        private Mock<IUserRepository> userrepository;
        private Mock<IDonorRepository> donorrepository;
        private UsersController controller;

        public UsersControllerTests()
        {
            env = new Mock<IWebHostEnvironment>();
            logger = new Mock<ILogger<UsersController>>();
            userrepository = new Mock<IUserRepository>();
            donorrepository = new Mock<IDonorRepository>();
            controller = new UsersController(userrepository.Object, donorrepository.Object, env.Object, logger.Object);
        }

        private Mock<ClaimsPrincipal> MockClaimsSecurity(int id)
        {
            //Create Claims
            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            };

            //Mock claim to make the HttpContext contain one.
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(true);

            claimsPrincipalMock.Setup(m => m.Claims).Returns(claims);

            return claimsPrincipalMock;
        }

        [Fact]
        public void UsersController_has_AuthorizeAttribute()
        {
            var controller = typeof(UsersController);

            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public async Task Authenticate_given_valid_user_Email_and_Password_match_returns_authenticated_tuple()
        {
            var token = "verysecrettoken";
            var id = 1;

            var dto = new AuthenticateDTO
            {
                Email = "test@Test",
                Password = "1234",
            };

            var userDTO = new DetailedUserDTO
            {
                UserId = id,
                Email = dto.Email,
                UserRole = UserRoleEnum.Receiver.ToString(),
                FirstName = "test",
                SurName = "test"
            };

            userrepository.Setup(s => s.Authenticate(dto.Email, dto.Password)).ReturnsAsync((UserAuthStatus.SUCCESS, userDTO, token));

            var result = await controller.Authenticate(dto);
            var objectresult = result as OkObjectResult;
            var resultobject = objectresult.Value as TokenDTO;

            Assert.Equal("verysecrettoken", resultobject.Token);
            Assert.Equal(userDTO.UserId, resultobject.UserDTO.UserId);
            Assert.Equal(userDTO.Email, resultobject.UserDTO.Email);
            Assert.Equal(userDTO.UserRole, resultobject.UserDTO.UserRole);
            Assert.Equal(userDTO.FirstName, resultobject.UserDTO.FirstName);
            Assert.Equal(userDTO.SurName, resultobject.UserDTO.SurName);
        }

        [Fact]
        public async Task Authenticate_given_valid_donor_Email_and_Password_match_returns_authenticated_tuple()
        {
            var token = "verysecrettoken";

            var dto = new AuthenticateDTO
            {
                Email = "test@Test",
                Password = "1234",
            };

            var detailedDonorDTO = new DetailedDonorDTO
            {
                AaAccount = "aaAccount",
                UID = "0931qnt08m",
                Email = "donor@test.io",
                DeviceAddress = "127.0.0.123",
                WalletAddress = "127.0.0.1"
            };

            userrepository.Setup(s => s.Authenticate(dto.Email, dto.Password)).ReturnsAsync((UserAuthStatus.NO_USER, null, null));

            donorrepository.Setup(s => s.AuthenticateAsync(dto.Email, dto.Password)).ReturnsAsync((UserAuthStatus.SUCCESS, detailedDonorDTO, token));

            var result = await controller.Authenticate(dto);
            var objectresult = result as OkObjectResult;
            var resultobject = objectresult.Value as DonorTokenDTO;

            Assert.Equal("verysecrettoken", resultobject.Token);
            Assert.Equal(detailedDonorDTO.AaAccount, resultobject.DTO.AaAccount);
            Assert.Equal(detailedDonorDTO.UID, resultobject.DTO.UID);
            Assert.Equal(detailedDonorDTO.Email, resultobject.DTO.Email);
            Assert.Equal(detailedDonorDTO.DeviceAddress, resultobject.DTO.DeviceAddress);
            Assert.Equal(detailedDonorDTO.WalletAddress, resultobject.DTO.WalletAddress);
        }

        [Fact]
        public async Task Authenticate_given_wrong_Password_match_Returns_BadRequest_and_error_message()
        {
            var id = 1;

            var user = new User
            {
                Email = "test@Test",
                Password = "1234",
            };
            var dto = new AuthenticateDTO
            {
                Email = user.Email,
                Password = "wrongpassword",
            };

            var userDTO = new DetailedUserDTO
            {
                UserId = id,
                Email = dto.Email,
                UserRole = UserRoleEnum.Receiver.ToString(),
                FirstName = "test",
                SurName = "test"
            };

            var responseText = "Username or password is incorrect";

            userrepository.Setup(s => s.Authenticate(user.Email, dto.Password)).ReturnsAsync((UserAuthStatus.NO_USER, null, null));

            donorrepository.Setup(s => s.AuthenticateAsync(user.Email, dto.Password)).ReturnsAsync((UserAuthStatus.WRONG_PASSWORD, null, null));

            var result = await controller.Authenticate(dto);
            var objectresult = result as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(responseText, objectresult.Value);
        }
        [Fact]
        public async Task Authenticate_given_wrong_Email_match_Returns_BadRequest_and_error_message()
        {
            var id = 1;

            var user = new User
            {
                Email = "test@Test",
                Password = "1234",
            };
            var dto = new AuthenticateDTO
            {
                Email = "wrong@Test",
                Password = user.Password
            };

            var userDTO = new DetailedUserDTO
            {
                UserId = id,
                Email = dto.Email,
                UserRole = UserRoleEnum.Receiver.ToString(),
                FirstName = "test",
                SurName = "test"
            };

            var responseText = "Username or password is incorrect";

            userrepository.Setup(s => s.Authenticate(dto.Email, dto.Password)).ReturnsAsync((UserAuthStatus.NO_USER, null, null));

            donorrepository.Setup(s => s.AuthenticateAsync(dto.Email, dto.Password)).ReturnsAsync((UserAuthStatus.NO_USER, null, null));

            var result = await controller.Authenticate(dto);
            var objectresult = result as BadRequestObjectResult;
            var resultobject = objectresult.Value as string;

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(responseText, resultobject);
        }

        [Fact]
        public async Task Post_given_Role_Receiver_Creates_and_returns_Receiver()
        {
            var id = 1;
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@Test",
                Password = "1234",
                UserRole = UserRoleEnum.Receiver.ToString(),
            };

            var expected = (UserCreateStatus.SUCCESS, new TokenDTO {
                UserDTO = new DetailedUserDTO
                {
                    UserId = id,
                    UserRole = dto.UserRole
                },
            });

            userrepository.Setup(s => s.CreateAsync(It.IsAny<UserCreateDTO>())).ReturnsAsync(expected);

            var post = await controller.Post(dto);
            var result = post as CreatedAtActionResult;
            var resultValue = result.Value as TokenDTO;

            userrepository.Verify(s => s.CreateAsync(dto));

            Assert.Equal("Get", result.ActionName);
            Assert.Equal(id, result.RouteValues["id"]);
            Assert.Equal(dto.UserRole, resultValue.UserDTO.UserRole);
            Assert.Equal(id, resultValue.UserDTO.UserId);
        }

        [Fact]
        public async Task Post_given_Role_Producer_creates_and_returns_Producer()
        {
            var id = 1;
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@Test",
                Password = "1234",
                UserRole = UserRoleEnum.Producer.ToString(),

            };

            var expected = (UserCreateStatus.SUCCESS, new TokenDTO
            {
                UserDTO = new DetailedUserDTO
                {
                    UserId = id,
                    UserRole = dto.UserRole
                }
            });

            userrepository.Setup(s => s.CreateAsync(It.IsAny<UserCreateDTO>())).ReturnsAsync(expected);

            var post = await controller.Post(dto);
            var result = post as CreatedAtActionResult;
            var resultValue = result.Value as TokenDTO;

            userrepository.Verify(s => s.CreateAsync(dto));

            Assert.Equal("Get", result.ActionName);
            Assert.Equal(id, result.RouteValues["id"]);
            Assert.Equal(dto.UserRole, resultValue.UserDTO.UserRole);
            Assert.Equal(id, resultValue.UserDTO.UserId);
        }

        [Fact]
        public async Task Post_given_no_Role_returns_BadRequest_with_error_message()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@Test",
                Password = "1234",
            };

            var responseText = "Users must have assigned a valid role";

            userrepository.Setup(r => r.CreateAsync(dto)).ReturnsAsync((UserCreateStatus.INVALID_ROLE, new TokenDTO()));

            var post = await controller.Post(dto);
            var result = post as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(responseText, result.Value);
        }

        [Fact]
        public async Task Post_given_invalid_Role_returns_BadRequest_with_error_message()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@Test",
                Password = "1234",
                UserRole = "test"
            };

            var responseText = "Users must have assigned a valid role";

            userrepository.Setup(r => r.CreateAsync(dto)).ReturnsAsync((UserCreateStatus.INVALID_ROLE, new TokenDTO()));

            var post = await controller.Post(dto);
            var result = post as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(responseText, result.Value);
        }

        [Fact]
        public async Task Post_given_existing_user_returns_Conflict()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "test@Test",
                Password = "1234",
                UserRole = UserRoleEnum.Producer.ToString()
            };

            userrepository.Setup(r => r.CreateAsync(dto)).ReturnsAsync((UserCreateStatus.EMAIL_TAKEN, new TokenDTO()));

            var post = await controller.Post(dto);
            var result = post as ConflictObjectResult;

            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("This Email is already registered", result.Value);
        }

        [Fact]
        public async Task Post_given_empty_email_returns_BadRequest()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "Test",
                SurName = "Test",
                Email = "",
                Password = "1234",
                UserRole = UserRoleEnum.Producer.ToString()
            };

            userrepository.Setup(r => r.CreateAsync(dto)).ReturnsAsync((UserCreateStatus.MISSING_EMAIL, new TokenDTO()));

            var post = await controller.Post(dto);
            var result = post as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Post_empty_dto_returns_BadRequest()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "",
                SurName = "",
                Email = "",
                Password = "",
                UserRole = UserRoleEnum.Producer.ToString()
            };

            userrepository.Setup(r => r.CreateAsync(dto)).ReturnsAsync((UserCreateStatus.MISSING_NAME, new TokenDTO()));

            var post = await controller.Post(dto);
            var result = post as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Get_given_existing_id_returns_User()
        {
            var input = 1;

            var expected = new DetailedUserDTO
            {
                UserId = input,
                FirstName = "Test",
                SurName = "Test",
                Country = "Test",
                Thumbnail = "test.png",
                Description = "test",
                UserRole = UserRoleEnum.Producer.ToString()
            };

            userrepository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            var get = await controller.Get(input);

            Assert.Equal(expected.UserId, get.Value.Id);
            Assert.Equal(expected.FirstName, get.Value.FirstName);
            Assert.Equal(expected.SurName, get.Value.SurName);
            Assert.Equal(expected.Country, get.Value.Country);
            Assert.Equal(expected.Description, get.Value.Description);
            Assert.Equal(expected.UserRole, get.Value.UserRole);
            Assert.Equal(expected.Thumbnail, get.Value.Thumbnail);
        }

        [Fact]
        public async Task Get_given_existing_id_with_no_thumbnail_returns_User_empty_thumbnail()
        {
            var input = 1;

            var expected = new DetailedUserDTO
            {
                UserId = input,
                FirstName = "Test",
                SurName = "Test",
                Thumbnail = "",
                UserRole = UserRoleEnum.Producer.ToString()
            };

            userrepository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            var get = await controller.Get(input);

            Assert.Equal(expected.UserId, get.Value.Id);
            Assert.Equal(expected.FirstName, get.Value.FirstName);
            Assert.Equal(expected.SurName, get.Value.SurName);
            Assert.Equal(expected.UserRole, get.Value.UserRole);
            Assert.Empty(get.Value.Thumbnail);
        }

        [Fact]
        public async Task Get_given_existing_id_and_role_receiver_returns_receiver()
        {
            var input = 1;

            var expected = new DetailedProducerDTO
            {
                FirstName = "test",
                UserRole = UserRoleEnum.Receiver.ToString()
            };

            userrepository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            var get = await controller.Get(input);

            Assert.Equal(expected.UserRole, get.Value.UserRole);
            Assert.Equal(expected.FirstName, get.Value.FirstName);
        }

        [Fact]
        public async Task Get_given_existing_id_and_user_role_Producer_returns_Producer()
        {
            var expected = new DetailedProducerDTO
            {
                UserId = 1,
                FirstName = "Test",
                UserRole = UserRoleEnum.Producer.ToString(),
                Street = "Testvej",
                StreetNumber = "12324",
                Zipcode = "2457"
            };

            userrepository.Setup(s => s.FindAsync(expected.UserId)).ReturnsAsync(expected);

            var get = await controller.Get(expected.UserId);

            Assert.Equal(expected.FirstName, get.Value.FirstName);
            Assert.Equal(expected.UserRole, get.Value.UserRole);
            Assert.Equal(expected.Street, get.Value.Street);
        }

        [Fact]
        public async Task Get_given_non_existing_id_returns_NotFound()
        {
            var input = 1;

            var get = await controller.Get(input);

            Assert.IsType<NotFoundResult>(get.Result);
        }

        [Fact]
        public async Task GetProducerCount_given_one_producer_returns_one()
        {
            userrepository.Setup(s => s.GetCountProducersAsync()).ReturnsAsync(1);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var get = await controller.GetProducerCount();

            Assert.Equal(1, get.Value);
        }

        [Fact]
        public async Task GetProducerCount_given_none_producer_returns_zero()
        {
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var get = await controller.GetProducerCount();

            Assert.Equal(0, get.Value);
        }

        [Fact]
        public async Task GetProducerCount_given_Request_on_open_access_port_returns_Forbidden()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Request.Host = new HostString("localhost:");
            httpContext.Connection.RemoteIpAddress = new IPAddress(3812831);
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.GetProducerCount();

            Assert.IsType<ForbidResult>(get.Result);
        }

        [Fact]
        public async Task GetProducerCount_given_Request_on_open_access_port_from_localhost_returns_Forbidden()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.GetProducerCount();

            Assert.IsType<ForbidResult>(get.Result);
        }

        [Fact]
        public async Task GetProducerCount_given_Request_on_local_access_port_from_localhost_returns_Count()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 4001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.GetProducerCount();

            Assert.Equal(0, get.Value);
        }

        [Fact]
        public async Task GetReceiverCount_given_one_receiver_returns_one()
        {
            userrepository.Setup(s => s.GetCountReceiversAsync()).ReturnsAsync(1);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var get = await controller.GetReceiverCount();

            Assert.Equal(1, get.Value);
        }

        [Fact]
        public async Task Get_Receiver_Count_given_none_producer_returns_zero()
        {
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var get = await controller.GetReceiverCount();

            Assert.Equal(0, get.Value);
        }

        [Fact]
        public async Task GetReceiverCount_given_Request_on_open_access_port_returns_Forbidden()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Request.Host = new HostString("localhost:");
            httpContext.Connection.RemoteIpAddress = new IPAddress(3812831);
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.GetReceiverCount();

            Assert.IsType<ForbidResult>(get.Result);
        }

        [Fact]
        public async Task GetReceiverCount_given_Request_on_open_access_port_from_localhost_returns_Forbidden()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.GetReceiverCount();

            Assert.IsType<ForbidResult>(get.Result);
        }

        [Fact]
        public async Task GetReceiverCount_given_Request_on_local_access_port_from_localhost_returns_Count()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 4001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.GetReceiverCount();

            Assert.Equal(0, get.Value);
        }

        [Fact]
        public async Task Me_given_existing_id_returns_User()
        {
            var input = 1;

            var expected = new DetailedUserDTO
            {
                UserId = input,
                Email = "test@Test",
                FirstName = "Test",
                UserRole = UserRoleEnum.Producer.ToString(),
                Thumbnail = "test.png"
            };

            userrepository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(input);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var get = await controller.Me();

            Assert.IsType<OkObjectResult>(get);

            var objectResult = get as OkObjectResult;
            var user = objectResult.Value as DetailedUserDTO;

            Assert.Equal(expected.UserId, user.UserId);
            Assert.Equal(expected.Email, user.Email);
            Assert.Equal(expected.FirstName, user.FirstName);
            Assert.Equal(expected.UserRole, user.UserRole);
            Assert.Equal(expected.Thumbnail, user.Thumbnail);
        }

        [Fact]
        public async Task Me_given_existing_id_with_no_thumbnail_returns_User_no_thumbnail()
        {
            var input = 1;

            var expected = new DetailedUserDTO
            {
                UserId = input,
                Email = "test@Test",
                FirstName = "Test",
                UserRole = UserRoleEnum.Producer.ToString(),
                Thumbnail = ""
            };

            userrepository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(input);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var get = await controller.Me();

            Assert.IsType<OkObjectResult>(get);

            var objectResult = get as OkObjectResult;
            var user = objectResult.Value as DetailedUserDTO;

            Assert.Equal(expected.UserId, user.UserId);
            Assert.Equal(expected.Email, user.Email);
            Assert.Equal(expected.FirstName, user.FirstName);
            Assert.Equal(expected.UserRole, user.UserRole);
            Assert.Empty(user.Thumbnail);
        }

        [Fact]
        public async Task Me_given_existing_id_and_role_receiver_returns_receiver()
        {
            var input = 1;

            var expected = new DetailedReceiverDTO
            {
                UserId = input,
                Email = "Test@test",
                FirstName = "test",
                UserRole = UserRoleEnum.Receiver.ToString()
            };

            userrepository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(input);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var get = await controller.Me();

            Assert.IsType<OkObjectResult>(get);

            var objectResult = get as OkObjectResult;
            var user = objectResult.Value as DetailedUserDTO;

            Assert.Equal(expected.UserId, user.UserId);
            Assert.Equal(expected.Email, user.Email);
            Assert.Equal(expected.UserRole, user.UserRole);
            Assert.Equal(expected.FirstName, user.FirstName);
        }

        [Fact]
        public async Task Me_given_existing_id_and_role_receiver_returns_producer()
        {
            var input = 1;

            var expected = new DetailedProducerDTO
            {
                UserId = input,
                Email = "Test@test",
                FirstName = "Test",
                UserRole = UserRoleEnum.Producer.ToString(),
            };

            userrepository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(input);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var get = await controller.Me();
            Assert.IsType<OkObjectResult>(get);

            var objectResult = get as OkObjectResult;
            var user = objectResult.Value as DetailedUserDTO;

            Assert.Equal(expected.UserId, user.UserId);
            Assert.Equal(expected.Email, user.Email);
            Assert.Equal(expected.FirstName, user.FirstName);
            Assert.Equal(expected.UserRole, user.UserRole);
        }

        [Fact]
        public async Task Me_given_non_existing_id_returns_NotFound()
        {
            var input = 1;

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(input);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var get = await controller.Me();

            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task Me_given_exsiting_donor_id_returns_correct_donor()
        {
            var input = "test";

            var detailedDonor = new DetailedDonorDTO
            {
                AaAccount = input,
                UID = "test",
                Email = "test@test.com",
                DeviceAddress = "test-link",
                WalletAddress = "test-address",
                UserRole = "Donor"
            };
            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //Create ClaimIdentity
            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.NameIdentifier, input),
            };
            var identity = new ClaimsIdentity(claims);

            donorrepository.Setup(s => s.ReadAsync(input)).ReturnsAsync(detailedDonor);

            //Mock claim to make the HttpContext contain one.
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(true);

            claimsPrincipalMock.Setup(m => m.Claims).Returns(claims);
            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = claimsPrincipalMock.Object;

            var get = await controller.Me();

            Assert.IsType<OkObjectResult>(get);

            var objectResult = get as OkObjectResult;
            var donor = objectResult.Value as DetailedDonorDTO;

            Assert.Equal(input, donor.AaAccount);
            Assert.Equal(detailedDonor.UID, donor.UID);
            Assert.Equal(detailedDonor.Email, donor.Email);
            Assert.Equal(detailedDonor.WalletAddress, donor.WalletAddress);
            Assert.Equal(detailedDonor.DeviceAddress, donor.DeviceAddress);
            Assert.Equal("Donor", donor.UserRole);
        }

        [Fact]
        public async Task Me_given_non_existing_donor_id_returns_NotFound()
        {
            var input = "non_existing";

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //Create ClaimIdentity
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, input),
            };
            var identity = new ClaimsIdentity(claims);

            donorrepository.Setup(s => s.ReadAsync(input)).ReturnsAsync((DetailedDonorDTO) null);

            //Mock claim to make the HttpContext contain one.
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            claimsPrincipalMock.Setup(m => m.Claims).Returns(claims);
            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = claimsPrincipalMock.Object;

            var get = await controller.Me();

            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task Put_given_User_id_same_as_claim_calls_update()
        {
            var dto = new UserUpdateDTO
            {
                UserId = 1,
                FirstName = "test",
            };

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(dto.UserId);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            await controller.Put(dto);

            userrepository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task Put_given_different_User_id_as_claim_returns_Forbidden()
        {
            var dto = new UserUpdateDTO
            {
                UserId = 1,
                FirstName = "test",
            };

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.Put(dto);

            Assert.IsType<ForbidResult>(put);
        }

        [Fact]
        public async Task Put_given_non_existing_id_returns_NotFound()
        {
            var dto = new UserUpdateDTO
            {
                UserId = 1,
                FirstName = "test",
                NewPassword = "1234"
            };

            userrepository.Setup(m => m.UpdateAsync(dto)).ReturnsAsync(false);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(dto.UserId);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.Put(dto);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task Put_given_valid_dto_returns_NoContent()
        {
            var dto = new UserUpdateDTO
            {
                UserId = 1,
                FirstName = "test",
            };

            userrepository.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(true);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(dto.UserId);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            await controller.Put(dto);

            userrepository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task PutDeviceAddress_given_Existing_secret_returns_NoContent()
        {
            var dto = new UserPairingDTO
            {
                PairingSecret = "ABCD",
                DeviceAddress = "Test"
            };

            userrepository.Setup(r => r.UpdateDeviceAddressAsync(dto)).ReturnsAsync(true);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = await controller.PutDeviceAddress(dto);

            userrepository.Verify(s => s.UpdateDeviceAddressAsync(dto));
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task PutDeviceAddress_given_nonExisting_secret_returns_NotFound()
        {

            var dto = new UserPairingDTO
            {
                PairingSecret = "ABCD",
                DeviceAddress = "Test"
            };

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = await controller.PutDeviceAddress(dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PutDeviceAddress_given_Request_on_open_access_port_returns_Forbidden()
        {
            var dto = new UserPairingDTO
            {
                PairingSecret = "ABCD",
                DeviceAddress = "Test"
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Request.Host = new HostString("localhost:");
            httpContext.Connection.RemoteIpAddress = new IPAddress(3812831);
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.PutDeviceAddress(dto);

            Assert.IsType<ForbidResult>(get);
        }

        [Fact]
        public async Task PutDeviceAddress_given_Request_on_open_access_port_from_localhost_returns_Forbidden()
        {
            var dto = new UserPairingDTO
            {
                PairingSecret = "ABCD",
                DeviceAddress = "Test"
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.PutDeviceAddress(dto);

            Assert.IsType<ForbidResult>(get);
        }

        [Fact]
        public async Task PutDeviceAddress_given_Existing_secret_and_Request_on_local_access_port_from_localhost_returns_NoContent()
        {
            var dto = new UserPairingDTO
            {
                PairingSecret = "ABCD",
                DeviceAddress = "Test"
            };

            userrepository.Setup(r => r.UpdateDeviceAddressAsync(dto)).ReturnsAsync(true);

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 4001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var get = await controller.PutDeviceAddress(dto);

            Assert.IsType<NoContentResult>(get);
        }

        [Fact]
        public async Task PutImage_given_valid_id_and_image_returns_relative_path_to_file()
        {
            var id = 1;
            var idString = "1";
            var formFile = new Mock<IFormFile>();
            var fileName = "file.png";

            var userImageFormDTO = new UserImageFormDTO
            {
                UserId = idString,
                File = formFile.Object
            };

            userrepository.Setup(r => r.UpdateImageAsync(id, It.IsAny<IFormFile>())).ReturnsAsync(fileName);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var putImage = await controller.PutImage(userImageFormDTO);

            Assert.Equal(fileName, putImage.Value);
        }

        [Fact]
        public async Task PutImage_given_different_User_id_as_claim_returns_Forbidden()
        {
            var formFile = new Mock<IFormFile>();
            var idString = "1";

            var userImageFormDTO = new UserImageFormDTO
            {
                UserId = idString,
                File = formFile.Object
            };

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.PutImage(userImageFormDTO);

            Assert.IsType<ForbidResult>(put.Result);
        }

        [Fact]
        public async Task PutImage_given_non_existing_user_and_valid_claim_returns_NotFoundObjectResult_and_message()
        {
            var formFile = new Mock<IFormFile>();
            var idString = "1";
            var id = 1;
            var error = "User not found";

            var userImageFormDTO = new UserImageFormDTO
            {
                UserId = idString,
                File = formFile.Object
            };

            userrepository.Setup(r => r.UpdateImageAsync(id, It.IsAny<IFormFile>())).ReturnsAsync(default(string));

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.PutImage(userImageFormDTO);
            var notFound = put.Result as NotFoundObjectResult;

            Assert.IsType<NotFoundObjectResult>(put.Result);
            Assert.Equal(error, notFound.Value);
        }

        [Fact]
        public async Task PutImage_given_wrong_id_format_returns_BadRequest()
        {
            var formFile = new Mock<IFormFile>();
            var idString = "test";

            var userImageFormDTO = new UserImageFormDTO
            {
                UserId = idString,
                File = formFile.Object
            };

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //Create ClaimIdentity
            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.NameIdentifier, idString),
            };
            var identity = new ClaimsIdentity(claims);

            //Mock claim to make the HttpContext contain one.
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(true);

            claimsPrincipalMock.Setup(m => m.Claims).Returns(claims);
            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = claimsPrincipalMock.Object;

            var putImage = await controller.PutImage(userImageFormDTO);

            Assert.IsType<BadRequestResult>(putImage.Result);
        }

        [Fact]
        public async Task PutImage_given_invalid_image_returns_BadRequestObjectResult()
        {
            var id = 1;
            var idString = "1";
            var formFile = new Mock<IFormFile>();

            var userImageFormDTO = new UserImageFormDTO
            {
                UserId = idString,
                File = formFile.Object
            };

            userrepository.Setup(r => r.UpdateImageAsync(id, It.IsAny<IFormFile>())).ThrowsAsync(new ArgumentException("Invalid image file"));

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var putImage = await controller.PutImage(userImageFormDTO);

            Assert.IsType<BadRequestObjectResult>(putImage.Result);
        }

        [Fact]
        public async Task PutImage_given_invalid_image_returns_InternalServerError()
        {
            var id = 1;
            var idString = "1";
            var formFile = new Mock<IFormFile>();

            var userImageFormDTO = new UserImageFormDTO
            {
                UserId = idString,
                File = formFile.Object
            };

            userrepository.Setup(r => r.UpdateImageAsync(id, It.IsAny<IFormFile>())).ThrowsAsync(new ArgumentException());

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var putImage = await controller.PutImage(userImageFormDTO);
            var image = putImage.Result as StatusCodeResult;

            Assert.IsType<StatusCodeResult>(putImage.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, image.StatusCode);
        }
    }
}
