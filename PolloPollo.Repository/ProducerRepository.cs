using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PolloPollo.Entities;
using PolloPollo.Shared;

namespace PolloPollo.Repository
{
    public class ProducerRepository : IProducerRepository
    {
        private readonly PolloPolloContext _context;

        public ProducerRepository(PolloPolloContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(EntityEntry<User> createdUser)
        {
            if(createdUser == null)
            {
                return false;
            }

            var producerUserRole = new UserRole
            {
                UserId = createdUser.Entity.Id,
                UserRoleEnum = UserRoleEnum.Producer
            };

            _context.UserRoles.Add(producerUserRole);

            await _context.SaveChangesAsync();

            var producer = new Producer
            {
                UserId = producerUserRole.UserId
            };

            _context.Producers.Add(producer);

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ProducerDTO> FindAsync(int userId)
        {
            /*   var dto = from r in _context.Producers
                         where userId == r.User.UserId
                         select new ProducerDTO
                         {
                             ProducerId = r.Id,
                             UserId = r.User.UserId,
                             FirstName = r.User.FirstName,
                             Surname = r.User.Surname,
                             Country = r.User.Country,
                             Email = r.User.Email,
                             Description = r.User.Description,
                             City = r.User.City,
                             Thumbnail = r.User.Thumbnail,
                             Wallet = r.Wallet
                         };

               return await dto.FirstOrDefaultAsync(); */
            throw new NotImplementedException();

        }

        public IQueryable<ProducerDTO> Read()
        {
            /*
            return from p in _context.Producers
                   select new ProducerDTO
                   {
                       ProducerId = p.Id,
                       UserId = p.User.UserId,
                       Wallet = p.Wallet,
                       FirstName = p.User.FirstName,
                       Surname = p.User.Surname,
                       Email = p.User.Email,
                       Country = p.User.Country,
                       Description = p.User.Description,
                       City = p.User.City,
                       Thumbnail = p.User.Thumbnail
                   }; */
            throw new NotImplementedException();
        }
    }
}
