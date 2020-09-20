using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;
using PolloPollo.Web.Security;
using Microsoft.Extensions.Logging;
using System.Net;

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
        public async Task<IActionResult> Put([FromBody] DonorFromAaDepositDTO dto)
        {
            // Only allow updates from local communicaton as only the chat-bot should report
            // application creation results.
            if (!HttpContext.Request.IsLocal())
            {
                return Forbid();
            }

            (bool exists, bool created) = await _donorRepository.CreateAccountIfNotExistsAsync(dto);

            if (exists || created)
            {
                return Ok();
            }

            return NoContent();            
        }

        // GET: api/product
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("donorBalance/{aaDonorAccount}")]
        public async Task<ActionResult<DonorBalanceDTO>> Get(string aaDonorAccount)
        {
            (bool success, HttpStatusCode code, DonorBalanceDTO balance) = await _donorRepository.GetDonorBalance(aaDonorAccount);

            if (balance == null)
            {
                return NotFound();
            }

            return balance;
        }
    }
}
