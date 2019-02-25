using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MockQueryable.Moq;
using Moq;
using PolloPollo.Web.Controllers;
using PolloPollo.Repository;
using PolloPollo.Shared;
using Xunit;

namespace PolloPollo.Web.Tests
{
    public class DummiesControllerTests
    {
        [Fact]
        public async Task GetReturnsDTOs()
        {
            var dto = new DummyDTO();
            var all = new[] { dto }.AsQueryable().BuildMock();
            var repository = new Mock<IDummyRepository>();
            repository.Setup(s => s.Read()).Returns(all.Object);

            var controller = new DummiesController(repository.Object);

            var result = await controller.Get();

            Assert.Equal(dto, result.Value.Single());
        }

        [Fact]
        public async Task GetGivenExistingIdReturnsDto()
        {
            var dto = new DummyDTO();
            var repository = new Mock<IDummyRepository>();
            repository.Setup(s => s.FindAsync(42)).ReturnsAsync(dto);

            var controller = new DummiesController(repository.Object);

            var get = await controller.Get(42);

            Assert.Equal(dto, get.Value);
        }

        [Fact]
        public async Task GetGivenNonExistingIdReturnsNotFound()
        {
            var repository = new Mock<IDummyRepository>();

            var controller = new DummiesController(repository.Object);

            var get = await controller.Get(42);

            Assert.IsType<NotFoundResult>(get.Result);
        }

        [Fact]
        public async Task PostGivenDTOCreatesActor()
        {
            var repository = new Mock<IDummyRepository>();
            repository.Setup(s => s.CreateAsync(It.IsAny<DummyCreateUpdateDTO>())).ReturnsAsync(new DummyDTO());

            var controller = new DummiesController(repository.Object);

            var dto = new DummyCreateUpdateDTO();

            await controller.Post(dto);

            repository.Verify(s => s.CreateAsync(dto));
        }

        [Fact]
        public async Task PostGivenDTOReturnsCreatedAtActionResult()
        {
            var input = new DummyCreateUpdateDTO();
            var output = new DummyDTO { Id = 42 };
            var repository = new Mock<IDummyRepository>();
            repository.Setup(s => s.CreateAsync(input)).ReturnsAsync(output);

            var controller = new DummiesController(repository.Object);

            var post = await controller.Post(input);
            var result = post.Result as CreatedAtActionResult;

            Assert.Equal("Get", result.ActionName);
            Assert.Equal(42, result.RouteValues["id"]);
            Assert.Equal(output, result.Value);
        }

        [Fact]
        public async Task PutGivenDtoUpdatesEntity()
        {
            var repository = new Mock<IDummyRepository>();

            var controller = new DummiesController(repository.Object);

            var dto = new DummyCreateUpdateDTO();

            await controller.Put(42, dto);

            repository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task PutReturnsNoContent()
        {
            var dto = new DummyCreateUpdateDTO();
            var repository = new Mock<IDummyRepository>();
            repository.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(true);
            var controller = new DummiesController(repository.Object);

            var put = await controller.Put(42, dto);

            Assert.IsType<NoContentResult>(put);
        }

        [Fact]
        public async Task PutGivenRepositoryReturnsFalseReturnsNotFound()
        {
            var repository = new Mock<IDummyRepository>();

            var controller = new DummiesController(repository.Object);

            var dto = new DummyCreateUpdateDTO();

            var put = await controller.Put(42, dto);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task DeleteGivenExistingIdDeletesEntity()
        {
            var repository = new Mock<IDummyRepository>();

            var controller = new DummiesController(repository.Object);

            await controller.Delete(42);

            repository.Verify(s => s.DeleteAsync(42));
        }

        [Fact]
        public async Task DeleteReturnsNoContent()
        {
            var repository = new Mock<IDummyRepository>();
            
            repository.Setup(s => s.DeleteAsync(42)).ReturnsAsync(true);
            
            var controller = new DummiesController(repository.Object);

            var delete = await controller.Delete(42);

            Assert.IsType<NoContentResult>(delete);
        }

        [Fact]
        public async Task DeleteGivenNonExistingIdReturnsNotFound()
        {
            var repository = new Mock<IDummyRepository>();

            var controller = new DummiesController(repository.Object);

            var delete = await controller.Delete(42);

            Assert.IsType<NotFoundResult>(delete);
        }
    }
}


