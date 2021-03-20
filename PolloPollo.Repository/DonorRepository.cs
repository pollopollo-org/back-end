using System;
using PolloPollo.Entities;
using PolloPollo.Shared.DTO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PolloPollo.Services.Utils;
namespace PolloPollo.Services
{
    public class DonorRepository : IDonorRepository
    {
        private readonly PolloPolloContext _context;
        private readonly HttpClient _client;

        public DonorRepository(PolloPolloContext context, HttpClient client)
        {
            _context = context;
            _client = client;
        }


        public async Task<(int donorID, string message)?> CreateAsync(DonorCreateDTO dto)
        {
            try
            {
                var donor = new Donor
                {
                    AaAccount = dto.AaAccount,
                    UID = dto.UID,
                    Email = dto.Email,
                    Password = PasswordHasher.HashPassword(dto.Email, dto.Password)
                };
                await _context.Donors.AddAsync(donor);
                await _context.SaveChangesAsync();
                return (donor.Id,"User created successfully");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
        public async Task<DonorDTO> FindAsync(string aaDonorAccount)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(DonorUpdateDTO dto)
        {
             throw new NotImplementedException();
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
