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
        private readonly ILogger<DonorsController> _logger;

        public DonorsController(IDonorRepository dRepo, IApplicationRepository aRepo, ILogger<DonorsController> logger)
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
        [ApiConventionMethod(typeof(DefaultApiConventions),nameof(DefaultApiConventions.Post))]
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateDTO donorParam)
        {
            (DonorDTO dto, string token, UserAuthStatus status) = await _donorRepository.AuthenticateAsync(donorParam.Email, donorParam.Password);

            switch(status)
            {
                case UserAuthStatus.MISSING_EMAIL:
                    return BadRequest("Missing email");
                case UserAuthStatus.MISSING_PASSWORD:
                    return BadRequest("Missing password");
                case UserAuthStatus.NO_USER:
                    return BadRequest("No user with that email");
                case UserAuthStatus.WRONG_PASSWORD:
                    return BadRequest("Wrong password");
                default:
                    return Ok(
                        new DonorTokenDTO
                        {
                            Token = token,
                            DTO = dto
                        });
            }
        }


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
        public async Task<IActionResult> GetBalance(string aaDonorAccount)
        {
            var (statusCode, balance) = await _donorRepository.GetDonorBalanceAsync(aaDonorAccount);

            if (balance == null)
            {
                return NotFound();
            }

            return Ok(balance);
        }

        // GET api/donors/42
        [ApiConventionMethod(typeof(DefaultApiConventions),nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("{AaAccount}")]
        public async Task<IActionResult> Get(string AaAccount)
        {
            var donor = await _donorRepository.ReadAsync(AaAccount);

            if (donor == null)
            {
                return NotFound();
            }

            return Ok(new DonorDTO
            {
                AaAccount = donor.AaAccount,
                Password = donor.Password,
                UID = donor.UID,
                Email = donor.Email,
                DeviceAddress = donor.DeviceAddress,
                WalletAddress = donor.WalletAddress
            });
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
                case SUCCESS:
                    return CreatedAtAction(nameof(Get), new { AaAccount = result.AaAccount }, dto);
                case MISSING_EMAIL:
                    return BadRequest("No email entered");
                case MISSING_PASSWORD:
                    return BadRequest("No password entered");
                case PASSWORD_TOO_SHORT:
                    return BadRequest("Password was too short");
                case EMAIL_TAKEN:
                    return BadRequest("Email already taken");
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

            var status = await _donorRepository.UpdateAsync(dto);

            switch ((int) status)
            {
                case 200:
                    return Ok();
                default:
                    return NotFound();
            }
        }
    }
}
