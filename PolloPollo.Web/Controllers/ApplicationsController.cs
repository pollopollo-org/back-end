using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Applications/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
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

            if (found.UserId != userId)
            {
                return false;
            }

            await _applicationRepository.DeleteAsync(id);    

            return true;
        }
    }
}
