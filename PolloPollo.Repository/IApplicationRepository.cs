using System.Threading.Tasks;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Services
{
    public interface IApplicationRepository
    {
        Task<ApplicationDTO> CreateAsync(ApplicationCreateDTO dto);
    }
}
