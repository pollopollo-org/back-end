using PolloPollo.Entities;
using PolloPollo.Shared.DTO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

        public async Task<(bool exists, bool created)> CreateAccountIfNotExistsAsync(DonorCreateFromDepositDTO dto)
        {
            (bool exists, bool created) = (false, false);

            var donor = await (from d in _context.Donors
                               where d.AaAccount == dto.AccountId
                               select new
                               {
                                   d.WalletAddress
                               }).FirstOrDefaultAsync();

            if (donor == null)
            {
                // donor doesn't exist yet, let's create it
                var newDonor = new Donor()
                {
                    WalletAddress = dto.WalletAddress,
                    AaAccount = dto.AccountId
                };                
                _context.Donors.Add(newDonor);
                await _context.SaveChangesAsync();
            }

            return (true, exists);
        }
    }
}
