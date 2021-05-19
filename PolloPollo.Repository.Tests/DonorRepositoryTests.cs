using Moq;
using Moq.Protected;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using PolloPollo.Repository;
using System.Data.Common;
using System.Net;
using Microsoft.Data.Sqlite;
using PolloPollo.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static PolloPollo.Shared.UserCreateStatus;

namespace PolloPollo.Repository.Tests
{
    public class DonorRepositoryTests
    {
        private readonly IPolloPolloContext _context;
        private readonly IDonorRepository _repository;
        private readonly Mock<HttpMessageHandler> _handler;

        public DonorRepositoryTests()
        {
            //Connection
            var connection = new SqliteConnection("datasource=:memory:");
            connection.Open();

            //Context
            var builder = new DbContextOptionsBuilder<PolloPolloContext>().UseSqlite(connection);
            var context = new PolloPolloTestContext(builder.Options);
            context.Database.EnsureCreated();
            _context = context;

            //Handler
            _handler = new Mock<HttpMessageHandler>();

            //Client
            var client = new HttpClient(_handler.Object)
            {
                BaseAddress = new Uri("https://confirmhere.com")
            };

            //Repository
            _repository = new DonorRepository(GetSecurityConfig(), _context, client);
        }

        private IOptions<SecurityConfig> GetSecurityConfig()
        {
            SecurityConfig config = new SecurityConfig
            {
                Secret = "0d797046248eeb96eb32a0e5fdc674f5ad862cad",
            };
            return Options.Create(config as SecurityConfig);
        }

        [Fact]
        public async Task CreateAccountIfNotExists_on_non_existing()
        {
            DonorFromAaDepositDTO dto = new DonorFromAaDepositDTO()
            {
                WalletAddress = "1234567890",
                AccountId = "ABCDEFG"
            };

            (bool exists, bool created) = await _repository.CreateAccountIfNotExistsAsync(dto);

            // confirm that the account already existed or was created
            Assert.True(exists || created);

            /*
            handler.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                            req.Method == HttpMethod.Post
                            &&
                            req.RequestUri == new Uri("https://confirmhere.com/postconfirmation")
                        ),
                    ItExpr.IsAny<CancellationToken>()
                );*/
        }

        [Fact]
        public async Task CreateUser_success()
        {
            var donor = new DonorCreateDTO
            {
                Email = "test@test.com",
                Password = "12345678"
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Equal(SUCCESS, result.Status);
            Assert.False(string.IsNullOrEmpty(result.AaAccount));
        }

        [Fact]
        public async Task CreateUser_missing_email()
        {
            var donor = new DonorCreateDTO
            {
                Email = "",
                Password = "12345678"
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Null(result.AaAccount);
            Assert.Equal(MISSING_EMAIL, result.Status);
        }

        [Fact]
        public async Task CreateUser_missing_password()
        {
            var donor = new DonorCreateDTO
            {
                Email = "test@test.com",
                Password = ""
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Null(result.AaAccount);
            Assert.Equal(MISSING_PASSWORD, result.Status);
        }

        [Fact]
        public async Task CreateUser_short_password()
        {
            var donor = new DonorCreateDTO
            {
                Email = "test@test.com",
                Password = "short"
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Null(result.AaAccount);
            Assert.Equal(PASSWORD_TOO_SHORT, result.Status);
        }

        [Fact]
        public async Task CreateUser_email_taken()
        {
            var donor = new DonorCreateDTO
            {
                Email = "test@test1.com",
                Password = "P455W0RD!"
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Null(result.AaAccount);
            Assert.Equal(EMAIL_TAKEN, result.Status);
        }

        //TODO: Missing a testcase for the UNKNOWN_FAILURE case

        [Fact]
        public async Task ReadUser_existing()
        {
            var donorRead = await _repository.ReadAsync("seeded-test-donor-1");
            Assert.Equal("seeded-test-donor-1", donorRead.AaAccount);
            Assert.Equal("guid-1", donorRead.UID);
            Assert.Equal("test@test1.com", donorRead.Email);
            Assert.Equal("12345678", donorRead.DeviceAddress);
            Assert.Equal("12345678", donorRead.WalletAddress);
        }

        [Fact]
        public async Task ReadUser_nonexisting()
        {
            var donerRead = await _repository.ReadAsync("not-a-test-donor-1");

            Assert.Null(donerRead);
        }

        [Fact]
        public async Task ReadUserFromEmail_existing()
        {
            var donorRead = await _repository.ReadFromEmailAsync("test@test1.com");
            Assert.Equal("seeded-test-donor-1", donorRead.AaAccount);
            Assert.Equal("guid-1", donorRead.UID);
            Assert.Equal("test@test1.com", donorRead.Email);
            Assert.Equal("12345678", donorRead.DeviceAddress);
            Assert.Equal("12345678", donorRead.WalletAddress);
        }

        [Fact]
        public async Task ReadUserFromEmail_nonexisting()
        {
            var donorRead = await _repository.ReadFromEmailAsync("not@a.user");

            Assert.Null(donorRead);
        }


        [Fact]
        public async Task ReadAll_returns_all_users()
        {
            var donorList = await _repository.ReadAll().ToListAsync();
            var first = donorList[0];
            var last = donorList[donorList.Count - 1];

            Assert.Equal(5, donorList.Count);
            Assert.Equal("seeded-test-donor-1", first.AaAccount);
            Assert.Equal("seeded-test-donor-5", last.AaAccount);
        }

        [Fact]
        public async Task Update_on_existing_user()
        {
            var donorUpdate = new DonorUpdateDTO
            {
                AaAccount = "seeded-test-donor-1",
                Email = "new-email",
                Password = "new-password",
                DeviceAddress = "new-device-address",
                WalletAddress = "new-wallet-address"
            };

            var newDonor = await _repository.UpdateAsync(donorUpdate);

            Assert.Equal(HttpStatusCode.OK, newDonor);
        }

        [Fact]
        public async Task Update_on_non_existing_user()
        {
            var donorUpdate = new DonorUpdateDTO
            {
                AaAccount = "seeded-test-donor-43",
                Email = "should-fail",
                Password = "should-fail",
                DeviceAddress = "should-fail",
                WalletAddress = "should-fail"
            };

            var newDonor = await _repository.UpdateAsync(donorUpdate);

            Assert.Equal(HttpStatusCode.NotFound, newDonor);
        }

        [Fact]
        public async Task Authenticate_missing_email()
        {
            var result = await _repository.AuthenticateAsync("", "P4SSW0RD");

            Assert.Equal(UserAuthStatus.MISSING_EMAIL, result.status);
            Assert.Null(result.DTO);
            Assert.Null(result.token);
        }

        [Fact]
        public async Task Authenticate_missing_password()
        {
            var result = await _repository.AuthenticateAsync("a@mail.com", "");

            Assert.Equal(UserAuthStatus.MISSING_PASSWORD, result.status);
            Assert.Null(result.DTO);
            Assert.Null(result.token);
        }

        [Fact]
        public async Task Authenticate_wrong_password()
        {
            var result = await _repository.AuthenticateAsync("test@test1.com", "P4SSW0RD");

            Assert.Equal(UserAuthStatus.WRONG_PASSWORD, result.status);
            Assert.Null(result.DTO);
            Assert.Null(result.token);
        }

        [Fact]
        public async Task Authenticate_nonexisting_user()
        {
            var result = await _repository.AuthenticateAsync("a@mail.com", "P4SSW0RD");

            Assert.Equal(UserAuthStatus.NO_USER, result.status);
            Assert.Null(result.DTO);
            Assert.Null(result.token);
        }

        [Fact]
        public async Task Authenticate_success()
        {
            var result = await _repository.AuthenticateAsync("lol@lol.com", "asdasdasd");

            Assert.Equal(UserAuthStatus.SUCCESS, result.status);

            Assert.NotNull(result.DTO);
            Assert.Equal("lol@lol.com", result.DTO.Email);

            Assert.NotNull(result.token);
            Assert.Equal(192, result.token.Length);
        }

        [Fact]
        public async Task Delete_on_existing()
        {
            var deleted = await _repository.DeleteAsync("seeded-test-donor-1");
            Assert.True(deleted);
        }

        [Fact]
        public async Task Delete_on_non_existing()
        {
            var deleted = await _repository.DeleteAsync("seeded-nonExistingDonor");
            Assert.False(deleted);
        }

        [Fact]
        public async Task GetDonorBalanceOnExisting()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(1000000000), Encoding.UTF8, "application/json")
                });
            var balanceResponse = await _repository.GetDonorBalanceAsync("seeded-test-donor-1");
            Assert.Equal(200, (int) balanceResponse.statusCode);
            Assert.Equal(1_000_000_000, balanceResponse.balance.BalanceInBytes);
            Assert.Equal(7, balanceResponse.balance.BalanceInUSD);
        }

        [Fact]
        public async Task GetDonorBalanceOnNonExisting()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(JsonConvert.SerializeObject(0), Encoding.UTF8, "application/json")
                });
            var balanceResponse = await _repository.GetDonorBalanceAsync("seeded-test-donor-1");
            Assert.Equal(404, (int) balanceResponse.statusCode);
            Assert.Null(balanceResponse.balance);
        }
    }
}
