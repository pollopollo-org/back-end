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
                UserId = 42,
                Price = 42,
            };

            var expected = new ProductDTO
            {
                ProductId = id,
                Title = "Test",
                UserId = 42,
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
                UserId = 42,
                Price = 42,
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

            var get = await controller.Get(0, 0);
            var getOk = get.Result as OkObjectResult;
            var value = getOk.Value as ProductListDTO;

            Assert.Equal(dto, value.List.First());
            Assert.Equal(1, value.Count);
        }

        [Fact]
        public async Task Get_with_first_0_last_1_returns_1_dto()
        {
            var dto = new ProductDTO { ProductId = 1 };
            var dto1 = new ProductDTO { ProductId = 2 };
            var dtos = new[] { dto, dto1 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read()).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(0, 1);
            var getOk = get.Result as OkObjectResult;
            var value = getOk.Value as ProductListDTO;

            Assert.Equal(dto, value.List.First());
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task Get_with_first_1_last_2_returns_2_last_dto()
        {
            var dto = new ProductDTO { ProductId = 1 };
            var dto1 = new ProductDTO { ProductId = 2 };
            var dto2 = new ProductDTO { ProductId = 3 };
            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read()).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(1, 2);
            var getOk = get.Result as OkObjectResult;
            var value = getOk.Value as ProductListDTO;

            Assert.Equal(dto1.ProductId, value.List.ElementAt(0).ProductId);
            Assert.Equal(dto2.ProductId, value.List.ElementAt(1).ProductId);
            Assert.Equal(3, value.Count);
        }

        [Fact]
        public async Task Get_with_first_2_last_2_returns_last_dto()
        {
            var dto = new ProductDTO { ProductId = 1 };
            var dto1 = new ProductDTO { ProductId = 2 };
            var dto2 = new ProductDTO { ProductId = 3 };
            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read()).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(2, 2);
            var getOk = get.Result as OkObjectResult;
            var value = getOk.Value as ProductListDTO;

            Assert.Equal(dto2.ProductId, value.List.ElementAt(0).ProductId);
            Assert.Equal(3, value.Count);
        }

        [Fact]
        public async Task Get_given_existing_id_returns_product() 
        {
            var input = 1; 

            var expected = new ProductDTO 
            {
                Title = "Test",
                UserId = 42,
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

        [Fact]
        public async Task GetByProducer_returns_dtos()
        {
            var input = 1;

            var dto = new ProductDTO();
            var dtos = new[] { dto }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read(1)).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetByProducer(input);

            Assert.Equal(dto, get.Value.Single());
        }

        [Fact]
        public async Task GetByProducer_non_existing_id_returns_NotFound()
        {
            var input = 1;

            var dtos = new List<ProductDTO>().AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetByProducer(input);

            Assert.IsType<NotFoundResult>(get.Result);
        }

        [Fact]
        public async Task Put_given_dto_updates_product()
        {
            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            var dto = new ProductUpdateDTO();

            await controller.Put(dto);

            repository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task Put_returns_NoContent()
        {
            var dto = new ProductUpdateDTO();

            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(true);

            var controller = new ProductsController(repository.Object);

            var put = await controller.Put(dto);

            Assert.IsType<NoContentResult>(put);
        }

        [Fact]
        public async Task Put_given_non_existing_returns_false_returns_NotFound()
        {
            var dto = new ProductUpdateDTO();

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            var put = await controller.Put(dto);

            Assert.IsType<NotFoundResult>(put);
        }
    }
}
