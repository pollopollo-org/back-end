using System;
using System.Threading.Tasks;
using PolloPollo.Entities;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
                Status = ApplicationStatus.Open
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

            var applicationDTO = new ApplicationDTO
            {
                ApplicationId = application.Id,
                UserId = application.UserId,
                ProductId = application.ProductId,
                Motivation = application.Motivation,
                TimeStamp = application.TimeStamp,
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
                                     UserId = a.UserId,
                                     ProductId = a.ProductId,
                                     Motivation = a.Motivation,
                                     TimeStamp = a.TimeStamp,
                                     Status = a.Status
                                 }).SingleOrDefaultAsync();

            if (application == null)
            {
                return null;
            }

            return application;
        }

        /// <summary>
        /// Retrieve all open applications
        /// </summary>
        /// <returns></returns>
        public IQueryable<ApplicationDTO> ReadOpen()
        {
            var entities = from a in _context.Applications
                           where a.Status == ApplicationStatus.Open
                           select new ApplicationDTO
                           {
                               ApplicationId = a.Id,
                               UserId = a.UserId,
                               ProductId = a.Id,
                               Motivation = a.Motivation,
                               TimeStamp = a.TimeStamp,
                               Status = a.Status
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
                           select new ApplicationDTO
                           {
                               ApplicationId = a.Id,
                               UserId = a.UserId,
                               ProductId = a.ProductId,
                               Motivation = a.Motivation,
                               TimeStamp = a.TimeStamp,
                               Status = a.Status
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

            if (application.Status != ApplicationStatus.Open) {
                return false;
            }

            _context.Applications.Remove(application);

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
