using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PolloPollo.Shared.DTO;
using PolloPollo.Shared;

namespace PolloPollo.Services
{
    public interface IUserRepository
    {
        Task<(DetailedUserDTO userDTO, string token)> Authenticate(string email, string password);
        Task<(UserCreateStatus status, TokenDTO dto)> CreateAsync(UserCreateDTO dto);
        Task<DetailedUserDTO> FindAsync(int userId);
        Task<bool> UpdateAsync(UserUpdateDTO dto);
        Task<bool> UpdateDeviceAddressAsync(UserPairingDTO dto);
        Task<string> UpdateImageAsync(int id, IFormFile image);
        Task<int> GetCountProducersAsync();
        Task<int> GetCountReceiversAsync();
    }
}
