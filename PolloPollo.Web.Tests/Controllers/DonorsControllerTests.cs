using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Org.BouncyCastle.Utilities.Net;
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

        [Fact]
        public async Task Put_aaDonorDeposited_given_existing_donor()
        {
            var donorFromAaDepositDTO = new DonorFromAaDepositDTO
            {
                AccountId = "test",
                WalletAddress = "12345678"
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAccountIfNotExistsAsync(donorFromAaDepositDTO)).ReturnsAsync((true, false));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(donorFromAaDepositDTO);

            Assert.IsType<OkResult>(put);
        }

        [Fact]
        public async Task Put_aaDonorDeposited_given_created_donor()
        {
            var donorFromAaDepositDTO = new DonorFromAaDepositDTO
            {
                AccountId = "test",
                WalletAddress = "12345678"
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAccountIfNotExistsAsync(donorFromAaDepositDTO)).ReturnsAsync((false, true));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(donorFromAaDepositDTO);

            Assert.IsType<OkResult>(put);
        }

        [Fact]
        public async Task Put_aaDonorDeposited_given_non_existing_and_non_created_donor()
        {
            var donorFromAaDepositDTO = new DonorFromAaDepositDTO
            {
                AccountId = "test",
                WalletAddress = "12345678"
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAccountIfNotExistsAsync(donorFromAaDepositDTO)).ReturnsAsync((false, false));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(donorFromAaDepositDTO);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task Put_aaDonorDeposited_given_non_local_connection()
        {
            var donorFromAaDepositDTO = new DonorFromAaDepositDTO
            {
                AccountId = "test",
                WalletAddress = "12345678"
            };

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, null, logger.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Request.Host = new HostString("localhost:");
            httpContext.Connection.RemoteIpAddress = new System.Net.IPAddress(3812831);
            controller.ControllerContext.HttpContext = httpContext;

            var put = await controller.Put(donorFromAaDepositDTO);

            Assert.IsType<ForbidResult>(put);
        }

        [Fact]
        public async Task Put_aaDonationConfirmed_given_non_local_connection()
        {
            var applicationUnitId = "test";

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, null, logger.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.LocalIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            httpContext.Connection.LocalPort = 5001;
            httpContext.Request.Host = new HostString("localhost:");
            httpContext.Connection.RemoteIpAddress = new System.Net.IPAddress(3812831);
            controller.ControllerContext.HttpContext = httpContext;

            var put = await controller.Put(applicationUnitId);

            Assert.IsType<ForbidResult>(put);
        }

        [Fact]
        public async Task Put_aaDonationConfirmed_given_non_existing_application()
        {
            var applicationUnitId = "test";

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.FindByUnitAsync(applicationUnitId)).ReturnsAsync((ApplicationDTO) null);

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(applicationUnitId);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task Put_aaDonationConfirmed_given_update_failed()
        {
            var applicationUnitId = "test";

            var application = new ApplicationDTO
            {
                ApplicationId = 1,
            };

            ApplicationUpdateDTO applicationUpdateDto = new ApplicationUpdateDTO()
            {
                ApplicationId = application.ApplicationId,
                UnitId = applicationUnitId,
                Status = ApplicationStatusEnum.Completed
            };

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.FindByUnitAsync(applicationUnitId)).ReturnsAsync(application);
            repository.Setup(s => s.UpdateAsync(applicationUpdateDto)).ReturnsAsync((false, (false, null)));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(applicationUnitId);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task Put_aaDonationConfirmed_given_status_completed_and_email_error()
        {
            var applicationUnitId = "test";

            var application = new ApplicationDTO
            {
                ApplicationId = 1,
            };

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.FindByUnitAsync(applicationUnitId)).ReturnsAsync(application);
            //a created object here, will not be equal to the one actually given to UpdateAsync, we need to say on any input.
            repository.Setup(s => s.UpdateAsync(It.IsAny<ApplicationUpdateDTO>())).ReturnsAsync((true, (false, "emailerror")));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(applicationUnitId);

            Assert.IsType<OkResult>(put);
        }

        [Fact]
        public async Task Put_aaDonationConfirmed_given_status_completed_and_no_email_error()
        {
            var applicationUnitId = "test";

            var application = new ApplicationDTO
            {
                ApplicationId = 1,
            };

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.FindByUnitAsync(applicationUnitId)).ReturnsAsync(application);
            //a created object here, will not be equal to the one actually given to UpdateAsync, we need to say on any input.
            repository.Setup(s => s.UpdateAsync(It.IsAny<ApplicationUpdateDTO>())).ReturnsAsync((true, (false, null)));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(applicationUnitId);

            Assert.IsType<OkResult>(put);
        }

        [Fact]
        public async Task Put_aaDonationConfirmed_given_status_pending_and_email_error()
        {
            var applicationUnitId = "test";

            var application = new ApplicationDTO
            {
                ApplicationId = 1,
            };

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.FindByUnitAsync(applicationUnitId)).ReturnsAsync(application);
            //a created object here, will not be equal to the one actually given to UpdateAsync, we need to say on any input.
            repository.Setup(s => s.UpdateAsync(It.IsAny<ApplicationUpdateDTO>())).ReturnsAsync((true, (false, "emailerror")));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(applicationUnitId);

            Assert.IsType<OkResult>(put);
        }

        [Fact]
        public async Task Put_aaDonationConfirmed_given_status_pending_and_no_email_error()
        {
            var applicationUnitId = "test";

            var application = new ApplicationDTO
            {
                ApplicationId = 1,
                Bytes = 5,
            };

            var repository = new Mock<IApplicationRepository>();
            repository.Setup(s => s.FindByUnitAsync(applicationUnitId)).ReturnsAsync(application);
            //a created object here, will not be equal to the one actually given to UpdateAsync, we need to say on any input.
            repository.Setup(s => s.UpdateAsync(It.IsAny<ApplicationUpdateDTO>())).ReturnsAsync((true, (true, null)));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, logger.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var put = await controller.Put(applicationUnitId);

            Assert.IsType<OkResult>(put);
        }

        [Fact]
        public async Task Put_donors_given_non_existing_donor()
        {
            var donorUpdateDTO = new DonorUpdateDTO
            {
                DeviceAddress = "test",
                WalletAddress = "test",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.UpdateAsync(donorUpdateDTO)).ReturnsAsync(HttpStatusCode.NotFound);

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);

            var put = await controller.Put(donorUpdateDTO);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task Put_donors_given_existing_donor()
        {
            var donorUpdateDTO = new DonorUpdateDTO
            {
                DeviceAddress = "test",
                WalletAddress = "test",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.UpdateAsync(donorUpdateDTO)).ReturnsAsync(HttpStatusCode.OK);

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);

            var put = await controller.Put(donorUpdateDTO);

            Assert.IsType<OkResult>(put);
        }

        [Fact]
        public async Task Get_donorBalance_given_non_existing_donor()
        {
            var aaDonorAccount = "test";

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.GetDonorBalanceAsync(aaDonorAccount)).ReturnsAsync((HttpStatusCode.NotFound, null));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);

            var put = await controller.GetBalance(aaDonorAccount);

            Assert.IsType<NotFoundResult>(put.Result);
        }

        [Fact]
        public async Task Get_donorBalance_given_existing_donor()
        {
            var aaDonorAccount = "test";

            var donorBalanceDTO = new DonorBalanceDTO
            {
                BalanceInBytes = 4,
                BalanceInUSD = 5,
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.GetDonorBalanceAsync(aaDonorAccount)).ReturnsAsync((HttpStatusCode.NotFound, donorBalanceDTO));

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, logger.Object);

            var put = await controller.GetBalance(aaDonorAccount);

            Assert.IsType<OkResult>(put);

            var result = put.Result as BadRequestObjectResult;

            //Continue here, the result must contain the same information of the donorBalance
        }
    }
}
