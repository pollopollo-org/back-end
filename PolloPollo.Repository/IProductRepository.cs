using Microsoft.AspNetCore.Http;
using PolloPollo.Shared.DTO;
using System.Linq;
using System.Threading.Tasks;

namespace PolloPollo.Services
{
    public interface IProductRepository
    {
        Task<(ProductDTO created, string message)> CreateAsync(ProductCreateDTO dto);
        Task<ProductDTO> FindAsync(int productId);
        IQueryable<ProductDTO> ReadOpen();
        IQueryable<ProductDTO> Read(int producerId);
        Task<(bool status, int pendingApplications, bool emailSent)> UpdateAsync(ProductUpdateDTO dto);
        Task<string> UpdateImageAsync(int productId, IFormFile image);
        Task<int> GetCountAsync();
    }
}
