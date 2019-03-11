using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Repository
{
    public interface IReceiverRepository
    {
        Task<UserDTO> CreateAsync(UserCreateDTO dto);

        Task<UserDTO> FindAsync(int userId);

        Task<bool> DeleteAsync(int userId);

        Task<bool> UpdateAsync(UserCreateUpdateDTO dto);

        IQueryable<UserDTO> Read();

    }
}
