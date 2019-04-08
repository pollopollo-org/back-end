using System.Threading.Tasks;
using PolloPollo.Entities;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Services
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly PolloPolloContext _context;

        public ApplicationRepository(PolloPolloContext context)
        {
            _context = context;
        }


    }
}
