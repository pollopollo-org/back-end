using PolloPollo.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PolloPollo.Services.Utils;
using PolloPollo.Shared.DTO;
using PolloPollo.Shared;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Collections.Generic;

namespace PolloPollo.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly PolloPolloContext _context;
        private readonly IImageWriter _imageWriter;
        private readonly IEmailClient _emailClient;

        public ProductRepository(IImageWriter imageWriter, IEmailClient emailClient, PolloPolloContext context)
        {
            _imageWriter = imageWriter;
            _emailClient = emailClient;
            _context = context;
        }

        /// <summary>
        /// Create product from ProductCreateDTO and return a ProductDTO
        /// </summary>
        public async Task<(ProductDTO created, string message)> CreateAsync(ProductCreateDTO dto)
        {
            if (dto == null)
            {
                return (null, "Empty DTO");
            }

            var producerUser = await (from p in _context.Users
                                      where p.Id == dto.UserId
                                      select new
                                      {
                                          p.Producer.WalletAddress
                                      }).FirstOrDefaultAsync();

            if (producerUser == null)
            {
                return (null, "Producer not found");
            }
            else if (string.IsNullOrEmpty(producerUser.WalletAddress))
            {
                return (null, "No wallet address");
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
                Created = DateTime.UtcNow
            };

            try
            {
                var createdProduct = _context.Products.Add(product);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return (null, "Error");
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

            return (productDTO, "Created");
        }

        /// <summary>
        /// Find a product by id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns name="ProductDTO"></returns>
        public async Task<ProductDTO> FindAsync(int productId)
        {
            var weekAgo = DateTime.UtcNow.Subtract(new TimeSpan(7, 0, 0, 0));
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0));

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
                                     // Stats
                                     DateLastDonation = p.Applications.Count == 0
                                                    ? null
                                                    : p.Applications.Select(a => a.DateOfDonation).DefaultIfEmpty(DateTime.MinValue).Max().Equals(DateTime.MinValue)
                                                        ? null
                                                        : p.Applications.Select(a => a.DateOfDonation).DefaultIfEmpty(DateTime.MinValue).Max().ToString("yyyy-MM-dd HH':'mm"),
                                     CompletedDonationsPastWeek =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Completed && a.LastModified >= weekAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                                     CompletedDonationsPastMonth =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Completed && a.LastModified >= monthAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                                     CompletedDonationsAllTime =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Completed
                                         select new
                                         { ApplicationId = a.Id }).Count(),
                                     PendingDonationsPastWeek =
                                       (from a in p.Applications
                                        where (a.Status == ApplicationStatusEnum.Pending) && a.LastModified >= weekAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                                     PendingDonationsPastMonth =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Pending && a.LastModified >= monthAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                                     PendingDonationsAllTime =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Pending
                                         select new
                                         { ApplicationId = a.Id }).Count(),
                                     // Lists of applications of status x
                                     OpenApplications =
                                        from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Open
                                        select new ApplicationDTO
                                        {
                                            ApplicationId = a.Id,
                                            ReceiverId = a.UserId,
                                            ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                            Country = a.User.Country,
                                            Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                            ProductId = a.Product.Id,
                                            ProductTitle = a.Product.Title,
                                            ProductPrice = a.Product.Price,
                                            ProducerId = a.Product.UserId,
                                            Motivation = a.Motivation,
                                            Status = a.Status,
                                        },
                                     PendingApplications =
                                        from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Pending
                                        select new ApplicationDTO
                                        {
                                            ApplicationId = a.Id,
                                            ReceiverId = a.UserId,
                                            ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                            Country = a.User.Country,
                                            Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                            ProductId = a.Product.Id,
                                            ProductTitle = a.Product.Title,
                                            ProductPrice = a.Product.Price,
                                            ProducerId = a.Product.UserId,
                                            Motivation = a.Motivation,
                                            Status = a.Status,
                                        },
                                     ClosedApplications =
                                             from a in p.Applications
                                             where a.Status == ApplicationStatusEnum.Unavailable
                                             select new ApplicationDTO
                                             {
                                                 ApplicationId = a.Id,
                                                 ReceiverId = a.UserId,
                                                 ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                                 Country = a.User.Country,
                                                 Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                                 ProductId = a.Product.Id,
                                                 ProductTitle = a.Product.Title,
                                                 ProductPrice = a.Product.Price,
                                                 ProducerId = a.Product.UserId,
                                                 Motivation = a.Motivation,
                                                 Status = a.Status,
                                             },
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
            var weekAgo = DateTime.UtcNow.Subtract(new TimeSpan(7, 0, 0, 0));
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0));

            var entities = from p in _context.Products
                           where p.Available == true
                           orderby p.Rank descending
                           orderby p.Created descending
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
                               // Stats
                               DateLastDonation = p.Applications.Count == 0
                                                    ? null
                                                    : p.Applications.Select(a => a.DateOfDonation).DefaultIfEmpty(DateTime.MinValue).Max().Equals(DateTime.MinValue)
                                                        ? null
                                                        : p.Applications.Select(a => a.DateOfDonation).DefaultIfEmpty(DateTime.MinValue).Max().ToString("yyyy-MM-dd HH':'mm"),
                               CompletedDonationsPastWeek =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Completed && a.LastModified >= weekAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               CompletedDonationsPastMonth =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Completed && a.LastModified >= monthAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               CompletedDonationsAllTime =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Completed
                                         select new
                                         { ApplicationId = a.Id }).Count(),
                               PendingDonationsPastWeek =
                                       (from a in p.Applications
                                        where (a.Status == ApplicationStatusEnum.Pending) && a.LastModified >= weekAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               PendingDonationsPastMonth =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Pending && a.LastModified >= monthAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               PendingDonationsAllTime =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Pending
                                         select new
                                         { ApplicationId = a.Id }).Count(),
                               // Applications for the product of status x
                               OpenApplications =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Open
                                         select new ApplicationDTO
                                         {
                                             ApplicationId = a.Id,
                                             ReceiverId = a.UserId,
                                             ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                             Country = a.User.Country,
                                             Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                             ProductId = a.Product.Id,
                                             ProductTitle = a.Product.Title,
                                             ProductPrice = a.Product.Price,
                                             ProducerId = a.Product.UserId,
                                             Motivation = a.Motivation,
                                             Status = a.Status,
                                         }).ToList(),
                               PendingApplications =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Pending
                                         select new ApplicationDTO
                                         {
                                             ApplicationId = a.Id,
                                             ReceiverId = a.UserId,
                                             ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                             Country = a.User.Country,
                                             Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                             ProductId = a.Product.Id,
                                             ProductTitle = a.Product.Title,
                                             ProductPrice = a.Product.Price,
                                             ProducerId = a.Product.UserId,
                                             Motivation = a.Motivation,
                                             Status = a.Status,
                                         }).ToList(),
                               ClosedApplications =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Unavailable
                                         select new ApplicationDTO
                                         {
                                             ApplicationId = a.Id,
                                             ReceiverId = a.UserId,
                                             ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                             Country = a.User.Country,
                                             Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                             ProductId = a.Product.Id,
                                             ProductTitle = a.Product.Title,
                                             ProductPrice = a.Product.Price,
                                             ProducerId = a.Product.UserId,
                                             Motivation = a.Motivation,
                                             Status = a.Status,
                                         }).ToList(),
                           };

            return entities;
        }

        /// <summary>
        /// Retrieve all products
        /// </summary>
        /// <returns></returns>
        public IQueryable<ProductDTO> ReadFiltered(string Country = "ALL", string City = "ALL")
        {
            var weekAgo = DateTime.UtcNow.Subtract(new TimeSpan(7, 0, 0, 0));
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0));

            var entities = from p in _context.Products
                           where p.Available == true
                           where _context.Users.Where(u => u.Id == p.UserId).Select(u => u.Country).First().Equals(Country) || Country.Equals("ALL")
                           where _context.Producers.Where(u => u.UserId == p.UserId).Select(u => u.City).First().Equals(City) || City.Equals("ALL")
                           orderby p.Rank descending
                           orderby p.Created descending
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
                               // Stats
                               DateLastDonation = p.Applications.Count == 0
                                                    ? null
                                                    : p.Applications.Select(a => a.DateOfDonation).DefaultIfEmpty(DateTime.MinValue).Max().Equals(DateTime.MinValue)
                                                        ? null
                                                        : p.Applications.Select(a => a.DateOfDonation).DefaultIfEmpty(DateTime.MinValue).Max().ToString("yyyy-MM-dd HH':'mm"),
                               CompletedDonationsPastWeek =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Completed && a.LastModified >= weekAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               CompletedDonationsPastMonth =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Completed && a.LastModified >= monthAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               CompletedDonationsAllTime =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Completed
                                         select new
                                         { ApplicationId = a.Id }).Count(),
                               PendingDonationsPastWeek =
                                       (from a in p.Applications
                                        where (a.Status == ApplicationStatusEnum.Pending) && a.LastModified >= weekAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               PendingDonationsPastMonth =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Pending && a.LastModified >= monthAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               PendingDonationsAllTime =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Pending
                                         select new
                                         { ApplicationId = a.Id }).Count(),
                               // Applications for the product of status x
                               OpenApplications =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Open
                                         select new ApplicationDTO
                                         {
                                             ApplicationId = a.Id,
                                             ReceiverId = a.UserId,
                                             ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                             Country = a.User.Country,
                                             Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                             ProductId = a.Product.Id,
                                             ProductTitle = a.Product.Title,
                                             ProductPrice = a.Product.Price,
                                             ProducerId = a.Product.UserId,
                                             Motivation = a.Motivation,
                                             Status = a.Status,
                                         }).ToList(),
                               PendingApplications =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Pending
                                         select new ApplicationDTO
                                         {
                                             ApplicationId = a.Id,
                                             ReceiverId = a.UserId,
                                             ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                             Country = a.User.Country,
                                             Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                             ProductId = a.Product.Id,
                                             ProductTitle = a.Product.Title,
                                             ProductPrice = a.Product.Price,
                                             ProducerId = a.Product.UserId,
                                             Motivation = a.Motivation,
                                             Status = a.Status,
                                         }).ToList(),
                               ClosedApplications =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Unavailable
                                         select new ApplicationDTO
                                         {
                                             ApplicationId = a.Id,
                                             ReceiverId = a.UserId,
                                             ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                             Country = a.User.Country,
                                             Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                             ProductId = a.Product.Id,
                                             ProductTitle = a.Product.Title,
                                             ProductPrice = a.Product.Price,
                                             ProducerId = a.Product.UserId,
                                             Motivation = a.Motivation,
                                             Status = a.Status,
                                         }).ToList(),
                           };

            return entities;
        }

        public async Task<(bool status, int pendingApplications, (bool emailSent, string emailError))> UpdateAsync(ProductUpdateDTO dto)
        {
            var pendingApplications = 0;
            (bool emailSent, string emailError) = (false, null);

            var product = await _context.Products.
                Include(p => p.Applications).
                FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (product == null)
            {
                return (false, pendingApplications, (emailSent, emailError));
            }

            foreach (var application in product.Applications)
            {
                if (application.Status == ApplicationStatusEnum.Open && !dto.Available)
                {
                    application.Status = ApplicationStatusEnum.Unavailable;
                    await _context.SaveChangesAsync();


                    // Send email to receiver informing them that their application has been cancelled
                    var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == application.UserId);
                    var receiverEmail = receiver.Email;
                    var productName = product.Title;
                    (emailSent, emailError) = SendCancelEmail(receiverEmail, productName);


                }
                else if (application.Status == ApplicationStatusEnum.Pending)
                {
                    pendingApplications++;
                }
            }

            product.Available = dto.Available;

            await _context.SaveChangesAsync();

            return (true, pendingApplications, (emailSent, emailError));
        }

        /// <summary>
        /// Send email about cancelled application to receiver
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private (bool sent, string error) SendCancelEmail(string ReceiverEmail, string ProductName)
        {
            string subject = "PolloPollo application cancelled";
            string body = $"You had an open application for {ProductName} but the Producer has removed the product from the PolloPollo platform, and your application for it has therefore been cancelled.You may log on to the PolloPollo platform to see if the product has been replaced by another product, you want to apply for instead.\n\nSincerely,\nThe PolloPollo Project";

            return _emailClient.SendEmail(ReceiverEmail, subject, body);
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
        /// Retrieve all products by specified producer
        /// </summary>
        /// <param name="producerId"></param>
        /// <returns></returns>
        public IQueryable<ProductDTO> Read(int producerId)
        {
            var weekAgo = DateTime.UtcNow.Subtract(new TimeSpan(7, 0, 0, 0));
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0));

            var entities = from p in _context.Products
                           where p.UserId == producerId
                           orderby p.Rank descending
                           orderby p.Created descending
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
                               // Stats
                               DateLastDonation = p.Applications.Count == 0
                                                    ? null
                                                    : p.Applications.Select(a => a.DateOfDonation).DefaultIfEmpty(DateTime.MinValue).Max().Equals(DateTime.MinValue)
                                                        ? null
                                                        : p.Applications.Select(a => a.DateOfDonation).DefaultIfEmpty(DateTime.MinValue).Max().ToString("yyyy-MM-dd HH':'mm"),
                               CompletedDonationsPastWeek =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Completed && a.LastModified >= weekAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               CompletedDonationsPastMonth =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Completed && a.LastModified >= monthAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               CompletedDonationsAllTime =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Completed
                                         select new
                                         { ApplicationId = a.Id }).Count(),
                               PendingDonationsPastWeek =
                                       (from a in p.Applications
                                        where (a.Status == ApplicationStatusEnum.Pending) && a.LastModified >= weekAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               PendingDonationsPastMonth =
                                       (from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Pending && a.LastModified >= monthAgo
                                        select new
                                        { ApplicationId = a.Id }).Count(),
                               PendingDonationsAllTime =
                                        (from a in p.Applications
                                         where a.Status == ApplicationStatusEnum.Pending
                                         select new
                                         { ApplicationId = a.Id }).Count(),
                               // Applications for the product of status x
                               OpenApplications =
                                        from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Open
                                        select new ApplicationDTO
                                        {
                                            ApplicationId = a.Id,
                                            ReceiverId = a.UserId,
                                            ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                            Country = a.User.Country,
                                            Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                            ProductId = a.Product.Id,
                                            ProductTitle = a.Product.Title,
                                            ProductPrice = a.Product.Price,
                                            ProducerId = a.Product.UserId,
                                            Motivation = a.Motivation,
                                            Status = a.Status,
                                        },
                               PendingApplications =
                                        from a in p.Applications
                                        where a.Status == ApplicationStatusEnum.Pending
                                        select new ApplicationDTO
                                        {
                                            ApplicationId = a.Id,
                                            ReceiverId = a.UserId,
                                            ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                            Country = a.User.Country,
                                            Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                            ProductId = a.Product.Id,
                                            ProductTitle = a.Product.Title,
                                            ProductPrice = a.Product.Price,
                                            ProducerId = a.Product.UserId,
                                            Motivation = a.Motivation,
                                            Status = a.Status,
                                        },
                               ClosedApplications =
                                             from a in p.Applications
                                             where a.Status == ApplicationStatusEnum.Unavailable
                                             select new ApplicationDTO
                                             {
                                                 ApplicationId = a.Id,
                                                 ReceiverId = a.UserId,
                                                 ReceiverName = $"{a.User.FirstName} {a.User.SurName}",
                                                 Country = a.User.Country,
                                                 Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(a.User.Thumbnail),
                                                 ProductId = a.Product.Id,
                                                 ProductTitle = a.Product.Title,
                                                 ProductPrice = a.Product.Price,
                                                 ProducerId = a.Product.UserId,
                                                 Motivation = a.Motivation,
                                                 Status = a.Status,
                                             },
                           };

            return entities;
        }

        /// <summary>
        /// Retrieve count of product
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public IQueryable<string> GetCountries()
        {
            var countries = (from p in _context.Products
                             where p.Available
                             select new
                             {
                                 (from u in _context.Users
                                  where u.Id == p.UserId
                                  select new
                                  { u.Country }).First().Country,
                             }).Select(c => c.Country).Distinct().OrderBy(x => x);

            return countries;
        }

        /// <summary>
        /// Get list of cities in a specified country in which available products exist
        /// </summary>
        public IQueryable<string> GetCities(string country)
        {
            var cities = (from p in _context.Products
                          where p.Available && p.Country.Equals(country)
                          select new
                            {
                                (from u in _context.Producers
                                 where u.UserId == p.UserId
                                 select new
                                 { u.City }).First().City,
                            }).Select(c => c.City).Distinct().OrderBy(x => x);

            return cities;
        }
    }
}
