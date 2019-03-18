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
    public class ProducerRepository : IProducerRepository
    {
        private readonly PolloPolloContext _context;

        public ProducerRepository(PolloPolloContext context)
        {
            _context = context;
        }

        public async Task<ProducerDTO> CreateAsync(UserCreateDTO dto)
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

            await CreateProducerAsync(user.Id);

            return await FindAsync(user.Id);
        }

        private async Task<bool> CreateProducerAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if(user == null)
            {
                return false;
            }

            var producer = new Producer
            {
                User = user
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
            var dto = from r in _context.Producers
                      where userId == r.User.Id
                      select new ProducerDTO
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
                          Thumbnail = r.User.Thumbnail,
                          Wallet = r.Wallet
                      };

            return await dto.FirstOrDefaultAsync();
        }

        public IQueryable<ProducerDTO> Read()
        {

            return from p in _context.Producers
                   select new ProducerDTO
                   {
                       Id = p.Id,
                       UserId = p.User.Id,
                       Wallet = p.Wallet,
                       FirstName = p.User.FirstName,
                       Surname = p.User.Surname,
                       Email = p.User.Email,
                       Country = p.User.Country,
                       Password = p.User.Password,
                       Description = p.User.Description,
                       City = p.User.City,
                       Thumbnail = p.User.Thumbnail
                   };
        }

        public async Task<bool> UpdateAsync(UserCreateUpdateDTO dto)
        {
            var user = await _context.Users.FindAsync(dto.Id);

            if(user == null)
            {
                return false;
            }


            user.Id = dto.Id;
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
