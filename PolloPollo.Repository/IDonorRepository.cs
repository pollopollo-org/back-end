using PolloPollo.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using PolloPollo.Shared;

namespace PolloPollo.Services
{
    public interface IDonorRepository
    {
        Task<(bool exists, bool created)> CreateAccountIfNotExistsAsync(DonorFromAaDepositDTO dto);
        Task<(HttpStatusCode statusCode, DonorBalanceDTO balance)> GetDonorBalanceAsync(string aaDonorAccount);
        Task<(UserCreateStatus Status, string AaAccount)> CreateAsync(DonorCreateDTO dto);
        IQueryable<DonorListDTO> ReadAll();
        Task<DonorDTO> ReadAsync(string aaDonorAccount);
        Task<bool> DeleteAsync(string aaDonorAccount);
        Task<HttpStatusCode> UpdateAsync(DonorUpdateDTO dto);
        Task<(UserAuthStatus status, DetailedDonorDTO DTO, string token)> AuthenticateAsync(string email, string password);

    }
}
