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
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> Get(int first, int last)
        {
            if (last == 0)
            {
                last = int.MaxValue;
            }

            var list = await _productRepository.Read().Skip(first).Take(last).ToListAsync();
            
            return list;
        }

        // GET: api/product
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            var product = await _productRepository.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET api/products
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [HttpGet] 
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetByProducer(int producerId)
        {
            var products = await _productRepository.Read(producerId).ToListAsync(); 

            if (products.Count < 1)
            {
                return NotFound();
            }

            return products;
        }

        // PUT: api/products/5
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Put))]
        [HttpPut("{id}")] 
        public async Task<ActionResult> Put([FromBody] ProductCreateUpdateDTO dto)
        {
            var result = await _productRepository.UpdateAsync(dto); 

            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}
