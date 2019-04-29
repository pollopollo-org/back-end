using System;
using System.Threading.Tasks;
using PolloPollo.Entities;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PolloPollo.Services.Utils;

namespace PolloPollo.Services
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly PolloPolloContext _context;

        public ApplicationRepository(PolloPolloContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create application from ApplicationCreateDTO and return an ApplicationDTO
        /// </summary>
        public async Task<ApplicationDTO> CreateAsync(ApplicationCreateDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            var application = new Application
            {
                UserId = dto.UserId,
                ProductId = dto.ProductId,
                Motivation = dto.Motivation,
                TimeStamp = DateTime.UtcNow,
                Status = ApplicationStatusEnum.Open
            };

            try
            {
                var createdApplication = _context.Applications.Add(application);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return null;
            }

            var receiver = (from u in _context.Users
                            where u.Id == application.UserId
                            select new
                            {
                                ReceiverName = u.FirstName + " " + u.SurName,
                                u.Country,
                                u.Thumbnail
                            }).FirstOrDefault();

            var product = (from p in _context.Products
                           where p.Id == application.ProductId
                           select new 
                           {
                               ProductId = p.Id,
                               ProductTitle = p.Title,
                               ProductPrice = p.Price,
                               ProducerId = p.UserId
                           }).FirstOrDefault();

            var applicationDTO = new ApplicationDTO
            {
                ApplicationId = application.Id,
                ReceiverId = application.UserId,
                ReceiverName = receiver.ReceiverName,
                Country = receiver.Country,
                Thumbnail = receiver.Thumbnail,
                ProductId = product.ProductId,
                ProductTitle = product.ProductTitle,
                ProductPrice = product.ProductPrice,
                ProducerId = product.ProducerId,
                Motivation = application.Motivation,
                Status = application.Status,
            };

            return applicationDTO;
        }      

        /// <summary>
        /// Find an application by id
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns name="ApplicationDTO"></returns>
        public async Task<ApplicationDTO> FindAsync(int applicationId)
        {
            var application = await (from a in _context.Applications
                                 where a.Id == applicationId
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
                                 }).SingleOrDefaultAsync();

            if (application == null)
            {
                return null;
            }

            return application;
        }

        /// <summary>
        /// Update state of application
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(ApplicationUpdateDTO dto)
        {
            var application = await _context.Applications.
                FirstOrDefaultAsync(p => p.Id == dto.ApplicationId);

            if (application == null)
            {
                return false;
            }

            if (dto.ReceiverId != application.UserId)
            {
                return false;
            }


            application.Status = dto.Status;

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Retrieve all open applications
        /// </summary>
        /// <returns></returns>
        public IQueryable<ApplicationDTO> ReadOpen()
        {
            var entities = from a in _context.Applications
                           where a.Status == ApplicationStatusEnum.Open
                           orderby a.TimeStamp descending
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
                           };

            return entities;
        }

        /// <summary>
        /// Retrieve all applications by specified receiver
        /// </summary>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public IQueryable<ApplicationDTO> Read(int receiverId)
        {
            var entities = from a in _context.Applications
                           where a.UserId == receiverId
                           orderby a.TimeStamp descending
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
                           };

            return entities;
        }

        /// <summary>
        /// Delete an application by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns name="bool"></returns>
        public async Task<bool> DeleteAsync(int userId, int id)
        {
            var application = _context.Applications.Find(id);

            if (application == null)
            {
                return false;
            }

            if (userId != application.UserId)
            {
                return false;
            }

            if (application.Status != ApplicationStatusEnum.Open) {
                return false;
            }

            _context.Applications.Remove(application);

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
