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
            var userDTO = new UserDTO
            {
                Email = "test@itu.dk",
                Password = "1234",
            };
            var token = "verysecrettoken";

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.Authenticate(user.Email, user.Password)).Returns(token);

            var controller = new UsersController(repository.Object);

            var result = controller.Authenticate(userDTO);
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
            var userDTO = new UserDTO
            {
                Email = "wrong@itu.dk",
                Password = "wrongpassword",
            };
            var token = "verysecrettoken";

            var repository = new Mock<IUserRepository>();
            repository.Setup(s => s.Authenticate(user.Email, user.Password)).Returns(token);

            var controller = new UsersController(repository.Object);

            var result = controller.Authenticate(userDTO);
            var badResult = result as BadRequestObjectResult;

            Assert.Equal(400, badResult.StatusCode);
        }
    }
}
