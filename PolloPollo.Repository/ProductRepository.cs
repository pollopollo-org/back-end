using PolloPollo.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PolloPollo.Services.Utils;
using PolloPollo.Shared.DTO;
using PolloPollo.Shared;

namespace PolloPollo.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly PolloPolloContext _context;
        private readonly IImageWriter _imageWriter;

        public ProductRepository(IImageWriter imageWriter, PolloPolloContext context)
        {
            _imageWriter = imageWriter;
            _context = context;
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
                Rank = dto.Rank,
                TimeStamp = DateTime.UtcNow
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
                Rank = dto.Rank,
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
                                         Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(p.Thumbnail),
                                         Location = p.Location,
                                         Available = p.Available,
                                         Rank = p.Rank,
                                         OpenApplications = p.Applications
                                           .Where(a => a.Status == ApplicationStatusEnum.Open)
                                           .Count(),
                                        PendingApplications = p.Applications
                                           .Where(a => a.Status == ApplicationStatusEnum.Pending)
                                           .Count(),
                                        ClosedApplications = p.Applications
                                           .Where(a => a.Status == ApplicationStatusEnum.Closed)
                                           .Count(),
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
        public IQueryable<ProductDTO> ReadOpen()
        {
            var entities = from p in _context.Products
                           where p.Available == true
                           orderby p.Rank descending
                           orderby p.TimeStamp descending
                           select new ProductDTO
                           {
                               ProductId = p.Id,
                               Title = p.Title,
                               UserId = p.UserId,
                               Price = p.Price,
                               Country = p.Country,
                               Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(p.Thumbnail),
                               Description = p.Description,
                               Location = p.Location,
                               Available = p.Available,
                               Rank = p.Rank,
                               OpenApplications = p.Applications
                                 .Where(a => a.Status == ApplicationStatusEnum.Open)
                                 .Count(),
                               PendingApplications = p.Applications
                                 .Where(a => a.Status == ApplicationStatusEnum.Pending)
                                 .Count(),
                               ClosedApplications = p.Applications
                                 .Where(a => a.Status == ApplicationStatusEnum.Closed)
                                 .Count(),
                           };

            return entities;
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
                           orderby p.Rank descending
                           orderby p.TimeStamp descending
                           select new ProductDTO
                           {
                               ProductId = p.Id,
                               Title = p.Title,
                               UserId = p.UserId,
                               Price = p.Price,
                               Country = p.Country,
                               Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(p.Thumbnail),
                               Description = p.Description,
                               Location = p.Location,
                               Available = p.Available,
                               Rank = p.Rank,
                               OpenApplications = p.Applications
                                .Where(a => a.Status == ApplicationStatusEnum.Open)
                                .Count(),
                               PendingApplications = p.Applications
                                .Where(a => a.Status == ApplicationStatusEnum.Pending)
                                .Count(),
                               ClosedApplications = p.Applications
                                .Where(a => a.Status == ApplicationStatusEnum.Closed)
                                .Count(),
                           };

            return entities;
        }

        public async Task<(bool status, int pendingApplications)> UpdateAsync(ProductUpdateDTO dto)
        {
            var pendingApplications = 0;

            var product = await _context.Products.
                Include(p => p.Applications).
                FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (product == null)
            {
                return (false, pendingApplications);
            }

            foreach (var application in product.Applications)
            {
                if (application.Status == ApplicationStatusEnum.Open)
                {
                    application.Status = ApplicationStatusEnum.Closed;
                }
                else if (application.Status == ApplicationStatusEnum.Pending)
                {
                    pendingApplications++;
                }
            }

            product.Available = dto.Available;

            await _context.SaveChangesAsync();

            return (true, pendingApplications);
        }

        public async Task<string> UpdateImageAsync(int id, IFormFile image)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return null;
            }

            var folder = ImageFolderEnum.@static.ToString();

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

                return ImageHelper.GetRelativeStaticFolderImagePath(fileName);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// Retrieve count of product
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            return await _context.Products.CountAsync();
        }


    }
}
