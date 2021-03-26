using PolloPollo.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace PolloPollo.Services
{
    public interface IDonorRepository
    {
        Task<bool> CheckAccountExistsAsync(DonorFromAaDepositDTO dto);
        Task<(bool exists, bool created)> CreateAccountIfNotExistsAsync(DonorFromAaDepositDTO dto);
        Task<(bool, HttpStatusCode, DonorBalanceDTO)> GetDonorBalance(string aaDonorAccount);
        Task<(string AaAccount, string message)> CreateAsync(DonorCreateDTO dto);
        IQueryable<DonorListDTO> ReadAll();
        Task<DonorDTO> ReadAsync(string aaDonorAccount);
        Task<bool> DeleteAsync(string aaDonorAccount);
        Task<string> UpdateAsync(DonorUpdateDTO dto);
        
    }
}
