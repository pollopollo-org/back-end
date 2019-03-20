using PolloPollo.Shared;
using System.Threading.Tasks;

namespace PolloPollo.Repository
{
    public interface IUserRepository
    {
        string Authenticate(string email, string password);

        Task<TokenDTO> CreateAsync(UserCreateDTO dto);

        Task<UserDTO> FindAsync(int userId);

        Task<bool> UpdateAsync(UserUpdateDTO dto);
    }
}
