using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolloPollo.Shared;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly string folder;

        public ProductsController(IProductRepository repo)
        {
            _productRepository = repo;
            folder = "static";
        }

        //POST
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Post([FromBody] ProductCreateDTO dto)
        {
            var claimRole = User.Claims.First(c => c.Type == ClaimTypes.Role);

            if (!claimRole.Value.Equals(UserRoleEnum.Producer.ToString()))
            {
                return Unauthorized();
            }

            var created = await _productRepository.CreateAsync(dto);

            if (created == null)
            {
                return Conflict();
            }

            return CreatedAtAction(nameof(Get), new { id = created.ProductId }, created);
        }

        // GET api/products
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ProductListDTO>> Get(int offset, int amount)
        {
            if (amount == 0)
            {
                amount = int.MaxValue;
            }

            var read = _productRepository.Read();
            var list = await _productRepository.Read().Skip(offset).Take(amount).ToListAsync();

            return new ProductListDTO
            {
                Count = read.Count(),
                List = list
            };
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

        // GET api/products/producer
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("producer/{producerId}")] 
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetByProducer(int producerId)
        {
            var products = await _productRepository.Read(producerId).ToListAsync(); 

            if (products.Count < 1)
            {
                return NotFound();
            }

            return products;
        }

        // GET api/products/count
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("count")]
        public async Task<ActionResult<int>> Get()
        {
            return await _productRepository.GetCountAsync();
        }

        // PUT: api/products/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}")] 
        public async Task<ActionResult> Put(int id, [FromBody] ProductUpdateDTO dto)
        {
            var claimRole = User.Claims.First(c => c.Type == ClaimTypes.Role);

            if (!claimRole.Value.Equals(UserRoleEnum.Producer.ToString()))
            {
                return Unauthorized();
            }

            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            // Identity check of current user
            // if id don't match, it is forbidden to update
            if (!claimId.Value.Equals(dto.UserId.ToString()))
            {
                return Forbid();
            }

            var result = await _productRepository.UpdateAsync(dto); 

            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("image")]
        public async Task<ActionResult<string>> PutImage([FromForm] ProductImageFormDTO dto)
        {
            var claimRole = User.Claims.First(c => c.Type == ClaimTypes.Role);

            if (!claimRole.Value.Equals(UserRoleEnum.Producer.ToString()))
            {
                return Unauthorized();
            }

            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            // Identity check of current user
            // if id don't match, it is forbidden to update
            if (!claimId.Value.Equals(dto.UserId))
            {
                return Forbid();
            }

            try
            {
                if (int.TryParse(dto.UserId, out int intId) && int.TryParse(dto.ProductId, out int productIntId))
                {
                    var newImage = await _productRepository.UpdateImageAsync(folder, productIntId, dto.File);

                    if (string.IsNullOrEmpty(newImage))
                    {
                        return NotFound("Product not found");
                    }

                    return Ok($"{folder}/{newImage}");
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
    }
}
