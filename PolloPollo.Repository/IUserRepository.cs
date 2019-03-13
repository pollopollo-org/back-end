namespace PolloPollo.Repository
{
    public interface IUserRepository
    {
        string Authenticate(string email, string password);
    }
}