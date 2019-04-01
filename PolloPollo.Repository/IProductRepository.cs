using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Repository
{
    public interface IProductRepository
    {
        Task<ProductDTO> CreateAsync(ProductCreateDTO dto);
        Task<ProductDTO> FindAsync(int productId);
        IQueryable<ProductDTO> Read();
        IQueryable<ProductDTO> Read(int producerId);
        Task<bool> UpdateAsync(ProductUpdateDTO dto);
    }
}
