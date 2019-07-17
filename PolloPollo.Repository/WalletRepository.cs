using PolloPollo.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
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

        public async Task<(bool, HttpStatusCode, bool)> ConfirmReceival(int ApplicationId, string ReceiverEmail, string ProductName, string ProducerAddress)
        {
            var response = await _client.PostAsJsonAsync($"/postconfirmation", new {applicationId = ApplicationId});
            var sent = SendConfirmationEmail(ReceiverEmail, ProductName, ProducerAddress);
            return (response.IsSuccessStatusCode, response.StatusCode, sent);
        }

        public bool SendConfirmationEmail(string ReceiverEmail, string ProductName, string ProducerAddress)
        {
            MailMessage mail = new MailMessage("no-reply@pollopollo.org", ReceiverEmail, "Your PolloPollo application received a donation",
                    "Your application for " + ProductName + " has been fulfilled by a donor. The product can now be picked up at " + ProducerAddress + ". When you receive your product, you must log on to the PolloPollo website and confirm reception of the product. When you confirm reception, the donated funds are released to the Producer of the product.");
            SmtpClient client = new SmtpClient("localhost");
            client.Port = 25;
            client.UseDefaultCredentials = true;
            client.Send(mail);
            return true;
        }
    }
}
