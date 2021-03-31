using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using PolloPollo.Services;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using PolloPollo.Entities;
using Microsoft.EntityFrameworkCore;
using static PolloPollo.Shared.UserCreateStatus;

namespace PolloPollo.Services.Tests
{
    public class DonorRepositoryTests
    {
        private readonly IPolloPolloContext _context;
        private readonly IDonorRepository _repository;

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
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                   .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.NoContent
                    });

            //Client
            var client = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("https://confirmhere.com")
            };

            //Repository
            _repository = new DonorRepository(_context, client);
        }

        [Fact]
        public async Task CreateAccountIfNotExistsAsyncCreatesAccount()
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
        public async Task CreateUser_Succes()
        {
            var donor = new DonorCreateDTO
            {
                AaAccount = "test",
                Email = "test@test.com",
                Password = "12345678"
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Equal("test", result.AaAccount);
            Assert.Equal(SUCCES, result.Status);
        }

        [Fact]
        public async Task CreateUser_Missing_email()
        {
            var donor = new DonorCreateDTO
            {
                AaAccount = "test",
                Email = "",
                Password = "12345678"
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Null(result.AaAccount);
            Assert.Equal(MISSING_EMAIL, result.Status);
        }

        [Fact]
        public async Task CreateUser_Missing_password()
        {
            var donor = new DonorCreateDTO
            {
                AaAccount = "test",
                Email = "test@test.com",
                Password = ""
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Null(result.AaAccount);
            Assert.Equal(MISSING_PASSWORD, result.Status);
        }

        [Fact]
        public async Task CreateUser_Short_password()
        {
            var donor = new DonorCreateDTO
            {
                AaAccount = "test",
                Email = "test@test.com",
                Password = "short"
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Null(result.AaAccount);
            Assert.Equal(PASSWORD_TOO_SHORT, result.Status);
        }

        [Fact]
        public async Task CreateUser_Email_taken()
        {
            var donor = new DonorCreateDTO
            {
                AaAccount = "test",
                Email = "test@test1.com",
                Password = "P455W0RD!"
            };

            var result = await _repository.CreateAsync(donor);
            Assert.Null(result.AaAccount);
            Assert.Equal(EMAIL_TAKEN, result.Status);
        }

        [Fact]
        public async Task ReadUser_existing()
        {
            var donerRead = await _repository.ReadAsync("seeded-test-donor-1");
            Assert.Equal("seeded-test-donor-1", donerRead.AaAccount);
            Assert.Equal("guid-1", donerRead.UID);
            Assert.Equal("test@test1.com", donerRead.Email);
            Assert.Equal("12345678", donerRead.DeviceAddress);
            Assert.Equal("12345678", donerRead.WalletAddress);
        }

        [Fact]
        public async Task ReadUser_nonexisting()
        {
            var donerRead = await _repository.ReadAsync("not-a-test-donor-1");
            
            Assert.Null(donerRead);
        }

        [Fact]
        public async Task ReadAllUsersFindsAllUsers()
        {
            var donorList = await _repository.ReadAll().ToListAsync();
            var first = donorList[0];
            var last = donorList[donorList.Count - 1];

            Assert.Equal(4, donorList.Count);
            Assert.Equal("seeded-test-donor-1", first.AaAccount);
            Assert.Equal("seeded-test-donor-4", last.AaAccount);
        }

        [Fact]
        public async Task UpdateUpdatesCorrectly()
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

            Assert.Equal("Donor with AaAccount seeded-test-donor-1 was succesfully updated", newDonor);
        }

        [Fact]
        public async Task UpdateUpdatesCorrectlyOnNonExistingDonor()
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

            Assert.Equal("Donor not found.", newDonor);
        }
    }
}
