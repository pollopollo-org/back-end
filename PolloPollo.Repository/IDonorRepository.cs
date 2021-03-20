using PolloPollo.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Services
{
    public interface IDonorRepository
    {
        Task<bool> CheckAccountExistsAsync(DonorFromAaDepositDTO dto);
        Task<(bool exists, bool created)> CreateAccountIfNotExistsAsync(DonorFromAaDepositDTO dto);
        Task<(bool, HttpStatusCode, DonorBalanceDTO)> GetDonorBalance(string aaDonorAccount);
        Task<(int donorID, string message)?> CreateAsync(DonorCreateDTO dto);
        Task<DonorDTO> FindAsync(string aaDonorAccount);
        Task<bool> DeleteAsync(string aaDonorAccount);
        Task<bool> UpdateAsync(DonorUpdateDTO dto);
        
    }
}
