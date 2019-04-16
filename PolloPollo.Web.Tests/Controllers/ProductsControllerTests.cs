using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
using PolloPollo.Shared;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using MockQueryable.Moq;
using System;
using Microsoft.AspNetCore.Authorization;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Web.Controllers.Tests
{
    public class ProductsControllerTests
    {
        private Mock<ClaimsPrincipal> MockClaimsSecurity(int id, string role)
        {
            //Create Claims
            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.NameIdentifier, id.ToString()),
               new Claim(ClaimTypes.Role, role),
            };

            //Mock claim to make the HttpContext contain one.
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(true);

            claimsPrincipalMock.Setup(m => m.Claims).Returns(claims);

            return claimsPrincipalMock;
        }

        [Fact]
        public void ProductsController_has_AuthroizeAttribute()
        {
            var controller = typeof(ProductsController);

            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public async Task Post_given_valid_DTO_creates_and_returns_ProductDTO()
        {
            var id = 1;
            var dto = new ProductCreateDTO
            {
                Title = "Test",
                UserId = 42,
                Price = 42,
                Rank = 1,
            };

            var expected = new ProductDTO
            {
                ProductId = id,
                Title = "Test",
                UserId = 42,
                Price = 42,
                Available = true,
                Rank = 1,
            };

            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.CreateAsync(It.IsAny<ProductCreateDTO>())).ReturnsAsync(expected);

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(dto);

            var result = post.Result as CreatedAtActionResult;
            var resultValue = result.Value as ProductDTO;

            repository.Verify(s => s.CreateAsync(dto));

            Assert.Equal("Get", result.ActionName);
            Assert.Equal(expected.ProductId, result.RouteValues["id"]);
            Assert.Equal(expected.ProductId, resultValue.ProductId);
            Assert.Equal(expected.Rank, resultValue.Rank);
        }

        [Fact]
        public async Task Post_given_invalid_User_Role_returns_Unauthorized()
        {
            var dto = new ProductCreateDTO();

            var userRole = UserRoleEnum.Receiver.ToString();

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, userRole);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(dto);

            Assert.IsType<UnauthorizedResult>(post.Result);
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

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(dto);

            Assert.IsType<ConflictResult>(post.Result);
        }

        [Fact]
        public async Task Post_given_null_returns_Conflict()
        {

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var post = await controller.Post(null);

            Assert.IsType<ConflictResult>(post.Result);
        }

        [Fact]
        public async Task Get_given_first_default_int_and_last_default_int_returns_all_dtos()
        {
            var dto = new ProductDTO();
            var dtos = new[] { dto }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read()).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(0, 0);
            var value = get.Value as ProductListDTO;

            Assert.Equal(dto, value.List.First());
            Assert.Equal(1, value.Count);
        }

        [Fact]
        public async Task Get_given_first_0_last_1_returns_1_dto()
        {
            var dto = new ProductDTO { ProductId = 1 };
            var dto1 = new ProductDTO { ProductId = 2 };
            var dtos = new[] { dto, dto1 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read()).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(0, 1);
            var value = get.Value as ProductListDTO;

            Assert.Equal(dto, value.List.First());
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task Get_given_first_1_last_2_returns_2_last_dto()
        {
            var dto = new ProductDTO { ProductId = 1 };
            var dto1 = new ProductDTO { ProductId = 2 };
            var dto2 = new ProductDTO { ProductId = 3 };
            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read()).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(1, 2);
            var value = get.Value as ProductListDTO;

            Assert.Equal(dto1.ProductId, value.List.ElementAt(0).ProductId);
            Assert.Equal(dto2.ProductId, value.List.ElementAt(1).ProductId);
            Assert.Equal(3, value.Count);
        }

        [Fact]
        public async Task Get_given_first_2_last_2_returns_last_dto()
        {
            var dto = new ProductDTO { ProductId = 1 };
            var dto1 = new ProductDTO { ProductId = 2 };
            var dto2 = new ProductDTO { ProductId = 3 };
            var dtos = new[] { dto, dto1, dto2 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read()).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(2, 2);
            var value = get.Value as ProductListDTO;

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
                Thumbnail = "test.png",
                Available = true,
            };

            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(input);

            Assert.Equal(expected.ProductId, get.Value.ProductId);
            Assert.Equal(expected.Thumbnail, get.Value.Thumbnail);
        }

        [Fact]
        public async Task Get_given_non_existing_id_returns_NotFound()
        {
            var input = 1;

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            var get = await controller.Get(input);

            Assert.IsType<NotFoundResult>(get.Result);
        }

        [Fact]
        public async Task GetByProducer_given_valid_id_and_invalid_status_returns_BadRequestObjectResult_and_message()
        {
            var input = 1;

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetByProducer(input, "bad");
            var result = get.Result as BadRequestObjectResult;

            Assert.IsType<BadRequestObjectResult>(get.Result);
            Assert.Equal("Invalid status in parameter", result.Value);
        }

        [Fact]
        public async Task GetByProducer_given_valid_id_and_all_returns_all_dtos()
        {
            var input = 1;

            var dto = new ProductDTO
            {
                Available = true
            };

            var dto1 = new ProductDTO
            {
                Available = false
            };

            var dtos = new[] { dto, dto1 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetByProducer(input, ProductStatusEnum.All.ToString());

            Assert.Equal(dto, get.Value.ElementAt(0));
            Assert.Equal(dto1, get.Value.ElementAt(1));
        }

        [Fact]
        public async Task GetByProducer_given_valid_id_and_available_returns_available_dtos()
        {
            var input = 1;

            var dto = new ProductDTO
            {
                 Available = true
            };

            var dto1 = new ProductDTO
            {
                Available = false
            };

            var dtos = new[] { dto, dto1 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetByProducer(input, ProductStatusEnum.Available.ToString());

            Assert.Equal(dto, get.Value.Single());
        }

        [Fact]
        public async Task GetByProducer_given_valid_id_and_unavailable_returns_unavailable_dtos()
        {
            var input = 1;

            var dto = new ProductDTO
            {
                Available = false
            };

            var dto1 = new ProductDTO
            {
                Available = true
            };

            var dtos = new[] { dto, dto1 }.AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetByProducer(input, ProductStatusEnum.Unavailable.ToString());

            Assert.Equal(dto, get.Value.Single());
        }

        [Fact]
        public async Task GetByProducer_given_non_existing_id_returns_Empty_List()
        {
            var input = 1;

            var dtos = new List<ProductDTO>().AsQueryable().BuildMock();
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.Read(input)).Returns(dtos.Object);

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetByProducer(input);

            Assert.Equal(new List<ProductDTO>(), get.Value);
        }

        [Fact]
        public async Task GetCount_given_one_product_returns_one()
        {
            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.GetCountAsync()).ReturnsAsync(1);

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetCount();

            Assert.Equal(1, get.Value); 
        }

        [Fact]
        public async Task GetCount_given_none_product_returns_zero()
        {
            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            var get = await controller.GetCount();

            Assert.Equal(0, get.Value); 
        }

        [Fact]
        public async Task Put_given_dto_updates_product()
        {
            var id = 1;
            var userId = 1;
            var dto = new ProductUpdateDTO
            {
                UserId = userId
            };

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(userId, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            await controller.Put(id, dto);

            repository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task Put_given_valid_id_and_valid_dto_returns_PendingCountDTO()
        {
            var userId = 1;
            var id = 1;
            var dto = new ProductUpdateDTO
            {
                UserId = userId
            };
            var countDTO = new PendingApplicationsCountDTO
            {
                PendingApplications = 9000
            };

            var repository = new Mock<IProductRepository>();
            repository.Setup(s => s.UpdateAsync(dto)).ReturnsAsync((true, countDTO.PendingApplications));

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(userId, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.Put(id, dto);

            Assert.Equal(countDTO.PendingApplications, put.Value.PendingApplications);
        }

        [Fact]
        public async Task Put_given_non_existing_returns_false_returns_NotFound()
        {
            var userId = 1;
            var id = 1;
            var dto = new ProductUpdateDTO
            {
                UserId = userId
            };

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(userId, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.Put(id, dto);

            Assert.IsType<NotFoundResult>(put.Result);
        }

        [Fact]
        public async Task Put_given_dto_and_id_with_invalid_User_Role_in_Claim_returns_Unauthorized()
        {
            var dto = new ProductUpdateDTO();

            var id = 1;
            var userRole = UserRoleEnum.Receiver.ToString();


            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id, userRole);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.Put(id, dto);

            Assert.IsType<UnauthorizedResult>(put.Result);
        }

        [Fact]
        public async Task Put_given_dto_with_different_User_id_as_Claim_returns_Forbidden()
        {
            var userId = 1;
            var id = 1;
            var userRole = UserRoleEnum.Producer.ToString();

            var dto = new ProductUpdateDTO
            {
                UserId = userId
            };

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, userRole);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.Put(id, dto);

            Assert.IsType<ForbidResult>(put.Result);
        }

        [Fact]
        public async Task PutImage_given_valid_id_and_image_returns_relative_path_to_file()
        {
            var userId = 1;
            var userIdString = "1";
            var productId = "1";
            var formFile = new Mock<IFormFile>();
            var fileName = "file.png";

            var productImageFormDTO = new ProductImageFormDTO
            {
                UserId = userIdString,
                ProductId = productId,
                File = formFile.Object
            };

            var repository = new Mock<IProductRepository>();
            repository.Setup(r => r.UpdateImageAsync(userId, It.IsAny<IFormFile>())).ReturnsAsync(fileName);
            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(userId, UserRoleEnum.Producer.ToString());


            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var putImage = await controller.PutImage(productImageFormDTO);

            Assert.Equal(fileName, putImage.Value);
        }

        [Fact]
        public async Task PutImage_given_different_User_id_as_claim_returns_Forbidden()
        {
            var formFile = new Mock<IFormFile>();
            var idString = "1";
            var productId = "1";

            var productImageFormDTO = new ProductImageFormDTO
            {
                UserId = idString,
                ProductId = productId,
                File = formFile.Object
            };

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(42, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.PutImage(productImageFormDTO);

            Assert.IsType<ForbidResult>(put.Result);
        }

        [Fact]
        public async Task PutImage_given_non_existing_user_and_valid_claim_returns_NotFoundObjectResult_and_message()
        {
            var formFile = new Mock<IFormFile>();
            var idString = "1";
            var id = 1;
            var productId = "1";
            var error = "Product not found";

            var productImageFormDTO = new ProductImageFormDTO
            {
                UserId = idString,
                ProductId = productId,
                File = formFile.Object
            };

            var repository = new Mock<IProductRepository>();
            repository.Setup(r => r.UpdateImageAsync(id, It.IsAny<IFormFile>())).ReturnsAsync(default(string));

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.PutImage(productImageFormDTO);
            var notFound = put.Result as NotFoundObjectResult;

            Assert.IsType<NotFoundObjectResult>(put.Result);
            Assert.Equal(error, notFound.Value);
        }

        [Fact]
        public async Task PutImage_given_wrong_id_format_returns_BadRequest()
        {
            var formFile = new Mock<IFormFile>();
            var idString = "test";
            var productId = "1";

            var productImageFormDTO = new ProductImageFormDTO
            {
                UserId = idString,
                ProductId = productId,
                File = formFile.Object
            };

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //Create ClaimIdentity
            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.NameIdentifier, idString),
               new Claim(ClaimTypes.Role, UserRoleEnum.Producer.ToString())
            };
            var identity = new ClaimsIdentity(claims);

            //Mock claim to make the HttpContext contain one.
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(true);

            claimsPrincipalMock.Setup(m => m.Claims).Returns(claims);
            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = claimsPrincipalMock.Object;

            var putImage = await controller.PutImage(productImageFormDTO);

            Assert.IsType<BadRequestResult>(putImage.Result);
        }

        [Fact]
        public async Task PutImage_given_invalid_image_returns_BadRequestObjectResult()
        {
            var id = 1;
            var idString = "1";
            var productId = "1";
            var formFile = new Mock<IFormFile>();

            var productImageFormDTO = new ProductImageFormDTO
            {
                UserId = idString,
                ProductId = productId,
                File = formFile.Object
            };

            var repository = new Mock<IProductRepository>();
            repository.Setup(r => r.UpdateImageAsync(id, It.IsAny<IFormFile>())).ThrowsAsync(new ArgumentException("Invalid image file"));
            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var putImage = await controller.PutImage(productImageFormDTO);

            Assert.IsType<BadRequestObjectResult>(putImage.Result);
        }

        [Fact]
        public async Task PutImage_given_invalid_image_returns_InternalServerError()
        {
            var id = 1;
            var idString = "1";
            var productId = "1";
            var formFile = new Mock<IFormFile>();

            var productImageFormDTO = new ProductImageFormDTO
            {
                UserId = idString,
                ProductId = productId,
                File = formFile.Object
            };

            var repository = new Mock<IProductRepository>();
            repository.Setup(r => r.UpdateImageAsync(id, It.IsAny<IFormFile>())).ThrowsAsync(new ArgumentException());
            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id, UserRoleEnum.Producer.ToString());

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var putImage = await controller.PutImage(productImageFormDTO);
            var image = putImage.Result as StatusCodeResult;

            Assert.IsType<StatusCodeResult>(putImage.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, image.StatusCode);
        }

        [Fact]
        public async Task PutImage_given_invalid_User_Role_returns_Unauthorized()
        {
            var formFile = new Mock<IFormFile>();
            var idString = "1";
            var id = 1;
            var productId = "1";
            var userRole = UserRoleEnum.Receiver.ToString();

            var productImageFormDTO = new ProductImageFormDTO
            {
                UserId = idString,
                ProductId = productId,
                File = formFile.Object
            };

            var repository = new Mock<IProductRepository>();

            var controller = new ProductsController(repository.Object);

            // Needs HttpContext to mock it.
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var cp = MockClaimsSecurity(id, userRole);

            //Update the HttpContext to use mocked claim
            controller.ControllerContext.HttpContext.User = cp.Object;

            var put = await controller.PutImage(productImageFormDTO);

            Assert.IsType<UnauthorizedResult>(put.Result);
        }
    }
}
