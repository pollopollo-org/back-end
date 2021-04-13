using Microsoft.Extensions.Options;
using System;
using PolloPollo.Entities;
using PolloPollo.Shared.DTO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PolloPollo.Services.Utils;
using PolloPollo.Shared;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using static PolloPollo.Shared.UserCreateStatus;

namespace PolloPollo.Services
{
    public class DonorRepository : IDonorRepository
    {
        private readonly IPolloPolloContext _context;
        private readonly HttpClient _client;
        private readonly SecurityConfig _config;
        public DonorRepository(IOptions<SecurityConfig> config, IPolloPolloContext context, HttpClient client)
        {
            _config = config.Value;
            _context = context;
            _client = client;
        }



        public async Task<(DonorDTO DTO, string token, UserAuthStatus status)> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email)) return (null, null, UserAuthStatus.MISSING_EMAIL);
            if (string.IsNullOrEmpty(password)) return (null, null, UserAuthStatus.MISSING_PASSWORD);

            var donorEntity = await ReadFromEmailAsync(email);

            // return null if user not found
            if (donorEntity == null) { return (null, null, UserAuthStatus.NO_USER); }


            var validPassword = PasswordHasher.VerifyPassword(donorEntity.Email, donorEntity.Password, password);

            // if password is invalid, then bail out as well
            if (!validPassword)
            {
                return (null, null, UserAuthStatus.WRONG_PASSWORD);
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Import HmacSHa256 key to be used for creating a unique signing of the token
            // Defined in appsettings
            var key = Encoding.ASCII.GetBytes(_config.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    // Add information to Claim
                    new Claim(ClaimTypes.NameIdentifier, donorEntity.AaAccount)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                // Add unique signature signing to Token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var createdToken = tokenHandler.WriteToken(token);

            return (
                donorEntity,
                createdToken,
                UserAuthStatus.SUCCESS
                );
        }
        public async Task<(UserCreateStatus Status, string AaAccount)> CreateAsync(DonorCreateDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Email)) return (MISSING_EMAIL, null);
            if (string.IsNullOrEmpty(dto.Password)) return (MISSING_PASSWORD, null);
            if (dto.Password.Length < 8) return (PASSWORD_TOO_SHORT, null);
            var exist = from d in _context.Donors where d.Email == dto.Email select d;
            if (await exist.AnyAsync()) return (EMAIL_TAKEN, null);

            try
            {
                var donor = new Donor
                {
                    AaAccount = Guid.NewGuid().ToString(), //Needs to be generated somewhere
                    UID = Guid.NewGuid().ToString(),
                    Email = dto.Email,
                    Password = PasswordHasher.HashPassword(dto.Email, dto.Password)
                };
                await _context.Donors.AddAsync(donor);
                await _context.SaveChangesAsync();
                return (SUCCESS, dto.AaAccount);
            }
            catch (Exception)
            {
                return (UNKNOWN_FAILURE, null);
            }
        }

        public IQueryable<DonorListDTO> ReadAll()
        {
            var list = from d in _context.Donors
                       select new DonorListDTO
                       {
                           AaAccount = d.AaAccount,
                           UID = d.UID,
                           Email = d.Email
                       };
            return list;
        }
        public async Task<DonorDTO> ReadAsync(string aaDonorAccount)
        {
            var donor = await _context.Donors.FindAsync(aaDonorAccount);

            if (donor is null) return null;
            return new DonorDTO
            {
                AaAccount = donor.AaAccount,
                Password = donor.Password,
                UID = donor.UID,
                Email = donor.Email,
                DeviceAddress = donor.DeviceAddress,
                WalletAddress = donor.WalletAddress
            };
        }

        public async Task<DonorDTO> ReadFromEmailAsync(string email)
        {
            var donor = await _context.Donors.Where(d => d.Email == email).Select(d =>
                new DonorDTO
                {
                    AaAccount = d.AaAccount,
                    Password = d.Password,
                    UID = d.UID,
                    Email = d.Email,
                    DeviceAddress = d.DeviceAddress,
                    WalletAddress = d.WalletAddress
                }
            ).SingleOrDefaultAsync();

            if (donor is null) return null;
            return donor;
        }

        public async Task<HttpStatusCode> UpdateAsync(DonorUpdateDTO dto)
        {
            var entity = _context.Donors.FirstOrDefault(d => d.AaAccount == dto.AaAccount);
            if (entity == null)
            {
                return HttpStatusCode.NotFound;
            }
            entity.AaAccount = dto.AaAccount;
            entity.Email = dto.Email;
            entity.Password = dto.Password;
            entity.WalletAddress = dto.WalletAddress;
            entity.DeviceAddress = dto.DeviceAddress;
            await _context.SaveChangesAsync();
            return HttpStatusCode.OK;
        }


        /// <summary>
        /// Check if a donor Pollo Pollo account exists.
        /// </summary>
        /// <param name="dto">DonorFromAaDepositDTO with AccountId populated</param>
        /// <returns></returns>
        public async Task<bool> CheckAccountExistsAsync(string AaAccount)
        {
            int matches = await (from d in _context.Donors
                                 where d.AaAccount == AaAccount
                                 select new
                                 {
                                     d.WalletAddress
                                 }).CountAsync();
            return matches > 0;
        }



        /// <summary>
        /// Create a PolloPollo donor account
        /// </summary>
        /// <param name="dto">Populated DonorFromAaDepositDTO</param>
        /// <returns></returns>
        public async Task<(bool exists, bool created)> CreateAccountIfNotExistsAsync(DonorFromAaDepositDTO dto)
        {
            (bool exists, bool created) = (false, false);

            exists = await CheckAccountExistsAsync(dto.AccountId);

            if (!exists)
            {
                // donor doesn't exist yet, let's create it
                var newDonor = new Donor()
                {
                    WalletAddress = dto.WalletAddress,
                    AaAccount = dto.AccountId
                };
                _context.Donors.Add(newDonor);
                created = await _context.SaveChangesAsync() > 0; // check we've written entry to the db
            }
            return (created, exists);
        }

        /// <summary>
        /// Get balance (from AA via chatbot) for a donor.
        /// </summary>
        /// <param name="aaDonorAccount">Donor AA account ID</param>
        /// <returns></returns>
        public async Task<(HttpStatusCode statusCode, DonorBalanceDTO balance)> GetDonorBalanceAsync(string aaDonorAccount)
        {
            var response = await _client.PostAsJsonAsync($"/aaGetDonorBalance", new
            {
                aaAccount = aaDonorAccount
            });

            DonorBalanceDTO dto = null;

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var balanceInBytes = await response.Content.ReadAsAsync<int>();
                ByteExchangeRate exchangeRate = await _context.ByteExchangeRate.FirstAsync();
                dto = new DonorBalanceDTO
                {
                    BalanceInBytes = balanceInBytes,
                    BalanceInUSD = Shared.BytesToUSDConverter.BytesToUSD(balanceInBytes, exchangeRate.GBYTE_USD),
                };
            }

            return (response.StatusCode, dto);
        }

        /// <summary>
        /// Delete a donor by aaDonorAccount
        /// </summary>
        /// <param name="aaDonorAccount">Donor AA account ID</param>
        public async Task<bool> DeleteAsync(string aaDonorAccount)
        {
            var donor = await _context.Donors.Where(x => x.AaAccount.Equals(aaDonorAccount)).FirstOrDefaultAsync();

            if (donor == null)
            {
                return false;
            }

            _context.Donors.Remove(donor);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
