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
using static PolloPollo.Shared.UserCreateStatus;

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
        public async Task<ActionResult<DonorBalanceDTO>> GetBalance(string aaDonorAccount)
        {
            (bool success, HttpStatusCode code, DonorBalanceDTO balance) = await _donorRepository.GetDonorBalance(aaDonorAccount);

            if (balance == null)
            {
                return NotFound();
            }

            return balance;
        }

        // GET api/donors/42
        [ApiConventionMethod(typeof(DefaultApiConventions),nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("{AaAccount}")]
        public async Task<ActionResult<DonorDTO>> Get([FromRoute] string AaAccount)
        {
            var donor = await _donorRepository.ReadAsync(AaAccount);

            if (donor == null)
            {
                return NotFound();
            }

            return new DonorDTO
            {
                AaAccount = donor.AaAccount,
                Password = donor.Password,
                UID = donor.UID,
                Email = donor.Email,
                DeviceAddress = donor.DeviceAddress,
                WalletAddress = donor.WalletAddress
            };
        }
        // POST api/donors
        [AllowAnonymous]
        [ProducesResponseType(typeof(DonorDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<DonorDTO>> Post([FromBody] DonorCreateDTO dto)
        {

            var result = await _donorRepository.CreateAsync(dto);

            switch(result.Status)
            {
                case SUCCES:
                    return Created($"api/Donors/{result.AaAccount}", result);
                case MISSING_EMAIL:
                    return BadRequest("No email entered");
                case MISSING_PASSWORD:
                    return BadRequest("No password entered");
                case PASSWORD_TOO_SHORT:
                    return BadRequest("Password was to short");
                case EMAIL_TAKEN:
                    return BadRequest("Email already taken");
                case UNKNOWN_FAILURE:
                default:
                    return BadRequest("Unknown error");
            }
        }
        // PUT api/donors/5
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromBody] DonorUpdateDTO dto)
        {
            //Todo: Implement check to ensure that it is only possible for the current user to update themselves.

            var result = await _donorRepository.UpdateAsync(dto);

            if (result.Equals("Donor not found."))
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
