using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var entity = new DummyEntity
            {
                Description = dummy.Description
            };

            _context.Dummies.Add(entity);

            await _context.SaveChangesAsync();

            return await FindAsync(entity.Id);
        }

        public async Task<DummyDTO> FindAsync(int dummyId)
        {
            var entity = from d in _context.Dummies
                         where d.Id == dummyId
                         select new DummyDTO
                         {
                             Id = d.Id,
                             Description = d.Description
                         };

            return await entity.FirstOrDefaultAsync();
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
