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

        public async Task<(bool, HttpStatusCode, bool)> ConfirmReceival(int ApplicationId, DetailedUserDTO Receiver, ProductDTO Product, DetailedUserDTO Producer)
        {
            var response = await _client.PostAsJsonAsync($"/postconfirmation", new {applicationId = ApplicationId});
#if DEBUG
            var sent = true;
#else
            var producerAddress = Producer.Street + " " + Producer.StreetNumber + ", " + Producer.ZipCode + " " + Producer.City;
            var sent = SendConfirmationEmail(Receiver.Email, Product.Title, producerAddress);
#endif

            return (response.IsSuccessStatusCode, response.StatusCode, sent);
        }

        public bool SendConfirmationEmail(string ReceiverEmail, string ProductName, string ProducerAddress)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("no-reply@pollopollo.org"));
            message.To.Add(new MailboxAddress(ReceiverEmail));
            message.Subject = "Your PolloPollo application received a donation";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = $"Your application for {ProductName} has been fulfilled by a donor. The product can now be picked up at {ProducerAddress}. When you receive your product, you must log on to the PolloPollo website and confirm reception of the product. When you confirm reception, the donated funds are released to the Producer of the product."
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect("localhost", 25, false);

                    // Note: only needed if the SMTP server requires authentication
                    //   client.Authenticate("joey", "password");

                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
