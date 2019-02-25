using System;
using System.Linq;
using System.Threading.Tasks;
using PolloPollo.Shared;

namespace PolloPollo.Repository
{
    public interface IDummyRepository
    {
        Task<DummyDTO> CreateAsync(DummyCreateUpdateDTO dummy);

        Task<DummyDTO> FindAsync(int dummyId);

        IQueryable<DummyDTO> Read();

        Task<bool> UpdateAsync(DummyCreateUpdateDTO dummy);

        Task<bool> DeleteAsync(int dummyId);

    }
}
