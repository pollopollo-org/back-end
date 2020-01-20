namespace PolloPollo.Services.Utils
{
    public interface IEmailClient
    {
        bool SendEmail(string toEmail, string subject, string body);
    }
}
