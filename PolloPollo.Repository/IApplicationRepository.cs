using System.Linq;
using System.Threading.Tasks;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Services
{
    public interface IApplicationRepository
    {
        Task<ApplicationDTO> CreateAsync(ApplicationCreateDTO dto);
        Task<ApplicationDTO> FindAsync(int applicationId);
        IQueryable<ApplicationDTO> ReadOpen();
        IQueryable<ApplicationDTO> Read(int receiverId);
        Task<bool> UpdateAsync(ApplicationUpdateDTO dto);
        Task<bool> DeleteAsync(int userId, int applicationId);
    }
}
