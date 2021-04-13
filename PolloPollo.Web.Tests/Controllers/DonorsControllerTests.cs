using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Services;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using PolloPollo.Web.Controllers;
using Xunit;

namespace PolloPollo.Web.Tests.Controllers
{
    public class DonorsControllerTests
    {

        [Fact]
        public void UsersController_has_AuthroizeAttribute()
        {
            var controller = typeof(DonorsController);

            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public async Task Authenticate_given_wrong_password()
        {
            var token = "verysecrettoken";
            var id = 1;

            var donor = new Donor
            {
                Email = "test@Test",
                Password = "wrongpassword",
            };
            var authDTO = new AuthenticateDTO
            {
                Email = donor.Email,
                Password = donor.Password,
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((null, null, UserAuthStatus.WRONG_PASSWORD));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<BadRequestObjectResult>(authenticate.Result);

            var result = authenticate.Result as BadRequestObjectResult;

            Assert.Equal("Wrong password", result.Value);
        }

        [Fact]
        public async Task Authenticate_given_no_user()
        {
            var token = "verysecrettoken";
            var id = 1;

            var donor = new Donor
            {
                Email = "test@Test",
                Password = "12345678",
            };
            var authDTO = new AuthenticateDTO
            {
                Email = donor.Email,
                Password = donor.Password,
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((null, null, UserAuthStatus.NO_USER));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<BadRequestObjectResult>(authenticate.Result);

            var result = authenticate.Result as BadRequestObjectResult;

            Assert.Equal("No user with that email", result.Value);
        }

        [Fact]
        public async Task Authenticate_given_missing_password()
        {
            var token = "verysecrettoken";
            var id = 1;

            var donor = new Donor
            {
                Email = "test@Test",
                Password = "",
            };
            var authDTO = new AuthenticateDTO
            {
                Email = donor.Email,
                Password = donor.Password,
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((null, null, UserAuthStatus.MISSING_PASSWORD));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<BadRequestObjectResult>(authenticate.Result);

            var result = authenticate.Result as BadRequestObjectResult;

            Assert.Equal("Missing password", result.Value);
        }

        [Fact]
        public async Task Authenticate_given_missing_email()
        {
            var token = "verysecrettoken";
            var id = 1;

            var donor = new Donor
            {
                Email = "",
                Password = "12345678",
            };
            var authDTO = new AuthenticateDTO
            {
                Email = donor.Email,
                Password = donor.Password,
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((null, null, UserAuthStatus.MISSING_EMAIL));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<BadRequestObjectResult>(authenticate.Result);

            var result = authenticate.Result as BadRequestObjectResult;

            Assert.Equal("Missing email", result.Value);
        }
    }
}
