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
using static PolloPollo.Shared.UserCreateStatus;

namespace PolloPollo.Services
{
    public class DonorRepository : IDonorRepository
    {
        private readonly IPolloPolloContext _context;
        private readonly HttpClient _client;

        public DonorRepository(IPolloPolloContext context, HttpClient client)
        {
            _context = context;
            _client = client;
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
                    AaAccount = dto.AaAccount, //Needs to be generated somewhere
                    UID = Guid.NewGuid().ToString(),
                    Email = dto.Email,
                    Password = PasswordHasher.HashPassword(dto.Email, dto.Password)
                };
                await _context.Donors.AddAsync(donor);
                await _context.SaveChangesAsync();
                return (SUCCES, dto.AaAccount);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
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

        public async Task<string> UpdateAsync(DonorUpdateDTO dto)
        {
            var entity = _context.Donors.FirstOrDefault(d => d.AaAccount == dto.AaAccount);
            if (entity == null)
            {
                return "Donor not found.";
            }
            entity.AaAccount = dto.AaAccount;
            entity.Email = dto.Email;
            entity.Password = dto.Password;
            entity.WalletAddress = dto.WalletAddress;
            entity.DeviceAddress = dto.DeviceAddress;
            await _context.SaveChangesAsync();
            return "Donor with AaAccount " +entity.AaAccount+" was succesfully updated";
        }


        /// <summary>
        /// Check if a donor Pollo Pollo account exists.
        /// </summary>
        /// <param name="dto">DonorFromAaDepositDTO with AccountId populated</param>
        /// <returns></returns>
        public async Task<bool> CheckAccountExistsAsync(DonorFromAaDepositDTO dto)
        {
            int matches = await (from d in _context.Donors
                                 where d.AaAccount == dto.AccountId
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

            exists = await CheckAccountExistsAsync(dto);

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
        public async Task<(bool, HttpStatusCode, DonorBalanceDTO)> GetDonorBalance(string aaDonorAccount)
        {
            var response = await _client.PostAsJsonAsync($"/aaGetDonorBalance", new
            {
                aaAccount = aaDonorAccount
            });

            DonorBalanceDTO dto = new DonorBalanceDTO();

            if (response.IsSuccessStatusCode)
            {
                dto.BalanceInBytes = await response.Content.ReadAsAsync<int>();
                ByteExchangeRate exchangeRate = await _context.ByteExchangeRate.FirstAsync();
                dto.BalanceInUSD = Shared.BytesToUSDConverter.BytesToUSD(dto.BalanceInBytes, exchangeRate.GBYTE_USD);
            }

            return (response.IsSuccessStatusCode, response.StatusCode, dto);
        }

        /// <summary>
        /// Delete a donor by aaDonorAccount
        /// </summary>
        /// <param name="aaDonorAccount">Donor AA account ID</param>
        public async Task<bool> DeleteAsync(string aaDonorAccount)
        {
            Donor donor = await _context.Donors.Where(x => x.AaAccount.Equals(aaDonorAccount)).FirstOrDefaultAsync();

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
