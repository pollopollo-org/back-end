using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Shared;
using PolloPollo.Web.Controllers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using MockQueryable.Moq;

namespace PolloPollo.Web.Tests.Controllers
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task Post_given_valid_DTO_creates_and_returns_ProductDTO()
        {
            var id = 1;
            var dto = new ProductCreateDTO
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
            repository.Setup(s => s.CreateAsync(It.IsAny<ProductCreateDTO>())).ReturnsAsync(expected);

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
            var dto = new ProductCreateDTO
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

        [Fact]
        public async Task Get_returns_dtos()
        {
            var dto = new ProductDTO();
            var dtos = new[] { dto }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read()).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get();

            Assert.Equal(dto, get.Value.Single());
        }

        [Fact]
        public async Task Get_given_existing_id_returns_product() 
        {
            var input = 1; 

            var expected = new ProductDTO 
            {
                Title = "Test",
                ProducerId = 42,
                Price = 42,
                Available = true,
            }; 

            var repository = new Mock<IProductRepository>(); 
            repository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected); 
            
            var controller = new ProductsController(repository.Object); 

            var get = await controller.Get(input); 

            Assert.Equal(expected.ProductId, get.Value.ProductId);  
        }

        [Fact]
        public async Task Get_with_non_existing_id_returns_NotFound()
        {
            var input = 1; 

            var repository = new Mock<IProductRepository>(); 

            var controller = new ProductsController(repository.Object); 

            var get = await controller.Get(input); 

            Assert.IsType<NotFoundResult>(get.Result); 
        }
    }
}
