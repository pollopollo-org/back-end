using PolloPollo.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Services
{
    public interface IWalletRepository
    {
        Task<(bool, HttpStatusCode, bool)> ConfirmReceival(int ApplicationId, DetailedUserDTO Receiver, ProductDTO Product, DetailedUserDTO Producer);
    }
}
