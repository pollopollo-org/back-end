using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Services
{
    public interface IUserRepository
    {
        Task<(DetailedUserDTO userDTO, string token)> Authenticate(string email, string password);
        Task<TokenDTO> CreateAsync(UserCreateDTO dto);
        Task<DetailedUserDTO> FindAsync(int userId);
        Task<bool> UpdateAsync(UserUpdateDTO dto);
        Task<string> UpdateImageAsync(string folder, int id, IFormFile image);
    }
}
