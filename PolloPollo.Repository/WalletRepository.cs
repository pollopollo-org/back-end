using PolloPollo.Entities;
using PolloPollo.Web.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Services
{
    public class WalletRepository : IWalletRepository
    {
        private readonly PolloPolloContext _context;
        private readonly HttpClient _client;
        private readonly ILogging _log;

        public WalletRepository(PolloPolloContext context, HttpClient client, ILogging log)
        {
            _context = context;
            _client = client;
            _log = log;
        }

        public async Task<bool> ConfirmReceival(int ApplicationId)
        {
            var response = await _client.PostAsJsonAsync($"/api/postconfirmation", new {applicationId = ApplicationId});

            _log.Log(new LogObject
            {
                Timestamp = DateTime.Now,
                EventType = LogEnum.CalledChatbot,
                Message = $"The chatbot was called with application id {ApplicationId}. Response: {response.StatusCode.ToString()}"
            });

            return response.IsSuccessStatusCode;
        }
    }
}
