
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserRepository _userRepository;
        

        public UsersController(IUserRepository repo)
        {
            _userRepository = repo;
           
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateDTO userParam)
        {
            var token = _userRepository.Authenticate(userParam.Email, userParam.Password);

            if (token == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(token);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> Get(int id)
        {
            var user = await _userRepository.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST api/values
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<TokenDTO>> Post([FromBody] UserCreateDTO dto)
        {
            if (!Enum.IsDefined(typeof(UserRoleEnum), dto.Role)) {
                return BadRequest("Users must have a correct role");
            }

            var created = await _userRepository.CreateAsync(dto);

            // Already exists
            if (created == null)
            {
                return Conflict();
            }

            return CreatedAtAction(nameof(Get), new { id = created.UserDTO.UserId }, created);
        }
    }
}
