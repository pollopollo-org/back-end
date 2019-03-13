
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserRepository _userRepository;
        private readonly IReceiverRepository _receiverRepo;
        private readonly IProducerRepository _producerRepo;

        public UsersController(IUserRepository repo, IProducerRepository producerRepo, IReceiverRepository receiverRepo)
        {
            _userRepository = repo;
            _producerRepo = producerRepo;
            _receiverRepo = receiverRepo;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] UserDTO userParam)
        {
            var token = _userRepository.Authenticate(userParam.Email, userParam.Password);

            if (token == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(token);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> Get(int id)
        {
            var producer = await _userRepository.FindAsync(id);

            if (producer == null)
            {
                return NotFound();
            }

            return producer;
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult<int>> Post([FromBody] UserCreateDTO dto)
        {
            int createdId = 0;

            // Depending on the selected role, create either producer or
            // receiver assoiciated to this user
            switch (dto.Role)
            {
                case "Producer":
                    var createdProd = await _producerRepo.CreateAsync(dto);
                    createdId = createdProd.Id;
                    break;
                case "Receiver":
                    var createdRec = await _receiverRepo.CreateAsync(dto);
                    createdId = createdRec.Id;
                    break;
            }

            return CreatedAtAction(nameof(Get), new { createdId }, createdId);
        }
    }
}
