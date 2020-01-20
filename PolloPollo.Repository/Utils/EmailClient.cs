using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;

namespace PolloPollo.Services.Utils
{
    public class EmailClient : IEmailClient
    {
        private ILogger _logger;

        public EmailClient(ILogger logger)
        {
            _logger = logger;
        }

        public bool SendEmail(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("no-reply@pollopollo.org"));
            message.To.Add(new MailboxAddress(toEmail));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = body
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("localhost", 25, MailKit.Security.SecureSocketOptions.None);
                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email error on address: {toEmail} and subject: {subject} with error message: {ex.Message}");
                return (false);
            }
        }
    }
}
