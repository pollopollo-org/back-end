using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        public void UsersController_has_Authorize_Attribute()
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
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((UserAuthStatus.WRONG_PASSWORD, null, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<BadRequestObjectResult>(authenticate);

            var objectResult = authenticate as BadRequestObjectResult;

            Assert.Equal("Wrong password", objectResult.Value);
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
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((UserAuthStatus.NO_USER, null, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<BadRequestObjectResult>(authenticate);

            var objectResult = authenticate as BadRequestObjectResult;

            Assert.Equal("No donor with that email", objectResult.Value);
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
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((UserAuthStatus.MISSING_PASSWORD, null, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<BadRequestObjectResult>(authenticate);

            var objectResult = authenticate as BadRequestObjectResult;

            Assert.Equal("Missing password", objectResult.Value);
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
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((UserAuthStatus.MISSING_EMAIL, null, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<BadRequestObjectResult>(authenticate);

            var objectResult = authenticate as BadRequestObjectResult;

            Assert.Equal("Missing email", objectResult.Value);
        }

        [Fact]
        public async Task Authenticate_given_success()
        {
            var token = "test_token";

            var donor = new Donor
            {
                Email = "email@test.com",
                Password = "12345678",
            };
            var authDTO = new AuthenticateDTO
            {
                Email = donor.Email,
                Password = donor.Password,
            };
            var detailedDonorDTO = new DetailedDonorDTO
            {
                AaAccount = "test",
                UID = "5",
                Email = authDTO.Email,
                DeviceAddress = "123-456-789",
                WalletAddress = "5",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.AuthenticateAsync(donor.Email, donor.Password)).ReturnsAsync((UserAuthStatus.SUCCESS, detailedDonorDTO, token));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var authenticate = await controller.Authenticate(authDTO);

            Assert.IsType<OkObjectResult>(authenticate);

            var objectResult = authenticate as OkObjectResult;
            var donorTokenDTO = objectResult.Value as DonorTokenDTO;

            Assert.Equal(token, donorTokenDTO.Token);

            var donorAuthenticatedDTO = donorTokenDTO.DTO;

            Assert.Equal("test", donorAuthenticatedDTO.AaAccount);
            Assert.Equal("5", donorAuthenticatedDTO.UID);
            Assert.Equal("email@test.com", donorAuthenticatedDTO.Email);
            Assert.Equal("123-456-789", donorAuthenticatedDTO.DeviceAddress);
            Assert.Equal("5", donorAuthenticatedDTO.WalletAddress);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, null, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, null, env.Object, logger.Object);
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
            repository.Setup(s => s.FindByUnitAsync(applicationUnitId)).ReturnsAsync((ApplicationDTO)null);

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(null, repository.Object, env.Object, logger.Object);
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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

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

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var put = await controller.Put(donorUpdateDTO);

            Assert.IsType<OkResult>(put);
        }

        [Fact]
        public async Task Get_donorBalance_given_non_existing_donor()
        {
            var aaDonorAccount = "test";

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.GetDonorBalanceAsync(aaDonorAccount)).ReturnsAsync((HttpStatusCode.NotFound, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var get = await controller.GetBalance(aaDonorAccount);

            Assert.IsType<NotFoundResult>(get);
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
            repository.Setup(s => s.GetDonorBalanceAsync(aaDonorAccount)).ReturnsAsync((HttpStatusCode.OK, donorBalanceDTO));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var get = await controller.GetBalance(aaDonorAccount);
            Assert.IsType<OkObjectResult>(get);

            var objectResult = get as OkObjectResult;
            Assert.IsType<DonorBalanceDTO>(objectResult.Value);

            var donorBalance = objectResult.Value as DonorBalanceDTO;

            Assert.Equal(4, donorBalance.BalanceInBytes);
            Assert.Equal(5, donorBalance.BalanceInUSD);
        }

        [Fact]
        public async Task Get_donor_given_existing_donor()
        {
            var aaDonorAccount = "test";

            var donorDTO = new DonorDTO
            {
                AaAccount = "test",
                Password = "12345678",
                UID = "5",
                Email = "test@test.dk",
                DeviceAddress = "123-456-789",
                WalletAddress = "5",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.ReadAsync(aaDonorAccount)).ReturnsAsync(donorDTO);

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var get = await controller.Get(aaDonorAccount);
            Assert.IsType<OkObjectResult>(get);

            var objectResult = get as OkObjectResult;
            Assert.IsType<DonorDTO>(objectResult.Value);

            var donor = objectResult.Value as DonorDTO;

            Assert.Equal("test", donor.AaAccount);
            Assert.Equal("12345678", donor.Password);
            Assert.Equal("5", donor.UID);
            Assert.Equal("test@test.dk", donor.Email);
            Assert.Equal("123-456-789", donor.DeviceAddress);
            Assert.Equal("5", donor.WalletAddress);
        }

        [Fact]
        public async Task Get_donor_given_non_existing_donor()
        {
            var aaDonorAccount = "test";

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.ReadAsync(aaDonorAccount)).ReturnsAsync((DonorDTO)null);

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);

            var get = await controller.Get(aaDonorAccount);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task Post_donor_given_exisiting_email()
        {
            var donorCreateDTO = new DonorCreateDTO
            {
                Password = "12345678",
                Email = "existing_email",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAsync(donorCreateDTO)).ReturnsAsync((UserCreateStatus.EMAIL_TAKEN, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
            var create = await controller.Post(donorCreateDTO);

            Assert.IsType<BadRequestObjectResult>(create.Result);

            var result = create.Result as BadRequestObjectResult;

            Assert.Equal("Email already taken", result.Value);
        }

        [Fact]
        public async Task Post_donor_given_short_password()
        {
            var donorCreateDTO = new DonorCreateDTO
            {
                Password = "short",
                Email = "email",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAsync(donorCreateDTO)).ReturnsAsync((UserCreateStatus.PASSWORD_TOO_SHORT, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
            var create = await controller.Post(donorCreateDTO);

            Assert.IsType<BadRequestObjectResult>(create.Result);

            var result = create.Result as BadRequestObjectResult;

            Assert.Equal("Password was too short", result.Value);
        }

        [Fact]
        public async Task Post_donor_given_no_password()
        {
            var donorCreateDTO = new DonorCreateDTO
            {
                Password = "",
                Email = "email",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAsync(donorCreateDTO)).ReturnsAsync((UserCreateStatus.MISSING_PASSWORD, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
            var create = await controller.Post(donorCreateDTO);

            Assert.IsType<BadRequestObjectResult>(create.Result);

            var result = create.Result as BadRequestObjectResult;

            Assert.Equal("No password entered", result.Value);
        }

        [Fact]
        public async Task Post_donor_given_missing_email()
        {
            var donorCreateDTO = new DonorCreateDTO
            {
                Password = "12345678",
                Email = "",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAsync(donorCreateDTO)).ReturnsAsync((UserCreateStatus.MISSING_EMAIL, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
            var create = await controller.Post(donorCreateDTO);

            Assert.IsType<BadRequestObjectResult>(create.Result);

            var result = create.Result as BadRequestObjectResult;

            Assert.Equal("No email entered", result.Value);
        }

        [Fact]
        public async Task Post_donor_given_valid_donor()
        {
            var donorCreateDTO = new DonorCreateDTO
            {
                Password = "12345678",
                Email = "test@test.com",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAsync(donorCreateDTO)).ReturnsAsync((UserCreateStatus.SUCCESS, "ThisIsAnAAccount"));

            DonorDTO dto = new DonorDTO
            {
                AaAccount = "ThisIsAnAAccount",
                Password = "no",
                UID = "5",
                Email = "bob@bob.com",
                DeviceAddress = "no",
                WalletAddress = "yes",
            };

            repository.Setup(s => s.ReadAsync("ThisIsAnAAccount")).ReturnsAsync(dto);

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
            var create = await controller.Post(donorCreateDTO);

            Assert.IsType<CreatedResult>(create.Result);

            var result = create.Result as CreatedResult;
            var donorDTO = result.Value as DonorDTO;

            Assert.Equal("ThisIsAnAAccount", donorDTO.AaAccount);
            Assert.Equal("no", donorDTO.Password);
            Assert.Equal("5", donorDTO.UID);
            Assert.Equal("bob@bob.com", donorDTO.Email);
            Assert.Equal("no", donorDTO.DeviceAddress);
            Assert.Equal("yes", donorDTO.WalletAddress);
        }

        [Fact]
        public async Task Post_donor_given_unknown_error()
        {
            var donorCreateDTO = new DonorCreateDTO
            {
                Password = "12345678",
                Email = "",
            };

            var repository = new Mock<IDonorRepository>();
            repository.Setup(s => s.CreateAsync(donorCreateDTO)).ReturnsAsync((UserCreateStatus.UNKNOWN_FAILURE, null));

            var env = new Mock<IWebHostEnvironment>();

            var logger = new Mock<ILogger<DonorsController>>();

            var controller = new DonorsController(repository.Object, null, env.Object, logger.Object);
            var create = await controller.Post(donorCreateDTO);

            Assert.IsType<BadRequestObjectResult>(create.Result);

            var result = create.Result as BadRequestObjectResult;

            Assert.Equal("Unknown error", result.Value);
        }
    }
}
