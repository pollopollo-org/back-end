using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Repository;
using PolloPollo.Shared;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore; 

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    //[ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private IProductRepository _productRepository;

        public ProductsController(IProductRepository repo)
        {
            _productRepository = repo;
        }

        //POST
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Post))]
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Post([FromBody] ProductCreateUpdateDTO dto)
        {
            var created = await _productRepository.CreateAsync(dto);

            if (created == null)
            {
                return Conflict();
            }

            return Ok(created);
        }

        // GET api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> Get()
        {
            return await _productRepository.Read().ToListAsync();
        }
    }
}
