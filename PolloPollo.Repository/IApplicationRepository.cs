using System.Linq;
using System.Threading.Tasks;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Repository
{
    public interface IApplicationRepository
    {
        Task<ApplicationDTO> CreateAsync(ApplicationCreateDTO dto);
        Task<ApplicationDTO> FindAsync(int applicationId);
        Task<ApplicationDTO> FindByUnitAsync(string unitId);
        IQueryable<ApplicationDTO> ReadOpen();
        IQueryable<ApplicationDTO> ReadFiltered(string country = "ALL", string city = "ALL");
        IQueryable<ApplicationDTO> ReadCompleted();
        IQueryable<ApplicationDTO> Read(int receiverId);
        IQueryable<ApplicationDTO> ReadWithdrawableByProducer(int producerId);
        Task<ContractInformationDTO> GetContractInformationAsync(int applicationId);
        Task<(bool status, (bool emailSent, string emailError))> UpdateAsync(ApplicationUpdateDTO dto);
        Task<bool> DeleteAsync(int userId, int applicationId);
        IQueryable<string> GetCountries();
        IQueryable<string> GetCities(string country);
    }
}
