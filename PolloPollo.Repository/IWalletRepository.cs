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
        Task<(bool, HttpStatusCode)> ConfirmReceival(int ApplicationId, DetailedUserDTO Receiver, ProductDTO Product, DetailedUserDTO Producer);
        Task<(bool, HttpStatusCode)> WithdrawBytes(int ApplicationId, string ProducerWalletAddress, string ProducerDeviceAddress);
        Task<(bool, HttpStatusCode, string)> AaCreateApplication(string ProducerWalletAddress, int AmountBytes, bool IsStableCoin);
    }
}
