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
    }
}
