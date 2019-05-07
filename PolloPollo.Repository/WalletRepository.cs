using PolloPollo.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Services
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

        public async Task<(bool, HttpStatusCode)> ConfirmReceival(int ApplicationId)
        {
            var response = await _client.PostAsJsonAsync($"/postconfirmation", new { applicationId = ApplicationId });
            return (response.IsSuccessStatusCode, response.StatusCode);
        }
    }
}
