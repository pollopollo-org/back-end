using PolloPollo.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PolloPollo.Services.Utils;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly PolloPolloContext _context;
        private readonly IImageWriter _imageWriter;
        private readonly string folder;

        public ProductRepository(IImageWriter imageWriter, PolloPolloContext context)
        {
            _imageWriter = imageWriter;
            _context = context;
            folder = "static";
        }

        /// <summary>
        /// Create product from ProductCreateDTO and return a ProductDTO
        /// </summary>
        public async Task<ProductDTO> CreateAsync(ProductCreateDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            var product = new Product
            {
                Title = dto.Title,
                UserId = dto.UserId,
                Price = dto.Price,
                Description = dto.Description,
                Country = dto.Country,
                Location = dto.Location,
                Available = true,
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
                UserId = dto.UserId,
                Price = dto.Price,
                Country = dto.Country,
                Description = dto.Description,
                Location = dto.Location,
                Available = product.Available,
            };

            return productDTO;
        }

        /// <summary>
        /// Find a product by id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns name="ProductDTO"></returns>
        public async Task<ProductDTO> FindAsync(int productId)
        {
            var product = await (from p in _context.Products
                                     where p.Id == productId
                                     select new ProductDTO
                                     {
                                         ProductId = p.Id,
                                         Title = p.Title,
                                         UserId = p.UserId,
                                         Price = p.Price,
                                         Description = p.Description,
                                         Country = p.Country,
                                         Thumbnail = $"{folder}/{p.Thumbnail}",
                                         Location = p.Location,
                                         Available = p.Available
                                     }).SingleOrDefaultAsync();

            if (product == null)
            {
                return null;
            }

            return product;
        }

        /// <summary>
        /// Retrieve all products
        /// </summary>
        /// <returns></returns>
        public IQueryable<ProductDTO> Read()
        {
            var entities = from p in _context.Products
                           where p.Available == true
                           select new ProductDTO
                           {
                               ProductId = p.Id,
                               Title = p.Title,
                               UserId = p.UserId,
                               Price = p.Price,
                               Country = p.Country,
                               Thumbnail = $"{folder}/{p.Thumbnail}",
                               Description = p.Description,
                               Location = p.Location,
                               Available = p.Available
                           };

            return entities;
        }

        public async Task<bool> UpdateAsync(ProductUpdateDTO dto)
        {
            var product = await _context.Products.FindAsync(dto.Id);

            if (product == null)
            {
                return false;
            }

            // Only allow update of availibility
            product.Available = dto.Available;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> UpdateImageAsync(string folder, int id, IFormFile image)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return null;
            }

            var oldThumbnail = product.Thumbnail;

            try
            {
                var fileName = await _imageWriter.UploadImageAsync(folder, image);

                product.Thumbnail = fileName;

                await _context.SaveChangesAsync();

                // Remove old image
                if (oldThumbnail != null)
                {
                    _imageWriter.DeleteImage(folder, oldThumbnail);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        /// <summary>
        /// Retrieve all products by specified producer
        /// </summary>
        /// <param name="producerId"></param>
        /// <returns></returns>
        public IQueryable<ProductDTO> Read(int producerId)
        {
            var entities = from p in _context.Products
                           where p.UserId == producerId
                           select new ProductDTO
                           {
                               ProductId = p.Id,
                               Title = p.Title,
                               UserId = p.UserId,
                               Price = p.Price,
                               Country = p.Country,
                               Thumbnail = $"{folder}/{p.Thumbnail}",
                               Description = p.Description,
                               Location = p.Location,
                               Available = p.Available
                           };

            return entities;
        }
    }
}
