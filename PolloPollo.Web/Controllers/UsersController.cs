
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Shared;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;
using PolloPollo.Web.Security;
using Microsoft.Extensions.Logging;
using static PolloPollo.Shared.UserCreateStatus;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    // [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IDonorRepository _donorReposistory;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository urepo, IDonorRepository drepo, IWebHostEnvironment env, ILogger<UsersController> logger)
        {
            _userRepository = urepo;
            _donorReposistory = drepo;
            _env = env;
            _logger = logger;
        }

        // POST api/users/authenticate
        [ApiConventionMethod(typeof(DefaultApiConventions),
                     nameof(DefaultApiConventions.Post))]
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateDTO userParam)
        {
            (UserAuthStatus ustatus, DetailedUserDTO userDTO, string utoken) = await _userRepository.Authenticate(userParam.Email, userParam.Password);
            if (ustatus == UserAuthStatus.SUCCESS) return Ok( new TokenDTO { Token = utoken, UserDTO = userDTO });
            (UserAuthStatus dstatus, DetailedDonorDTO donorDTO, string dtoken) = await _donorReposistory.AuthenticateAsync(userParam.Email, userParam.Password);
            if (dstatus == UserAuthStatus.SUCCESS) return Ok( new DonorTokenDTO { Token = dtoken, DTO = donorDTO});

            return BadRequest("Username or password is incorrect");
        }

        // GET api/users/42
        [ApiConventionMethod(typeof(DefaultApiConventions),
             nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> Get(int id)
        {
            var user = await _userRepository.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return new UserDTO
            {
                Id = user.UserId,
                FirstName = user.FirstName,
                SurName = user.SurName,
                UserRole = user.UserRole,
                Description = user.Description,
                Thumbnail = user.Thumbnail,
                City = user.City,
                Country = user.Country,
                Street = user.Street,
                StreetNumber = user.StreetNumber,
                Zipcode = user.Zipcode,
                // Producer stats
                CompletedDonationsPastWeekNo = user.CompletedDonationsPastWeekNo,
                CompletedDonationsPastWeekPrice = user.CompletedDonationsPastWeekPrice,
                CompletedDonationsPastMonthNo = user.CompletedDonationsPastMonthNo,
                CompletedDonationsPastMonthPrice = user.CompletedDonationsPastMonthPrice,
                CompletedDonationsAllTimeNo = user.CompletedDonationsAllTimeNo,
                CompletedDonationsAllTimePrice = user.CompletedDonationsAllTimePrice,

                PendingDonationsPastWeekNo = user.PendingDonationsPastWeekNo,
                PendingDonationsPastWeekPrice = user.PendingDonationsPastWeekPrice,
                PendingDonationsPastMonthNo = user.PendingDonationsPastMonthNo,
                PendingDonationsPastMonthPrice = user.PendingDonationsPastMonthPrice,
                PendingDonationsAllTimeNo = user.PendingDonationsAllTimeNo,
                PendingDonationsAllTimePrice = user.PendingDonationsAllTimePrice
            };
        }

        // GET api/producers/count
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("countproducer")]
        public async Task<ActionResult<int>> GetProducerCount()
        {
            if (!HttpContext.Request.IsLocal() && !_env.IsDevelopment())
            {
                return Forbid();
            }

            _logger.LogInformation($"Called get producer count");

            return await _userRepository.GetCountProducersAsync();
        }

        // GET api/receivers/count
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("countreceiver")]
        public async Task<ActionResult<int>> GetReceiverCount()
        {

            if (!HttpContext.Request.IsLocal() && !_env.IsDevelopment())
            {
                return Forbid();
            }

            _logger.LogInformation($"Called get receiver count");

            return await _userRepository.GetCountReceiversAsync();
        }

        // GET api/users/me
        [ApiConventionMethod(typeof(DefaultApiConventions),
             nameof(DefaultApiConventions.Get))]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var claimsIdentity = User.Claims as ClaimsIdentity;

            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (int.TryParse(claimId, out int id)) {
                var user = await _userRepository.FindAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            var donor = await _donorReposistory.ReadAsync(claimId);
            if (donor == null)
            {
                return NotFound();
            }

            return Ok(donor);
        }

        // POST api/users
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserCreateDTO dto)
        {
            var (status, token) = await _userRepository.CreateAsync(dto);

            switch(status)
            {
                case SUCCESS:
                    return CreatedAtAction(nameof(Get), new { id = token.UserDTO.UserId }, token);
                case MISSING_NAME:
                    return BadRequest("Missing name");
                case MISSING_EMAIL:
                    return BadRequest("Missing email");
                case EMAIL_TAKEN:
                    return Conflict("This Email is already registered");
                case MISSING_PASSWORD:
                    return BadRequest("Missing password");
                case MISSING_COUNTRY:
                    return BadRequest("Missing Country");
                case PASSWORD_TOO_SHORT:
                    return BadRequest("Password too short");
                case INVALID_ROLE:
                    return BadRequest("Users must have assigned a valid role");
                case NULL_INPUT:
                    return BadRequest("Input was null");
                case UNKNOWN_FAILURE:
                default:
                    return BadRequest("Unknown failure");
            }
        }

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("image")]
        public async Task<ActionResult<string>> PutImage([FromForm] UserImageFormDTO dto)
        {
            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            // Identity check of current user
            // if id don't match, it is forbidden to update
            if (!claimId.Value.Equals(dto.UserId))
            {
                return Forbid();
            }

            try
            {
                if (int.TryParse(dto.UserId, out int intId))
                {
                    var newImagePath = await _userRepository.UpdateImageAsync(intId, dto.File);

                    if (string.IsNullOrEmpty(newImagePath))
                    {
                        return NotFound("User not found");
                    }

                    return newImagePath;
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("Invalid image file"))
                {
                    return BadRequest(ex.Message);
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
        }

        // PUT api/users/5
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromBody] UserUpdateDTO dto)
        {
            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            // Identity check of current user
            // if id don't match, it is forbidden to update
            if (!claimId.Value.Equals(dto.UserId.ToString()))
            {
                return Forbid();
            }

            var result = await _userRepository.UpdateAsync(dto);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpPut("wallet")]
        public async Task<ActionResult> PutDeviceAddress([FromBody] UserPairingDTO dto)
        {
            if (!HttpContext.Request.IsLocal() && !_env.IsDevelopment())
            {
                return Forbid();
            }

            _logger.LogInformation($"Updating device address information for user with pairing secret {dto.PairingSecret}");

            var result = await _userRepository.UpdateDeviceAddressAsync(dto);

            if (!result)
            {
                _logger.LogError($"Failed updating deveice address information for user with pairing secret {dto.PairingSecret}, user not found or not correct user role of Producer");

                return NotFound();
            }

            _logger.LogInformation($"Successfully updating device address information for user with pairing secret {dto.PairingSecret}, with the new device address {dto.DeviceAddress} and wallet adress {dto.WalletAddress} ");

            return NoContent();
        }
    }
}
