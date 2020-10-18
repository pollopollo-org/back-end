using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;
using PolloPollo.Web.Security;
using Microsoft.Extensions.Logging;
using System.Net;
using PolloPollo.Shared;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DonorsController : ControllerBase
    {
        private readonly IDonorRepository _donorRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ILogger<ApplicationsController> _logger;

        public DonorsController(IDonorRepository dRepo, IApplicationRepository aRepo, ILogger<ApplicationsController> logger)
        {
            _applicationRepository = aRepo;
            _donorRepository = dRepo;
            _logger = logger;
        }

        // PUT api/donors/aaDonorDeposited
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

            return NotFound();            
        }

        // TODO: ** write test for this ** 
        // PUT api/donors/aaDonationConfirmed
        //[ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpPut("aaDonationConfirmed")]
        public async Task<IActionResult> Put([FromBody] string applicationUnitId)
        {
            // Only allow updates from local communicaton as only the chat-bot should report
            // application creation results.
            if (!HttpContext.Request.IsLocal())
            {
                return Forbid();
            }

            ApplicationDTO application  = await _applicationRepository.FindByUnitAsync(applicationUnitId);
            if (application == null)
            {
                _logger.LogError($"Updating status of application with unit-id {applicationUnitId} failed. Application not found.");

                return NotFound();
            }
            
            ApplicationUpdateDTO applicationUpdateDto = new ApplicationUpdateDTO()
            {
                ApplicationId = application.ApplicationId,
                UnitId = applicationUnitId,
                Status = ApplicationStatusEnum.Completed
            };

            var(result, (emailSent, emailError)) = await _applicationRepository.UpdateAsync(applicationUpdateDto);

            if (!result)
            {
                _logger.LogError($"Updating status of application with unit-id {applicationUnitId} failed. Application found.");

                return NotFound();
            }

            _logger.LogInformation($"Status of application with id {application.ApplicationId} was updated to: {applicationUpdateDto.Status.ToString()}.");

            if (applicationUpdateDto.Status == ApplicationStatusEnum.Pending)
            {
                _logger.LogInformation($"Email donation received to receiver, sent to localhost:25. Status: {emailSent}");

                if (emailError != null)
                {
                    _logger.LogError($"Email error on donation received with id: {application.ApplicationId} with error message: {emailError}");
                }
            }
            if (applicationUpdateDto.Status == ApplicationStatusEnum.Completed)
            {
                _logger.LogInformation($"Email thank you, sent to localhost:25. Status: {emailSent}");

                if (emailError != null)
                {
                    _logger.LogError($"Email error on thank you with applicationId: {application.ApplicationId} with error message: {emailError}");
                }
            }

            return Ok();
        }

        // GET: api/donors/donorBalance
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
