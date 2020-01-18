using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
using PolloPollo.Shared;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using MockQueryable.Moq;
using Microsoft.AspNetCore.Authorization;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;
using System.Net;
using Microsoft.Extensions.Logging;

namespace PolloPollo.Web.Controllers.Tests
{
    public class ApplicationsControllerTests
    {
        private Mock<ClaimsPrincipal> MockClaimsSecurity(int id, string role)
        {
            //Create Claims
            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.NameIdentifier, id.ToString()),
               new Claim(ClaimTypes.Role, role),
            };

            //Mock claim to make the HttpContext contain one.
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(true);

            claimsPrincipalMock.Setup(m => m.Claims).Returns(claims);

            return claimsPrincipalMock;
        }

        [Fact]
        public void ApplicationsController_has_AuthroizeAttribute()
        {
            var controller = typeof(ApplicationsController);

            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public async Task Post_given_valid_DTO_creates_and_returns_ApplicationDTO()
        {
            var id = 1;
            var dto = new ApplicationCreateDTO
            {
                UserId = 1,
                Motivation = "I need this product",
            };

            var expected = new ApplicationDTO
            {
                ApplicationId = id,
                Motivation = "I need this product",
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.CreateAsync(It.IsAny<ApplicationCreateDTO>())).ReturnsAsync(expected);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();

            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(dto);

            var result = post.Result as CreatedAtActionResult;
            var resultValue = result.Value as ApplicationDTO;

            applicationRepository.Verify(s => s.CreateAsync(dto));

            Assert.Equal("Get", result.ActionName);
            Assert.Equal(expected.ApplicationId, result.RouteValues["id"]);
            Assert.Equal(expected.ApplicationId, resultValue.ApplicationId);
        }

        [Fact]
        public async Task Post_given_invalid_User_Role_returns_Unauthorized()
        {
            var dto = new ApplicationCreateDTO();

            var userRole = UserRoleEnum.Producer.ToString();

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, userRole);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(dto);

            Assert.IsType<UnauthorizedResult>(post.Result);
        }

        [Fact]
        public async Task Post_given_existing_application_returns_Conflict()
        {

            var dto = new ApplicationCreateDTO
            {
                UserId = 1,
                ProductId = 42,
                Motivation = "I need this product",
            };

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(dto);

            Assert.IsType<ConflictResult>(post.Result);
        }

        [Fact]
        public async Task Post_given_null_returns_Conflict()
        {

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(null);

            Assert.IsType<ConflictResult>(post.Result);
        }

        [Fact]
        public async Task Get_given_existing_id_returns_application()
        {
            var id = 1;
            var expected = new ApplicationDTO
            {
                ApplicationId = id,
                Motivation = "I need this product",
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(id)).ReturnsAsync(expected);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.Get(id);

            Assert.Equal(expected.ApplicationId, get.Value.ApplicationId);
        }

        [Fact]
        public async Task Get_given_non_existing_id_returns_NotFound()
        {
            var id = 1;

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.Get(id);

            Assert.IsType<NotFoundResult>(get.Result);
        }

        [Fact]
        public async Task Get_given_offset_default_int_and_offset_default_int_returns_all_dtos()
        {
            var dto = new ApplicationDTO();
            var dtos = new[] { dto }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.ReadOpen()).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.Get(0, 0);
            var value = get.Value as ApplicationListDTO;

            Assert.Equal(dto, value.List.First());
            Assert.Equal(1, value.Count);
        }

        [Fact]
        public async Task Get_given_offset_0_amount_1_returns_1_dto()
        {
            var dto = new ApplicationDTO { ApplicationId = 1 };
            var dto1 = new ApplicationDTO { ApplicationId = 2 };
            var dtos = new[] { dto, dto1 }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.ReadOpen()).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.Get(0, 1);
            var value = get.Value as ApplicationListDTO;

            Assert.Equal(dto, value.List.First());
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task Get_given_offset_1_amount_2_returns_2_last_dto()
        {
            var dto = new ApplicationDTO { ApplicationId = 1 };
            var dto1 = new ApplicationDTO { ApplicationId = 2 };
            var dto2 = new ApplicationDTO { ApplicationId = 3 };
            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.ReadOpen()).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.Get(1, 2);
            var value = get.Value as ApplicationListDTO;

            Assert.Equal(dto1.ApplicationId, value.List.ElementAt(0).ApplicationId);
            Assert.Equal(dto2.ApplicationId, value.List.ElementAt(1).ApplicationId);
            Assert.Equal(3, value.Count);
        }

        [Fact]
        public async Task Get_given_offset_2_amount_2_returns_last_dto()
        {
            var dto = new ApplicationDTO { ApplicationId = 1 };
            var dto1 = new ApplicationDTO { ApplicationId = 2 };
            var dto2 = new ApplicationDTO { ApplicationId = 3 };
            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.ReadOpen()).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.Get(2, 2);
            var value = get.Value as ApplicationListDTO;

            Assert.Equal(dto2.ApplicationId, value.List.ElementAt(0).ApplicationId);
            Assert.Equal(3, value.Count);
        }

        [Fact]
        public async Task GetByReceiver_given_valid_id_returns_all_dtos()
        {
            var input = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Status = ApplicationStatusEnum.Pending
            };
            var dto1 = new ApplicationDTO
            {
                ApplicationId = 2,
                Status = ApplicationStatusEnum.Open
            };
            var dto2 = new ApplicationDTO
            {
                ApplicationId = 3,
                Status = ApplicationStatusEnum.Unavailable
            };

            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.GetByReceiver(input);

            Assert.Equal(dto.ApplicationId, get.Value.ElementAt(0).ApplicationId);
            Assert.Equal(dto.Status, get.Value.ElementAt(0).Status);
            Assert.Equal(dto1.ApplicationId, get.Value.ElementAt(1).ApplicationId);
            Assert.Equal(dto1.Status, get.Value.ElementAt(1).Status);
            Assert.Equal(dto2.ApplicationId, get.Value.ElementAt(2).ApplicationId);
            Assert.Equal(dto2.Status, get.Value.ElementAt(2).Status);
        }

        [Fact]
        public async Task GetByReceiver_given_valid_id_and_status_all_returns_all_dtos()
        {
            var input = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Status = ApplicationStatusEnum.Pending
            };
            var dto1 = new ApplicationDTO
            {
                ApplicationId = 2,
                Status = ApplicationStatusEnum.Open
            };
            var dto2 = new ApplicationDTO
            {
                ApplicationId = 3,
                Status = ApplicationStatusEnum.Unavailable
            };

            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.GetByReceiver(input, ApplicationStatusEnum.All.ToString());

            Assert.Equal(dto.ApplicationId, get.Value.ElementAt(0).ApplicationId);
            Assert.Equal(dto.Status, get.Value.ElementAt(0).Status);
            Assert.Equal(dto1.ApplicationId, get.Value.ElementAt(1).ApplicationId);
            Assert.Equal(dto1.Status, get.Value.ElementAt(1).Status);
            Assert.Equal(dto2.ApplicationId, get.Value.ElementAt(2).ApplicationId);
            Assert.Equal(dto2.Status, get.Value.ElementAt(2).Status);
        }

        [Fact]
        public async Task GetByReceiver_given_valid_id_and_status_closed_returns_closed_dtos()
        {
            var input = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Status = ApplicationStatusEnum.Pending
            };
            var dto1 = new ApplicationDTO
            {
                ApplicationId = 2,
                Status = ApplicationStatusEnum.Open
            };
            var dto2 = new ApplicationDTO
            {
                ApplicationId = 3,
                Status = ApplicationStatusEnum.Unavailable
            };

            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.GetByReceiver(input, ApplicationStatusEnum.Unavailable.ToString());

            Assert.Equal(dto2.ApplicationId, get.Value.ElementAt(0).ApplicationId);
            Assert.Equal(dto2.Status, get.Value.ElementAt(0).Status);
        }

        [Fact]
        public async Task GetByReceiver_given_valid_id_and_status_open_returns_open_dtos()
        {
            var input = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Status = ApplicationStatusEnum.Pending
            };
            var dto1 = new ApplicationDTO
            {
                ApplicationId = 2,
                Status = ApplicationStatusEnum.Open
            };
            var dto2 = new ApplicationDTO
            {
                ApplicationId = 3,
                Status = ApplicationStatusEnum.Unavailable
            };

            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.GetByReceiver(input, ApplicationStatusEnum.Open.ToString());

            Assert.Equal(dto1.ApplicationId, get.Value.ElementAt(0).ApplicationId);
            Assert.Equal(dto1.Status, get.Value.ElementAt(0).Status);
        }

        [Fact]
        public async Task GetByReceiver_given_valid_id_and_status_pending_returns_pending_dtos()
        {
            var input = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Status = ApplicationStatusEnum.Pending
            };
            var dto1 = new ApplicationDTO
            {
                ApplicationId = 2,
                Status = ApplicationStatusEnum.Open
            };
            var dto2 = new ApplicationDTO
            {
                ApplicationId = 3,
                Status = ApplicationStatusEnum.Unavailable
            };

            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.GetByReceiver(input, ApplicationStatusEnum.Pending.ToString());

            Assert.Equal(dto.ApplicationId, get.Value.ElementAt(0).ApplicationId);
            Assert.Equal(dto.Status, get.Value.ElementAt(0).Status);
        }

        [Fact]
        public async Task GetByReceiver_given_non_existing_id_returns_EmptyList()
        {
            var input = 1;

            var dtos = new List<ApplicationDTO>().AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.GetByReceiver(input);

            Assert.Equal(new List<ApplicationDTO>(), get.Value);
        }

        [Fact]
        public async Task GetByReceiver_given_valid_id_invalid_status_returns_empty_IEnumerable()
        {
            var input = 1;

            var dto = new ApplicationDTO();
            var dtos = new[] { dto }.AsQueryable().BuildMock();
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var get = await controller.GetByReceiver(input, "test");

            Assert.Empty(get.Value);
        }

        [Fact]
        public async Task Delete_given_non_existing_applicationId_returns_false()
        {
            var userId = 42;
            var nonexistingApplicationId = 12;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
                Status = ApplicationStatusEnum.Open,
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.DeleteAsync(userId, nonexistingApplicationId)).ReturnsAsync(false);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(userId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var delete = await controller.Delete(userId, nonexistingApplicationId);

            Assert.False(delete.Value);
        }

        [Fact]
        public async Task Delete_given_existing_applicationId_wrong_userId_returns_forbidden()
        {
            var found = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
            };

            var wrongUserId = 41;

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(found.ApplicationId)).ReturnsAsync(found);
            applicationRepository.Setup(s => s.DeleteAsync(found.ReceiverId, found.ApplicationId)).ReturnsAsync(true);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(found.ReceiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var delete = await controller.Delete(wrongUserId, found.ApplicationId);

            Assert.IsType<ForbidResult>(delete.Result);
        }

        [Fact]
        public async Task Delete_given_existing_applicationId_wrong_Role_returns_Unauthorized()
        {
            var found = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
            };

            var userId = 15;
            var userRole = UserRoleEnum.Producer.ToString();

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(userId, userRole);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var delete = await controller.Delete(userId, found.ApplicationId);

            Assert.IsType<UnauthorizedResult>(delete.Result);
        }

        [Fact]
        public async Task Delete_given_valid_ids_deletes_and_returns_true()
        {
            var found = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(found.ApplicationId)).ReturnsAsync(found);
            applicationRepository.Setup(s => s.DeleteAsync(found.ReceiverId, found.ApplicationId)).ReturnsAsync(true);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(found.ReceiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var delete = await controller.Delete(found.ReceiverId, found.ApplicationId);

            Assert.True(delete.Value);
        }

        [Fact]
        public async Task Delete_given_not_open_applications_returns_UnprocessableEntity()
        {
            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
                Status = ApplicationStatusEnum.Pending,
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(dto.ApplicationId)).ReturnsAsync(dto);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(dto.ReceiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.Delete(dto.ReceiverId, dto.ApplicationId);
            var statusCode = result.Result as StatusCodeResult;

            Assert.Equal(StatusCodes.Status422UnprocessableEntity, statusCode.StatusCode);
        }

        [Fact]
        public async Task Delete_given_not_existing_applications_returns_NotFound()
        {
            var applicationId = 42;
            var receiverId = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
                Status = ApplicationStatusEnum.Pending,
                ReceiverId = 1
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(dto.ApplicationId)).ReturnsAsync(dto);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(receiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.Delete(receiverId, applicationId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]

        public async Task Put_given_existing_dto_calls_update_successfully()
        {
            var dto = new ApplicationUpdateDTO
            {
                ReceiverId = 1,
                ApplicationId = 1, 
                Status = ApplicationStatusEnum.Pending
            };

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            await controller.Put(dto);

            applicationRepository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task Put_given_non_existing_returns_false_returns_NotFound()
        {
            var dto = new ApplicationUpdateDTO
            {
                ReceiverId = 1,
                ApplicationId = 1, 
                Status = ApplicationStatusEnum.Locked
            };

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(dto);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task Put_given_Request_on_open_access_port_from_localhost_returns_Forbidden()
        {
            var dto = new ApplicationUpdateDTO
            {
                Status = ApplicationStatusEnum.Pending
            };

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);
            // Needs HttpContext to mock it.
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var put = await controller.Put(dto);
            var result = put as ForbidResult;

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Put_given_Request_on_open_access_port_returns_Forbidden()
        {
            var dto = new ApplicationUpdateDTO
            {
                Status = ApplicationStatusEnum.Pending
            };

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Request.Host = new HostString("localhost:");
            httpContext.Connection.RemoteIpAddress = new IPAddress(3812831);
            controller.ControllerContext.HttpContext = httpContext;

            var put = await controller.Put(dto);
            var result = put as ForbidResult;

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Put_given_Request_on_local_access_port_from_localhost_returns_NoContent()
        {
            var dto = new ApplicationUpdateDTO
            {
                ReceiverId = 1,
                ApplicationId = 1,
                Status = ApplicationStatusEnum.Locked
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(a => a.UpdateAsync(dto)).ReturnsAsync((true, false));

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(dto);
            var result = put as NoContentResult;

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetContractInformation_given_existing_Id_returns_DTO()
        {
            var id = 5;

            var dto = new ContractInformationDTO
            {
                Price = 42,
                ProducerDevice = "ABCD",
                ProducerWallet = "EFGH"
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(r => r.GetContractInformationAsync(id)).ReturnsAsync(dto);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();

            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = await controller.GetContractInformation(id);

            applicationRepository.Verify(s => s.GetContractInformationAsync(id));

            Assert.Equal(dto.Price, result.Value.Price);
            Assert.Equal(dto.ProducerDevice, result.Value.ProducerDevice);
            Assert.Equal(dto.ProducerWallet, result.Value.ProducerWallet);
        }

        [Fact]
        public async Task GetContractInformation_given_nonExisting_Id_returns_NotFound()
        {
            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();

            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = await controller.GetContractInformation(5);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetContractInformation_given_Request_on_open_access_port_returns_Forbidden()
        {
            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();

            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Request.Host = new HostString("localhost:");
            httpContext.Connection.RemoteIpAddress = new IPAddress(3812831);
            controller.ControllerContext.HttpContext = httpContext;

            var result = await controller.GetContractInformation(5);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetContractInformation_given_Request_on_open_access_port_from_localhost_returns_Forbidden()
        {
            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();

            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var result = await controller.GetContractInformation(5);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetContactInformation_given_Existing_secret_and_Request_on_local_access_port_from_localhost_returns_DTO()
        {
            var id = 5;

            var dto = new ContractInformationDTO
            {
                Price = 42,
                ProducerDevice = "ABCD",
                ProducerWallet = "EFGH"
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(r => r.GetContractInformationAsync(id)).ReturnsAsync(dto);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();

            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 4001;
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            controller.ControllerContext.HttpContext = httpContext;

            var result = await controller.GetContractInformation(id);

            applicationRepository.Verify(s => s.GetContractInformationAsync(id));

            Assert.Equal(dto.Price, result.Value.Price);
            Assert.Equal(dto.ProducerDevice, result.Value.ProducerDevice);
            Assert.Equal(dto.ProducerWallet, result.Value.ProducerWallet);
        }

        [Fact]
        public async Task ConfirmReceival_given_invalid_applicationId_returns_NotFound()
        {
            var applicationId = 42;
            var receiverId = 42;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
                Status = ApplicationStatusEnum.Pending,
                ReceiverId = 1
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(dto.ApplicationId)).ReturnsAsync(dto);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(receiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.ConfirmReceival(receiverId, applicationId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task ConfirmReceival_given_userId_notmatching_applicationsReceiverId_returns_forbidden()
        {
            var applicationId = 1;
            var receiverId = 42;

            var applicationDTO = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
                Status = ApplicationStatusEnum.Pending,
                ReceiverId = 1
            };
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(applicationDTO.ApplicationId)).ReturnsAsync(applicationDTO);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(receiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.ConfirmReceival(receiverId, applicationId);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task ConfirmReceival_given_userId_invalid_role_returns_Unauthorized()
        {
            var applicationId = 1;
            var receiverId = 42;

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(receiverId, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.ConfirmReceival(receiverId, applicationId);

            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task ConfirmReceival_given_wrong_userId_returns_Forbidden()
        {
            var applicationId = 1;
            var receiverId = 1;

            var applicationRepository = new Mock<IApplicationRepository>();

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.ConfirmReceival(receiverId, applicationId);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task ConfirmReceival_given_nonpending_applicationsReceiverId_returns_UnprocessableEntity()
        {
            var applicationId = 1;
            var receiverId = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
                Status = ApplicationStatusEnum.Open,
                ReceiverId = 1
            };
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(dto.ApplicationId)).ReturnsAsync(dto);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(receiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.ConfirmReceival(receiverId, applicationId);

            var resultStatusCode = result.Result as StatusCodeResult;

            Assert.Equal(StatusCodes.Status422UnprocessableEntity, resultStatusCode.StatusCode);
        }

        [Fact]
        public async Task ConfirmReceival_given_allOK_updates_application_to_completed_and_returns_NoContent()
        {
            var applicationId = 1;
            var receiverId = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
                Status = ApplicationStatusEnum.Pending,
                ReceiverId = 1
            };

            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(dto.ApplicationId)).ReturnsAsync(dto);
            applicationRepository.Setup(s => s.UpdateAsync(It.IsAny<ApplicationUpdateDTO>())).ReturnsAsync((true, true));

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();
            walletRepository.Setup(s => s.ConfirmReceival(applicationId, null, null, null)).ReturnsAsync((true, HttpStatusCode.OK));

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(receiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.ConfirmReceival(receiverId, applicationId);

            applicationRepository.Verify(s => s.UpdateAsync(It.IsAny<ApplicationUpdateDTO>()));

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task ConfirmReceival_given_allOk_with_serverError_returns_InternalServerError()
        {
            var applicationId = 1;
            var receiverId = 1;

            var dto = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
                Status = ApplicationStatusEnum.Pending,
                ReceiverId = 1
            };
            var applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(s => s.FindAsync(dto.ApplicationId)).ReturnsAsync(dto);

            var productRepository = new Mock<IProductRepository>();

            var userRepository = new Mock<IUserRepository>();

            var walletRepository = new Mock<IWalletRepository>();
            walletRepository.Setup(s => s.ConfirmReceival(applicationId, null, null, null)).ReturnsAsync((false, HttpStatusCode.InternalServerError));

            var log = new Mock<ILogger<ApplicationsController>>();
            var controller = new ApplicationsController(applicationRepository.Object, productRepository.Object, userRepository.Object, walletRepository.Object, log.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(receiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var result = await controller.ConfirmReceival(receiverId, applicationId);

            var resultStatusCode = result.Result as StatusCodeResult;

            Assert.Equal(StatusCodes.Status500InternalServerError, resultStatusCode.StatusCode);
        }
    }
}
