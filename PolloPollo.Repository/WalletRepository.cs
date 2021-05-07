using PolloPollo.Entities;
using PolloPollo.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly PolloPolloContext _context;
        private readonly HttpClient _client;

        public WalletRepository(PolloPolloContext context, HttpClient client)
        {
            _context = context;
            _client = client;
        }

        public async Task<(bool, HttpStatusCode)> ConfirmReceival(int ApplicationId, DetailedUserDTO Receiver, ProductDTO Product, DetailedUserDTO Producer)
        {
            var response = await _client.PostAsJsonAsync($"/postconfirmation", new {applicationId = ApplicationId});
            return (response.IsSuccessStatusCode, response.StatusCode);
        }

        public async Task<(bool, HttpStatusCode)> WithdrawBytes(int ApplicationId, string ProducerWalletAddress, string ProducerDeviceAddress)
        {
            var response = await _client.PostAsJsonAsync($"/withdrawbytes", new { applicationId = ApplicationId,
                                                                                  walletAddress = ProducerWalletAddress,
                                                                                  deviceAddress = ProducerDeviceAddress});
            return (response.IsSuccessStatusCode, response.StatusCode);
        }

        public async Task<(bool, HttpStatusCode, string)> AaCreateApplicationAsync(string ProducerWalletAddress, int AmountBytes, bool IsStableCoin)
        {
            var response = await _client.PostAsJsonAsync($"/aacreateapplication", new
            {
                producerWalletAddress = ProducerWalletAddress,
                amountBytes = AmountBytes,
                isStableCoin = IsStableCoin

            });
            string unitId = await response.Content.ReadAsAsync<string>();
            return (response.IsSuccessStatusCode, response.StatusCode, unitId);
        }


    }
}
