using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Repository
{
    public interface IProductRepository
    {
        Task<ProductDTO> CreateAsync(ProductCreateDTO dto);
    }
}
