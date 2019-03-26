
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Repository;
using PolloPollo.Shared;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    // [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserRepository _userRepository;


        public UsersController(IUserRepository repo)
        {
            _userRepository = repo;

        }

        // POST api/users/authenticate
        [AllowAnonymous]
        [ApiConventionMethod(typeof(DefaultApiConventions),
                     nameof(DefaultApiConventions.Post))]
        [HttpPost("authenticate")]
        public async Task<ActionResult<TokenDTO>> Authenticate([FromBody] AuthenticateDTO userParam)
        {
            var (userDTO, token) = await _userRepository.Authenticate(userParam.Email, userParam.Password);

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
                FirstName = user.FirstName,
                SurName = user.SurName,
                UserRole = user.UserRole,
                Description = user.Description,
                Thumbnail = user.Thumbnail,
                City = user.City,
                Country = user.Country
            };
        }

        // GET api/users/me
        [ApiConventionMethod(typeof(DefaultApiConventions),
             nameof(DefaultApiConventions.Get))]
        [HttpGet("me")]
        public async Task<ActionResult<DetailedUserDTO>> Me()
        {
            var claimsIdentity = User.Claims as ClaimsIdentity;

            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            int.TryParse(claimId, out int id);

            var user = await _userRepository.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
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
                return BadRequest("Users must have a assigned a valid role");
            }

            var created = await _userRepository.CreateAsync(dto);

            if (created == null)
            {
                // TODO NEED TESTS
                // Already exists
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    return Conflict("This Email is already registered");
                }

                return BadRequest();
            }

            return CreatedAtAction(nameof(Get), new { id = created.UserDTO.UserId }, created);
        }

        // PUT api/users/5
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromBody] UserUpdateDTO dto)
        {
            var claimsIdentity = User.Claims as ClaimsIdentity;

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
    }
}
