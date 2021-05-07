namespace PolloPollo.Repository.Utils
{
    public interface IEmailClient
    {
        (bool sent, string error) SendEmail(string toEmail, string subject, string body);
    }
}
