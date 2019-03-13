using PolloPollo.Entities;

namespace PolloPollo.Repository
{
    public interface IUserRepository
    {
        User Authenticate(string email, string password);
    }
}