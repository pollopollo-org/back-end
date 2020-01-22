using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;

namespace PolloPollo.Services.Utils
{
    public class EmailClient : IEmailClient
    {
        public (bool sent, string error) SendEmail(string toEmail, string subject, string body)
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
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
