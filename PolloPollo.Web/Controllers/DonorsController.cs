using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;
using PolloPollo.Shared;
using System;
using PolloPollo.Web.Security;
using Microsoft.Extensions.Logging;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DonorsController : ControllerBase
    {
        private readonly IDonorRepository _donorRepository;
        private readonly ILogger<ApplicationsController> _logger;

        public DonorsController(IDonorRepository dRepo, ILogger<ApplicationsController> logger)
        {
            _donorRepository = dRepo;
            _logger = logger;
        }

        // PUT api/applications
        //[ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpPut("aaDonorDeposited")]
        public async Task<IActionResult> Put([FromBody] DonorCreateFromDepositDTO dto)
        {
            // Only allow updates from local communicaton as only the chat-bot should report
            // application creation results.
            if (!HttpContext.Request.IsLocal())
            {
                return Forbid();
            }           
            return Ok();
        }
    }
}
