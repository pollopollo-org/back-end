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

        public async Task<ReceiverDTO> CreateAsync(UserCreateDTO dto)
        {
            var user = new User
            {
                FirstName = dto.FirstName,
                Surname = dto.Surname,
                Email = dto.Email,
                Country = dto.Country,
                Password = dto.Password
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            await CreateReceiverAsync(user.Id);

            return await FindAsync(user.Id);
        }

        private async Task<bool> CreateReceiverAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return false;
            }

            var receiver = new Receiver
            {
                User = user
            };

            _context.Receivers.Add(receiver);

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

        public async Task<ReceiverDTO> FindAsync(int userId)
        {
            var dto = from r in _context.Receivers
                      where userId == r.User.Id
                      select new ReceiverDTO
                      {
                          Id = r.Id,
                          UserId = r.User.Id,
                          FirstName = r.User.FirstName,
                          Surname = r.User.Surname,
                          Country = r.User.Country,
                          Email = r.User.Email,
                          Password = r.User.Password,
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
                       Id = r.Id,
                       UserId = r.User.Id,
                       FirstName = r.User.FirstName,
                       Surname = r.User.Surname,
                       Email = r.User.Email,
                       Country = r.User.Country,
                       Password = r.User.Password,
                       Description = r.User.Description,
                       City = r.User.City,
                       Thumbnail = r.User.Thumbnail
                   };
        }

        public async Task<bool> UpdateAsync(ReceiverCreateUpdateDTO dto)
        {
            
            //var receiver = await _context.Receivers.FindAsync(dto.Id);
            var user = await _context.Users.FindAsync(dto.UserId);

            if(user == null)
            {
                return false;
            }

            user.FirstName = dto.FirstName;
            user.Surname = dto.Surname;
            user.Email = dto.Email;
            user.Country = dto.Country;
            user.Password = dto.Password;
            user.Description = dto.Description;
            user.City = dto.City;
            user.Thumbnail = dto.Thumbnail;

            await _context.SaveChangesAsync();

            return true;
            
        }
    }
}
