using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolloPollo.Repository;
using PolloPollo.Shared;

namespace PolloPollo.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiversController : ControllerBase
    {
        private readonly IReceiverRepository _repository;

        public ReceiversController(IReceiverRepository repository)
        {
            _repository = repository;
        }

        // GET api/receivers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReceiverDTO>>> Get()
        {
            return await _repository.Read().ToListAsync();
        }

        // GET api/receivers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReceiverDTO>> Get(int id)
        {
            var receiver = await _repository.FindAsync(id);

            if (receiver == null)
            {
                return NotFound();
            }

            return receiver;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] UserCreateUpdateDTO dto)
        {
            var result = await _repository.UpdateAsync(dto);

            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repository.DeleteAsync(id);

            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }

    }
}
