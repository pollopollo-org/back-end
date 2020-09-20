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
        Task<(bool exists, bool created)> CreateAccountIfNotExistsAsync(DonorCreateFromDepositDTO dto);
    }
}
