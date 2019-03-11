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
    public class UserRepository : IUserRepository
    {
        private readonly PolloPolloContext _context;

        public UserRepository(PolloPolloContext context)
        {
            _context = context;
        }

        public async Task<UserDTO> CreateAsync(UserCreateDTO dto)
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

            return await FindAsync(user.Id);
        }


        public async Task<bool> DeleteAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO> FindAsync(int userId)
        {
            var dto = from u in _context.Users
                      where userId == u.Id
                      select new UserDTO
                      {
                          Id = u.Id,
                          FirstName = u.FirstName,
                          Surname = u.Surname,
                          Country = u.Country,
                          Email = u.Email,
                          Password = u.Password
                      };

            return await dto.FirstOrDefaultAsync();
        }

        public IQueryable<UserDTO> Read()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(UserCreateUpdateDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}
