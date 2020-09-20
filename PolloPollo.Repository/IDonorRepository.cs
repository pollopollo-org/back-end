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
        Task<(bool exists, bool created)> CreateAccountIfNotExistsAsync(DonorFromAaDepositDTO dto);
        Task<(bool, HttpStatusCode, DonorBalanceDTO)> GetDonorBalance(string aaDonorAccount);
        Task<bool> CheckAccountExistsAsync(DonorFromAaDepositDTO dto);
    }
}
