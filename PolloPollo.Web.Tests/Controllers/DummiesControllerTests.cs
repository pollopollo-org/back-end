using System;
using Xunit;

namespace PolloPollo.Web.Tests
{
    public class DummyControllerTests
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
            var repository = new Mock<IActorRepository>();

            var controller = new ActorsController(repository.Object);

            var get = await controller.Get(42);

            Assert.IsType<NotFoundResult>(get.Result);
        }
    }
}


