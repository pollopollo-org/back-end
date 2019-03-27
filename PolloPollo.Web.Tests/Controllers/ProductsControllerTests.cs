using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Shared;
using PolloPollo.Web.Controllers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Web.Tests.Controllers
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task Post_given_valid_DTO_creates_and_returns_ProductDTO()
        {
            var id = 1;
            var dto = new ProductCreateUpdateDTO
            {
                Title = "Test",
                ProducerId = 42,
                Price = 42,
                Available = true,
            };

            var expected = new ProductDTO
            {
                ProductId = id,
                Title = "Test",
                ProducerId = 42,
                Price = 42,
                Available = true,
            };

            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.CreateAsync(It.IsAny<ProductCreateUpdateDTO>())).ReturnsAsync(expected);

            var controller = new ProductsController(repository.Object);

            var post = await controller.Post(dto);
            var httpResult = post.Result as OkObjectResult;
            var httpValue = httpResult.Value as ProductDTO;

            repository.Verify(s => s.CreateAsync(dto));

            Assert.Equal(StatusCodes.Status200OK, httpResult.StatusCode);
            Assert.IsType<ProductDTO>(httpValue);
            Assert.Equal(expected.ProductId, httpValue.ProductId);
        }

        [Fact]
        public async Task Post_given_existing_product_returns_Conflict()
        {
            var dto = new ProductCreateUpdateDTO
            {
                Title = "Test",
                ProducerId = 42,
                Price = 42,
                Available = true,
            };

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            var post = await controller.Post(dto);

            Assert.IsType<ConflictResult>(post.Result);
        }

        [Fact]
        public async Task Post_given_null_returns_Conflict()
        {
            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            var post = await controller.Post(null);

            Assert.IsType<ConflictResult>(post.Result);
        }
    }
}
