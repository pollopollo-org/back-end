using System;
using System.Linq;
using System.Threading.Tasks;
using PolloPollo.Entities;
using PolloPollo.Shared;

namespace PolloPollo.Repository
{
    public class DummyRepository : IDummyRepository
    {
        private readonly PolloPolloContext _context;

        public DummyRepository(PolloPolloContext context)
        {
            _context = context;
        }

        public async Task<DummyDTO> CreateAsync(DummyCreateUpdateDTO dummy)
        {
            throw new NotImplementedException();
        }

        public async Task<DummyDTO> FindAsync(int dummyId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<DummyDTO> Read()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(DummyCreateUpdateDTO dummy)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(int dummyId)
        {
            throw new NotImplementedException();
        }
    }
}
