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
                ProductId = 42,
                Motivation = "I need this product",
            };

            var expected = new ApplicationDTO
            {
                ApplicationId = id,
                UserId = 1,
                ProductId = 42,
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
        public async Task Delete_given_non_existing_applicationId_returns_false()
        {
            var repository = new Mock<IApplicationRepository>();

            var controller = new ApplicationsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var delete = await controller.Delete(42, 42);

            Assert.False(delete);
        }

        [Fact]
        public async Task Delete_given_existing_applicationId_wrong_userId_returns_false()
        {
            var repository = new Mock<IApplicationRepository>();

            var controller = new ApplicationsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var application = new ApplicationCreateDTO
            {
                UserId = 41,
                ProductId = 1,
                Motivation = "test",
            };

            var post = await controller.Post(application);

            var delete = await controller.Delete(42, post.Value.ApplicationId);

            Assert.False(delete);
        }

        [Fact]
        public async Task Delete_given_valid_ids_deletes_and_returns_true()
        {
            var repository = new Mock<IApplicationRepository>();

            var controller = new ApplicationsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Receiver.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var application = new ApplicationCreateDTO
            {
                UserId = 42,
                ProductId = 1,
                Motivation = "test",
            };

            var post = await controller.Post(application);

            var delete = await controller.Delete(42, post.Value.ApplicationId)

            var found = await controller.Find(42);

            Assert.True(delete);
            Assert.Null(found);
        }
    }
}
