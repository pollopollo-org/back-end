using Microsoft.AspNetCore.Http;
using PolloPollo.Shared.DTO;
using System.Linq;
using System.Threading.Tasks;

namespace PolloPollo.Repository
{
    public interface IProductRepository
    {
        Task<(ProductDTO created, string message)> CreateAsync(ProductCreateDTO dto);
        Task<ProductDTO> FindAsync(int productId);
        IQueryable<ProductDTO> ReadOpen();
        IQueryable<ProductDTO> ReadFiltered(string country = "ALL", string city = "ALL");
        IQueryable<ProductDTO> Read(int producerId);
        Task<(bool status, int pendingApplications, (bool emailSent, string emailError))> UpdateAsync(ProductUpdateDTO dto);
        Task<string> UpdateImageAsync(int productId, IFormFile image);
        Task<int> GetCountAsync();
        IQueryable<string> GetCountries();
        IQueryable<string> GetCities(string country);
    }
}
