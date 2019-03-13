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
    public class ReceiversControllerTests
    {
        [Fact]
        public async Task GetReturnsDTOs()
        {
            var dto = new ReceiverDTO();
            var all = new[] { dto }.AsQueryable().BuildMock();
            var repository = new Mock<IReceiverRepository>();
            repository.Setup(s => s.Read()).Returns(all.Object);

            var controller = new ReceiversController(repository.Object);

            var result = await controller.Get();

            Assert.Equal(dto, result.Value.Single());
        }

        [Fact]
        public async Task GetGivenExistingIdReturnsDto()
        {
            var dto = new ReceiverDTO();
            var repository = new Mock<IReceiverRepository>();
            repository.Setup(s => s.FindAsync(42)).ReturnsAsync(dto);

            var controller = new ReceiversController(repository.Object);

            var get = await controller.Get(42);

            Assert.Equal(dto, get.Value);
        }


    }
}
