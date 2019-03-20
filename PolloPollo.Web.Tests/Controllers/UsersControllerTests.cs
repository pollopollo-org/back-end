using Microsoft.AspNetCore.Mvc;
using Moq;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Shared;
using PolloPollo.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PolloPollo.Web.Tests
{
    public class UsersControllerTests
    {
        [Fact]
        public void AuthenticateReturnsAuthenticatedUser()
        {
            var user = new User
            {
                Email = "test@itu.dk",
                Password = "1234",
            };
            var dto = new AuthenticateDTO
            {
                Email = "test@itu.dk",
                Password = "1234",
            };
            var token = "verysecrettoken";

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.Authenticate(user.Email, user.Password)).Returns(token);

            var controller = new UsersController(repository.Object);

            var result = controller.Authenticate(dto);
            var okResult = result as OkObjectResult;


            Assert.Equal("verysecrettoken", (okResult.Value as string));
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public void AuthenticateWithNoUserReturns404()
        {
            var user = new User
            {
                Email = "test@itu.dk",
                Password = "1234",
            };
            var dto = new AuthenticateDTO
            {
                Email = "wrong@itu.dk",
                Password = "wrongpassword",
            };
            var token = "verysecrettoken";

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.Authenticate(user.Email, user.Password)).Returns(token);

            var controller = new UsersController(repository.Object);

            var result = controller.Authenticate(dto);
            var badResult = result as BadRequestObjectResult;

            Assert.Equal(400, badResult.StatusCode);
        }

        [Fact]
        public async Task PostWhenRoleReceiverCreatesReceiver()
        {
            var dto = new UserCreateDTO
            {
                FirstName = "Christina",
                Surname = "Steinhauer",
                Email = "test@itu.dk",
                Password = "1234",
                Role = UserRoleEnum.Receiver.ToString(),
            };

            var expected = new TokenDTO();

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.CreateAsync(It.IsAny<UserCreateDTO>())).ReturnsAsync(expected);

            var controller = new UsersController(repository.Object);    

            var result = await controller.Post(dto);

            repository.Verify(s => s.CreateAsync(dto));
        }

        [Fact]
        public async Task GetWhenInputIdReturnsTokenDTO()
        {

            var input = 1;
            
            var expected = new UserDTO
            {
                UserId = input
            };

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.FindAsync(input)).ReturnsAsync(expected);

            var controller = new UsersController(repository.Object);

            var result = await controller.Get(input);

            Assert.Equal(expected.UserId, result.Value.UserId);
        }

        [Fact]
        public async Task GetWhenNotExistingInputIdReturnsNotFound()
        {
            var repository = new Mock<IUserRepository>();

            var controller = new UsersController(repository.Object);

            var get = await controller.Get(0);

            Assert.IsType<NotFoundResult>(get.Result);
        }





        /*
                [Fact]
        public async Task PutGivenDtoUpdatesEntity()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var dto = new ProducerUpdateDTO
            {
                Email = "non_existing_user@itu.dk"
            };

            await controller.Put(dto);

            repository.Verify(s => s.UpdateAsync(dto));
        }

        [Fact]
        public async Task PutReturnsNoContent()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var dto = new ProducerUpdateDTO
            {
                Email = "non_existing_user@itu.dk"
            };

            repository.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(true);

            var put = await controller.Put(dto);

            Assert.IsType<NoContentResult>(put);
        }


        [Fact]
        public async Task PutGivenRepositoryReturnsFalseReturnsNotFound()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var dto = new ProducerUpdateDTO
            {
                Email = "non_existing_user@itu.dk"
            };

            var put = await controller.Put(dto);

            Assert.IsType<NotFoundResult>(put);
        }

        [Fact]
        public async Task PutGivenRepositoryReturnsUnauthorizedResult()
        {
            var repository = new Mock<IProducerRepository>();

            var controller = new ProducersController(repository.Object);

            var dto = new ProducerUpdateDTO();

            var put = await controller.Put(dto);

            Assert.IsType<UnauthorizedResult>(put);
        }
         */



        

    }
}
