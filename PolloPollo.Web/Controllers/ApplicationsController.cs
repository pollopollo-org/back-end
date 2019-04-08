using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;
using PolloPollo.Shared;

namespace PolloPollo.Web.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationRepository _applicationRepository;

        public ApplicationsController(IApplicationRepository repo)
        {
            _applicationRepository = repo;
        }

        // GET: api/Applications
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApplicationListDTO>> Get(int first, int last)
        {
            if (last == 0) 
            {
                last = int.MaxValue; 
            }

            var read = _applicationRepository.Read(); 
            var list = await _applicationRepository.Read().Skip(first).Take(last).ToListAsync(); 

            return new ApplicationListDTO
            {
                Count = read.Count(),
                List = list
            };
        }

        // GET: api/Applications/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult<ApplicationDTO>> Get(int id)
        {
            var application = await _applicationRepository.FindAsync(id);

            if (application == null) 
            {
                return NotFound(); 
            }

            return application; 
        }

        // GET api/application/receiver 
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("producer/{producerId}")] 
        public async Task<ActionResult<IEnumerable<ApplicationDTO>>> GetByReceiver(int UserId)
        {
            var applications = await _applicationRepository.Read(UserId).ToListAsync(); 

            if (applications.Count < 1)
            {
                return NotFound();
            }

            return applications;
        }

        // POST: api/Applications
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<ApplicationDTO>> Post([FromBody] ApplicationCreateDTO dto)
        {
            var claimRole = User.Claims.First(c => c.Type == ClaimTypes.Role);

            if (!claimRole.Value.Equals(UserRoleEnum.Receiver.ToString()))
            {
                return Unauthorized(); 
            }

            var created = await _applicationRepository.CreateAsync(dto); 

            if (created == null) 
            {
                return Conflict(); 
            }

            return CreatedAtAction(nameof(Get), new {id = created.ApplicationId}, created); 

        }

        // PUT: api/Applications/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [Route("{userId}/{id}")]
        [HttpDelete]
        public async Task<bool> Delete(int userId, int id)
        {
            var found = await _applicationRepository.FindAsync(id);

            if (found == null)
            {
                return false;
            }

            if (found.UserId != userId)
            {
                return false;
            }

            await _applicationRepository.DeleteAsync(id);    

            return true;
        }
    }
}
