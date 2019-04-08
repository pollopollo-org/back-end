using Microsoft.AspNetCore.Http;
using PolloPollo.Shared.DTO;
using System.Linq;
using System.Threading.Tasks;

namespace PolloPollo.Services
{
    public interface IProductRepository
    {
        Task<ProductDTO> CreateAsync(ProductCreateDTO dto);
        Task<ProductDTO> FindAsync(int productId);
        IQueryable<ProductDTO> Read();
        IQueryable<ProductDTO> Read(int producerId);
        Task<bool> UpdateAsync(ProductUpdateDTO dto);
        Task<string> UpdateImageAsync(int productId, IFormFile image);
    }
}
