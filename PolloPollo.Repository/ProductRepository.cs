using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly PolloPolloContext _context;

        public ProductRepository(PolloPolloContext context)
        {
            _context = context;
        }

        public async Task<ProductDTO> CreateAsync(ProductCreateDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            var product = new Product
            {
                Title = dto.Title,
                ProducerId = dto.ProducerId,
                Price = dto.Price,
                Description = dto.Description,
                Location = dto.Location,
                Available = dto.Available,
            };
            try
            {
                var createdProduct = _context.Products.Add(product);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return null;
            }

            var productDTO = new ProductDTO
            {
                ProductId = product.Id,
                Title = dto.Title,
                ProducerId = dto.ProducerId,
                Price = dto.Price,
                Description = dto.Description,
                Location = dto.Location,
                Available = dto.Available,
            };

            return productDTO;
        }
    }
}
