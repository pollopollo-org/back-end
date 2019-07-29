using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using PolloPollo.Services;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using PolloPollo.Entities;
using Microsoft.EntityFrameworkCore;

namespace PolloPollo.Services.Tests
{
    public class WalletRepositoryTests
    {
        [Fact]
        public async Task ConfirmReceival_sends_post()
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

                var repository = new WalletRepository(context, client);

                var (result, code, emailSent) = await repository.ConfirmReceival(42, null, null, null);

                Assert.True(result);

                handler.Protected().Verify(
                        "SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(req =>
                                req.Method == HttpMethod.Post
                                &&
                                req.RequestUri == new Uri("https://confirmhere.com/postconfirmation")
                            ),
                        ItExpr.IsAny<CancellationToken>()
                    );
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
