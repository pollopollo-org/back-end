using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolloPollo.Entities;
using PolloPollo.Shared;

namespace PolloPollo.Repository
{
    public class ReceiverRepository : IReceiverRepository
    {
        private readonly PolloPolloContext _context;

        public ReceiverRepository(PolloPolloContext context)
        {
            _context = context;
        }

        public async Task<ReceiverDTO> CreateAsync(int userId)
        {
            var receiver = new Receiver
            {
                UserId = userId
            };

            _context.Receivers.Add(receiver);

            await _context.SaveChangesAsync();

            return await FindAsync(userId);
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

        public async Task<ReceiverDTO> FindAsync(int userId)
        {
            var dto = from r in _context.Receivers
                      where userId == r.User.Id
                      select new ReceiverDTO
                      {
                          ReceiverId = r.Id,
                          UserId = r.User.Id,
                          FirstName = r.User.FirstName,
                          Surname = r.User.Surname,
                          Country = r.User.Country,
                          Email = r.User.Email,
                          Description = r.User.Description,
                          City = r.User.City,
                          Thumbnail = r.User.Thumbnail
                      };

            return await dto.FirstOrDefaultAsync();
        }

        public IQueryable<ReceiverDTO> Read()
        {
            return from r in _context.Receivers
                   select new ReceiverDTO
                   {
                       ReceiverId = r.Id,
                       UserId = r.User.Id,
                       FirstName = r.User.FirstName,
                       Surname = r.User.Surname,
                       Email = r.User.Email,
                       Country = r.User.Country,
                       Description = r.User.Description,
                       City = r.User.City,
                       Thumbnail = r.User.Thumbnail
                   };
        }

        public async Task<bool> UpdateAsync(UserUpdateDTO dto)
        {
            var user = await _context.Users.FindAsync(dto.Id);

            if(user == null)
            {
                return false;
            }

            user.FirstName = dto.FirstName;
            user.Surname = dto.Surname;
            user.Email = dto.Email;
            user.Country = dto.Country;
            user.Description = dto.Description;
            user.City = dto.City;
            user.Thumbnail = ""; // SHOULD USE StoreImageAsync() !!!!!!

            await _context.SaveChangesAsync();

            return true;
            
        }
    }
}
