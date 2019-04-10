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
using System;
using Microsoft.AspNetCore.Authorization;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;

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

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.CreateAsync(It.IsAny<ApplicationCreateDTO>())).ReturnsAsync(expected);

            var controller = new ApplicationsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(dto);

            var result = post.Result as CreatedAtActionResult;
            var resultValue = result.Value as ApplicationDTO;

            repository.Verify(s => s.CreateAsync(dto));

            Assert.Equal("Get", result.ActionName);
            Assert.Equal(expected.ApplicationId, result.RouteValues["id"]);
            Assert.Equal(expected.ApplicationId, resultValue.ApplicationId);
        }

        [Fact]
        public async Task Post_given_invalid_User_Role_returns_Unauthorized()
        {
            var dto = new ApplicationCreateDTO();

            var userRole = UserRoleEnum.Producer.ToString();

            var repository = new Mock<IApplicationRepository>();

            var controller = new ApplicationsController(repository.Object);

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

            var repository = new Mock<IApplicationRepository>();

            var controller = new ApplicationsController(repository.Object);

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

            var repository = new Mock<IApplicationRepository>();

            var controller = new ApplicationsController(repository.Object);

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

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.FindAsync(id)).ReturnsAsync(expected);

            var controller = new ApplicationsController(repository.Object);

            var get = await controller.Get(id);

            Assert.Equal(expected.ApplicationId, get.Value.ApplicationId);
        }

        [Fact]
        public async Task Get_given_non_existing_id_returns_NotFound()
        {
            var id = 1;

            var repository = new Mock<IApplicationRepository>();

            var controller = new ApplicationsController(repository.Object);

            var get = await controller.Get(id);

            Assert.IsType<NotFoundResult>(get.Result);
        }

        [Fact]
        public async Task Get_given_offset_default_int_and_offset_default_int_returns_all_dtos()
        {
            var dto = new ApplicationDTO();
            var dtos = new[] { dto }.AsQueryable().BuildMock();
            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.ReadOpen()).Returns(dtos.Object);

            var controller = new ApplicationsController(repository.Object);

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
            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.ReadOpen()).Returns(dtos.Object);

            var controller = new ApplicationsController(repository.Object);

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
            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.ReadOpen()).Returns(dtos.Object);

            var controller = new ApplicationsController(repository.Object);

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
            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.ReadOpen()).Returns(dtos.Object);

            var controller = new ApplicationsController(repository.Object);

            var get = await controller.Get(2, 2);
            var value = get.Value as ApplicationListDTO;

            Assert.Equal(dto2.ApplicationId, value.List.ElementAt(0).ApplicationId);
            Assert.Equal(3, value.Count);
        }

        [Fact]
        public async Task GetByReceiver_given_valid_id_returns_dtos()
        {
            var input = 1;

            var dto = new ApplicationDTO();
            var dtos = new[] { dto }.AsQueryable().BuildMock();
            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var controller = new ApplicationsController(repository.Object);

            var get = await controller.GetByReceiver(input);

            Assert.Equal(dto, get.Value.Single());
        }

        [Fact]
        public async Task GetByReceiver_given_non_existing_id_returns_NotFound()
        {
            var input = 1;

            var dtos = new List<ApplicationDTO>().AsQueryable().BuildMock();
            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var controller = new ApplicationsController(repository.Object);

            var get = await controller.GetByReceiver(input);

            Assert.IsType<NotFoundResult>(get.Result);
        }

        [Fact]
        public async Task Delete_given_non_existing_applicationId_returns_false()
        {
            var userId = 42;
            var nonexistingApplicationId = 12;

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.DeleteAsync(userId, nonexistingApplicationId)).ReturnsAsync(false);
            
            var controller = new ApplicationsController(repository.Object);

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

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.DeleteAsync(found.ReceiverId, found.ApplicationId)).ReturnsAsync(true);

            var controller = new ApplicationsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(found.ReceiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var delete = await controller.Delete(wrongUserId, found.ApplicationId);

            Assert.IsType<ForbidResult>(delete.Result);
        }

        [Fact]
        public async Task Delete_given_valid_ids_deletes_and_returns_true()
        {
            var found = new ApplicationDTO
            {
                ApplicationId = 1,
                Motivation = "test",
            };

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.DeleteAsync(found.ReceiverId, found.ApplicationId)).ReturnsAsync(true);

            var controller = new ApplicationsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(found.ReceiverId, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var delete = await controller.Delete(found.ReceiverId, found.ApplicationId);

            Assert.True(delete.Value);
        }
    }
}
