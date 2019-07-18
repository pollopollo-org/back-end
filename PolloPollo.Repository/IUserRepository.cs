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
        Task<bool> UpdateAsync(ProducerUpdateDTO dto);
        Task<bool> UpdateDeviceAddressAsync(UserPairingDTO dto);
        Task<string> UpdateImageAsync(int id, IFormFile image);
        Task<int> GetCountProducersAsync();
        Task<int> GetCountReceiversAsync();
    }
}
