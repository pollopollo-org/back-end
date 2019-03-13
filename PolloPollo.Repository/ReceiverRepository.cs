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
        private readonly IUserRepository _userRepo;


        public ReceiverRepository(PolloPolloContext context, IUserRepository userRepo)
        {
            _context = context;
            _userRepo = userRepo;
        }

        public async Task<ReceiverDTO> CreateAsync(UserCreateDTO dto)
        {
            int userId = await _userRepo.CreateAsync(dto);

            await CreateReceiverAsync(userId);

            return await FindAsync(userId);
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
            user.Description = dto.Description;
            user.City = dto.City;
            user.Thumbnail = dto.Thumbnail;

            await _context.SaveChangesAsync();

            return true;
            
        }
    }
}
