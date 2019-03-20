using Microsoft.EntityFrameworkCore.ChangeTracking;
using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Repository
{
    public interface IProducerRepository
    {
        Task<bool> CreateAsync(EntityEntry<User> user);

        Task<ProducerDTO> FindAsync(int userId);

        Task<bool> DeleteAsync(int userId);

        IQueryable<ProducerDTO> Read();
    }
}
