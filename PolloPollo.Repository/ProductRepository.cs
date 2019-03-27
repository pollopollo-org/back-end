using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PolloPollo.Repository
{
    public class ProductRepository
    {
        private readonly PolloPolloContext _context;

        public ProductRepository(PolloPolloContext context)
        {
            _context = context;
        }

        /*
         * Create product from ProductCreateDTO and return a ProductDTO
         */
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

        /*
         * Find a product by id
         */
        public async Task<ProductDTO> FindAsync(int productId)
        {
            var product = await (from p in _context.Products
                                     where p.Id == productId
                                     select new ProductDTO
                                     {
                                         ProductId = p.Id,
                                         Title = p.Title,
                                         ProducerId = p.ProducerId,
                                         Price = p.Price,
                                         Description = p.Description,
                                         Location = p.Location,
                                         Available = p.Available
                                     }).SingleOrDefaultAsync();

            if (product == null)
            {
                return null;
            }

            return product;
        }

        /*
         * Retrieve all products
         */
        public IQueryable<ProductDTO> Read()
        {
            var entities = from p in _context.Products
                           where p.Available == true
                           select new ProductDTO
                           {
                               ProductId = p.Id,
                               Title = p.Title,
                               ProducerId = p.ProducerId,
                               Price = p.Price,
                               Description = p.Description,
                               Location = p.Location,
                               Available = p.Available
                           };

            return entities;
        }

    }
}
