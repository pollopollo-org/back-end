
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Shared;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    // [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository repo)
        {
            _userRepository = repo;
        }

        // POST api/users/authenticate
        [ApiConventionMethod(typeof(DefaultApiConventions),
                     nameof(DefaultApiConventions.Post))]
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult<TokenDTO>> Authenticate([FromBody] AuthenticateDTO userParam)
        {
            (DetailedUserDTO userDTO, string token) = await _userRepository.Authenticate(userParam.Email, userParam.Password);

            if (token == null || userDTO == null)
            {
                return BadRequest("Username or password is incorrect");
            }

            return new TokenDTO
            {
                Token = token,
                UserDTO = userDTO
            };
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
                Country = user.Country
            };
        }

        // GET api/producers/count
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("countproducer")]
        public async Task<ActionResult<int>> GetProducerCount()
        {
            return await _userRepository.GetCountProducersAsync();
        }

         // GET api/receivers/count
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("countreceiver")]
        public async Task<ActionResult<int>> GetReceiverCount()
        {
            return await _userRepository.GetCountReceiversAsync();
        }

        // GET api/users/me
        [ApiConventionMethod(typeof(DefaultApiConventions),
             nameof(DefaultApiConventions.Get))]
        [HttpGet("me")]
        public async Task<ActionResult<DetailedUserDTO>> Me()
        {
            var claimsIdentity = User.Claims as ClaimsIdentity;

            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (int.TryParse(claimId, out int id)) {
                var user = await _userRepository.FindAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                return user;
            }

            return BadRequest();
        }

        // POST api/users
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<TokenDTO>> Post([FromBody] UserCreateDTO dto)
        {
            if (dto.UserRole == null || !Enum.IsDefined(typeof(UserRoleEnum), dto.UserRole))
            {
                return BadRequest("Users must have an assigned a valid role");
            }

            var created = await _userRepository.CreateAsync(dto);

            if (created == null)
            {
                // Already exists
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    return Conflict("This Email is already registered");
                }

                return BadRequest();
            }

            return CreatedAtAction(nameof(Get), new { id = created.UserDTO.UserId }, created);
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

        [HttpPut]
        public async Task<ActionResult> PutDeviceAddress([FromBody] UserPairingDTO dto) 
        {
            var result = await _userRepository.UpdateDeviceAddressAsync(dto);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();

        }
    }
}
