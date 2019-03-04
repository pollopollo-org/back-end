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
            return from d in _context.Dummies
                   select new DummyDTO
                   {
                       Id = d.Id,
                       Description = d.Description
                   };
        }

        public async Task<bool> UpdateAsync(DummyCreateUpdateDTO dummy)
        {
            var entity = await _context.Dummies.FindAsync(dummy.Id);

            if (entity == null)
            {
                return false;
            }

            entity.Description = dummy.Description;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int dummyId)
        {
            var entity = await _context.Dummies.FindAsync(dummyId);

            if (entity == null)
            {
                return false;
            }

            _context.Dummies.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
