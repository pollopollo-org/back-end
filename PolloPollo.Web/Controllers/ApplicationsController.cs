using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PolloPollo.Services;
using PolloPollo.Shared.DTO;
using PolloPollo.Shared;
using System;
using PolloPollo.Web.Security;
using Microsoft.Extensions.Logging;

namespace PolloPollo.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger<ApplicationsController> _logger;

        public ApplicationsController(IApplicationRepository aRepo, IProductRepository pRepo, IUserRepository uRepo, IWalletRepository wRepo, ILogger<ApplicationsController> logger)
        {
            _applicationRepository = aRepo;
            _userRepository = uRepo;
            _productRepository = pRepo;
            _walletRepository = wRepo;
            _logger = logger;
        }

        // GET: api/Applications/open
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApplicationListDTO>> GetOpen(int offset, int amount)
        {
            if (amount == 0)
            {
                amount = int.MaxValue;
            }

            var read = _applicationRepository.ReadOpen();
            var list = await _applicationRepository.ReadOpen().Skip(offset).Take(amount).ToListAsync();

            return new ApplicationListDTO
            {
                Count = read.Count(),
                List = list
            };
        }

        // GET api/applications/filtered
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("filtered")]
        public async Task<ActionResult<ApplicationListDTO>> GetFiltered(int offset, int amount, string country = "ALL", string city = "ALL")
        {
            if (amount == 0)
            {
                amount = int.MaxValue;
            }

            var read = _applicationRepository.ReadFiltered(country, city);
            var list = await _applicationRepository.ReadFiltered(country, city).Skip(offset).Take(amount).ToListAsync();

            return new ApplicationListDTO
            {
                Count = read.Count(),
                List = list
            };
        }

        // GET: api/Applications/completed
        [AllowAnonymous]
        [HttpGet("completed")]
        public async Task<ActionResult<ApplicationListDTO>> GetCompleted(int offset, int amount)
        {
            if (amount == 0)
            {
                amount = int.MaxValue;
            }

            var read = _applicationRepository.ReadCompleted();
            var list = await _applicationRepository.ReadCompleted().Skip(offset).Take(amount).ToListAsync();

            return new ApplicationListDTO
            {
                Count = read.Count(),
                List = list
            };
        }

        // GET: api/Applications/5
        [AllowAnonymous]
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
        [HttpGet("receiver/{receiverId}")]
        public async Task<ActionResult<IEnumerable<ApplicationDTO>>> GetByReceiver(int receiverId, string status = "All")
        {
            switch (status)
            {
                case nameof(ApplicationStatusEnum.All):
                    return await _applicationRepository.Read(receiverId).ToListAsync();
                default:
                    return Enum.TryParse(status, true, out ApplicationStatusEnum parsedStatus)
                        ? await _applicationRepository
                            .Read(receiverId).Where(a => a.Status == parsedStatus)
                            .ToListAsync()
                        : new List<ApplicationDTO>();
            }
        }

        // GET api/application/producer
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        [AllowAnonymous]
        [HttpGet("producer/{producerId}")]
        public async Task<ActionResult<IEnumerable<ApplicationDTO>>> GetWithdrawableByProducer(int producerId)
        {
            return await _applicationRepository.ReadWithdrawableByProducer(producerId).ToListAsync();
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

            if (created.Status == ApplicationStatusEnum.Unavailable)
            {
                return Forbid();
            }

            return CreatedAtAction(nameof(Get), new {id = created.ApplicationId}, created);

        }

        // PUT api/applications
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ApplicationUpdateDTO dto)
        {
            // Only allow updates from local communicaton. And allow status locking and opening from everywhere,
            if (!HttpContext.Request.IsLocal())
            {
                if (!(dto.Status == ApplicationStatusEnum.Locked || dto.Status == ApplicationStatusEnum.Open))
                    return Forbid();  
            }

            var (result, (emailSent, emailError)) = await _applicationRepository.UpdateAsync(dto);

            if (!result)
            {

                _logger.LogError($"Updating status of application with id {dto.ApplicationId} failed. Application not found.");

                return NotFound();
            }

            _logger.LogInformation($"Status of application with id {dto.ApplicationId} was updated to: {dto.Status.ToString()}.");

            if (dto.Status == ApplicationStatusEnum.Pending) 
            {
                _logger.LogInformation($"Email donation received to receiver, sent to localhost:25. Status: {emailSent}");

                if (emailError != null)
                {
                    _logger.LogError($"Email error on donation received with applicationId: {dto.ApplicationId} with error message: {emailError}");
                }
            }
            if (dto.Status == ApplicationStatusEnum.Completed)
            {
                _logger.LogInformation($"Email thank you, sent to localhost:25. Status: {emailSent}");

                if (emailError != null)
                {
                    _logger.LogError($"Email error on thank you with applicationId: {dto.ApplicationId} with error message: {emailError}");
                }
            }

   

            return NoContent();
        }

        // DELETE: api/ApiWithActions/5
        [Route("{userId}/{id}")]
        [HttpDelete()]
        public async Task<ActionResult<bool>> Delete(int userId, int id)
        {
            var claimRole = User.Claims.First(c => c.Type == ClaimTypes.Role);

            if (!claimRole.Value.Equals(UserRoleEnum.Receiver.ToString()))
            {
                return Unauthorized();
            }

            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            // Identity check of current user
            // if id don't match, it is forbidden to update
            if (!claimId.Value.Equals(userId.ToString()))
            {
                return Forbid();
            }

            // If the application is not open it is forbidden to delete
            var application = await _applicationRepository.FindAsync(id);

            if (application == null) {
                return NotFound();
            }

            if (application.Status != ApplicationStatusEnum.Open) {
                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }

            return await _applicationRepository.DeleteAsync(userId, id);
        }


        // Get api/applications/contractinfo/applicationId
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("contractinfo/{applicationId}")]
        public async Task<ActionResult<ContractInformationDTO>> GetContractInformation(int applicationId)
        {
            if (!HttpContext.Request.IsLocal())
            {
                return Forbid();
            }

            _logger.LogInformation($"Called get Contract information for application with id {applicationId}");

            var result = await _applicationRepository.GetContractInformationAsync(applicationId);


            if (result == null)
            {
                _logger.LogError($"Found no Contract Information for application with id {applicationId}");

                return NotFound();
            }

            _logger.LogInformation($"Got Contract information for application with id {applicationId}, with device address {result.ProducerDevice} and wallet address {result.ProducerWallet}, for price {result.Price}");

            return result;
        }

        // Post: api/10/6
        [Route("{userId}/{Id}")]
        [HttpPost]
        public async Task<ActionResult<bool>> ConfirmReceival(int userId, int Id)
        {
            // Check if the user is the correct usertype
            var claimRole = User.Claims.First(c => c.Type == ClaimTypes.Role);

            if (!claimRole.Value.Equals(UserRoleEnum.Receiver.ToString()))
            {
                return Unauthorized();
            }

            var claimId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            // Identity check of current user
            // if id does not match, it is forbidden to interact
            if (!claimId.Value.Equals(userId.ToString()))
            {
                return Forbid();
            }

            var application = await _applicationRepository.FindAsync(Id);

            if (application == null) {

                _logger.LogError($"Confirmation was attempted but failed for application with id {Id} by user with id {userId}. Application not found.");

                return NotFound();
            }

            if (application.ReceiverId != userId) {
                return Forbid();
            }

            if (application.Status != ApplicationStatusEnum.Pending) {
                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }

            _logger.LogInformation($"Confirmation was attempted for application with id {Id} by user with id {userId}.");

            var receiver = await _userRepository.FindAsync(application.ReceiverId);
            var producer = await _userRepository.FindAsync(application.ProducerId);
            var product = await _productRepository.FindAsync(application.ProductId);

            var (result, statusCode) = await _walletRepository.ConfirmReceival(Id, receiver, product, producer);

            if (result)
            {
                _logger.LogInformation($"The chatbot was called with application id {Id}. Response: {statusCode.ToString()}.");

                var dto = new ApplicationUpdateDTO
                {
                    ApplicationId = Id,
                    Status = ApplicationStatusEnum.Completed
                };

                await _applicationRepository.UpdateAsync(dto);

                return NoContent();
            }
            else
            {
                _logger.LogError($"The chatbot was called with application id {Id}. Response: {statusCode.ToString()}.");

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET: api/applications/countries
        [AllowAnonymous]
        [HttpGet("countries")]
        public IQueryable<string> GetCountries()
        {
            return _applicationRepository.GetCountries();
        }

        // GET: api/applications/cities
        [AllowAnonymous]
        [HttpGet("cities")]
        public IQueryable<string> GetCities(string country)
        {
            return _applicationRepository.GetCities(country);
        }
    }
}
