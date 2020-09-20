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

namespace PolloPollo.Services.Tests
{
    public class DonorRepositoryTests
    {
        [Fact]
        public async Task CreateAccountIfNotExistsAsyncCreatesAccount()
        {
            using (var connection = await CreateConnectionAsync())
            using (var context = await CreateContextAsync(connection))
            {
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

                var client = new HttpClient(handler.Object)
                {
                    BaseAddress = new Uri("https://confirmhere.com")
                };

                var repository = new DonorRepository(context, client);

                DonorFromAaDepositDTO dto = new DonorFromAaDepositDTO()
                {
                    WalletAddress = "1234567890",
                    AccountId = "ABCDEFG"
                };

                (bool exists, bool created) = await repository.CreateAccountIfNotExistsAsync(dto);

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
        }



        //Below are internal methods for use during testing
        private async Task<DbConnection> CreateConnectionAsync()
        {
            var connection = new SqliteConnection("datasource=:memory:");
            await connection.OpenAsync();

            return connection;
        }

        private async Task<PolloPolloContext> CreateContextAsync(DbConnection connection)
        {
            var builder = new DbContextOptionsBuilder<PolloPolloContext>().UseSqlite(connection);

            var context = new PolloPolloContext(builder.Options);
            await context.Database.EnsureCreatedAsync();

            return context;
        }
    }
}
