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
        Task<ReceiverDTO> CreateAsync(UserCreateDTO dto);

        Task<ReceiverDTO> FindAsync(int userId);

        Task<bool> DeleteAsync(int userId);

        Task<bool> UpdateAsync(ReceiverCreateUpdateDTO dto);

        IQueryable<ReceiverDTO> Read();

    }
}
