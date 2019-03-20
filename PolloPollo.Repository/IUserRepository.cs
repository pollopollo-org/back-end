using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PolloPollo.Shared;

namespace PolloPollo.Repository
{
    public interface IUserRepository
    {
        string Authenticate(string email, string password);
        Task<TokenDTO> CreateAsync(UserCreateDTO dto);
        Task<UserDTO> FindAsync(int userId);
        string HashPassword(string email, string password);
        Task<string> StoreImageAsync(IFormFile file);
        bool VerifyPassword(string email, string password, string plainPassword);
    }
}