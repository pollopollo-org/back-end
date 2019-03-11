using MockQueryable.Moq;
using Moq;
using PolloPollo.Repository;
using PolloPollo.Shared;
using PolloPollo.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace PolloPollo.Web.Tests.Controllers
{
    public class ProducersControllerTests
    {
        [Fact]
        public async Task GetReturnsDTOs()
        {
            var dto = new ProducerDTO();
            var all = new[] { dto }.AsQueryable().BuildMock();
            var repository = new Mock<IProducerRepository>();
            repository.Setup(s => s.Read()).Returns(all.Object);

            var controller = new ProducersController(repository.Object);

            var result = await controller.Get();

            Assert.Equal(dto, result.Value.Single());
        }

        [Fact]
        public async Task GetGivenExistingIdReturnsDto()
        {
            var dto = new UserDTO();
            var repository = new Mock<IProducerRepository>();
            repository.Setup(s => s.FindAsync(1)).ReturnsAsync(dto);

            var controller = new ProducersController(repository.Object);

            var get = await controller.Get(1);

            Assert.Equal(dto, get.Value);
        }

        [Fact]
        public async Task GetGivenNonExistingIdReturnsNotFound()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var get = await controller.Get(1);

            Assert.IsType<NotFoundResult>(get.Result);
        }
    }    
}
