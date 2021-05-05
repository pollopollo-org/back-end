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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;
using PolloPollo.Web.Security;
using Microsoft.Extensions.Logging;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductRepository repo, IWebHostEnvironment env, ILogger<ProductsController> logger)
        {
            _productRepository = repo;
            _env = env;
            _logger = logger;
        }

        //POST
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Post([FromBody] ProductCreateDTO dto)
        {
            var claimRole = User.Claims.First(c => c.Type == ClaimTypes.Role);

            if (!claimRole.Value.Equals(UserRoleEnum.Producer.ToString()))
            {
                return Forbid();
            }

            var (created, message) = await _productRepository.CreateAsync(dto);

            if (created == null && message.Equals("Error"))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            else if (created == null && message.Equals("No wallet address"))
            {
                return BadRequest("The user has no wallet address");
            }
            else if (created == null) {
                return BadRequest();
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

            var read = _productRepository.ReadOpen();
            var list = await _productRepository.ReadOpen().Skip(offset).Take(amount).ToListAsync();

            return new ProductListDTO
            {
                Count = read.Count(),
                List = list
            };
        }

        // GET api/products/filtered
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("filtered")]
        public async Task<ActionResult<ProductListDTO>> GetFiltered(int offset, int amount, string country = "ALL", string city = "ALL")
        {
            if (amount == 0)
            {
                amount = int.MaxValue;
            }

            var read = _productRepository.ReadFiltered(country, city);
            var list = await _productRepository.ReadFiltered(country, city).Skip(offset).Take(amount).ToListAsync();

            return new ProductListDTO
            {
                Count = read.Count(),
                List = list
            };
        }

        // GET: api/product
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
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
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetByProducer(int producerId, string status = "Available")
        {
            List<ProductDTO> products = null;

            switch (status)
            {
                case nameof(ProductStatusEnum.All):
                    products = await _productRepository.Read(producerId).ToListAsync();
                    break;
                case nameof(ProductStatusEnum.Unavailable):
                    products = await _productRepository.Read(producerId).Where(p => p.Available == false).ToListAsync();
                    break;
                case nameof(ProductStatusEnum.Available):
                    products = await _productRepository.Read(producerId).Where(p => p.Available == true).ToListAsync();
                    break;
                default:
                    return BadRequest("Invalid status in parameter");
            }

            return products;
        }

        // GET api/products/count
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount()
        {
            if (!HttpContext.Request.IsLocal() && !_env.IsDevelopment())
            {
                return Forbid();
            }

            _logger.LogInformation($"Called get products count");

            return await _productRepository.GetCountAsync();
        }

        // PUT: api/products/5
        [ProducesResponseType(typeof(PendingApplicationsCountDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}")]
        public async Task<ActionResult<PendingApplicationsCountDTO>> Put(int id, [FromBody] ProductUpdateDTO dto)
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

            var (status, pendingApplications, (emailSent, emailError)) = await _productRepository.UpdateAsync(dto);

            if (!dto.Available)
            {
                _logger.LogInformation($"Email cancel application to receiver, sent to localhost:25. Status: {emailSent}");

                if (!emailSent || emailError != null)
                {
                    _logger.LogError($"Email error on cancel applications on productId: {dto.Id} with error message: {emailError}");
                }
            }

            if (status)
            {
                return new PendingApplicationsCountDTO
                {
                    PendingApplications = pendingApplications
                };
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
                    var newImagePath = await _productRepository.UpdateImageAsync(productIntId, dto.File);

                    if (string.IsNullOrEmpty(newImagePath))
                    {
                        return NotFound("Product not found");
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

        // GET: api/product/countries
        [AllowAnonymous]
        [HttpGet("countries")]
        public IQueryable<string> GetCountries()
        {
            return _productRepository.GetCountries();
        }

        // GET: api/product/cities
        [AllowAnonymous]
        [HttpGet("cities")]
        public IQueryable<string> GetCities(string country)
        {
            return _productRepository.GetCities(country);
        }
    }
}
