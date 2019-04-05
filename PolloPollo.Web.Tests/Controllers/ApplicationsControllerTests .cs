using Moq;
using System.Collections.Generic;
using System.Security.Claims;

namespace PolloPollo.Web.Controllers.Tests
{
    public class ApplicationsControllerTests
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
    }
}
