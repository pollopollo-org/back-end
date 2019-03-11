using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Entities;
using PolloPollo.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolloPollo.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        [AllowAnonymous]
        public string Get()
        {
            var testDummy = new DummyEntity {
               Id = 5000,
               Description = "test",
            };
            return JwtTokenProvider.GenerateAccessToken(testDummy);
        }
    }
}
