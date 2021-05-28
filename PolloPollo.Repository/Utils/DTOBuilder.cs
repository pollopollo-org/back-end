using System;
using PolloPollo.Entities;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PolloPollo.Repository.Utils
{
    public class DTOBuilder
    {
        public static DetailedUserDTO CreateDetailedUserDTO(UserCreateDTO dto)
        {
            return new DetailedUserDTO
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                SurName = dto.SurName,
                Country = dto.Country,
            };
        }

        public static DetailedProducerDTO CreateDetailedProducerDTO(UserCreateDTO dto, Producer producer, string deviceAddress, string obyteHub)
        {
            return new DetailedProducerDTO
            {
                UserId = producer.UserId,
                Email = dto.Email,
                FirstName = dto.FirstName,
                SurName = dto.SurName,
                Country = dto.Country,

                // Set user role on DTO
                UserRole = UserRoleEnum.Producer.ToString(),

                // Get pairing link for OByte wallet immediately.
                PairingLink = !string.IsNullOrEmpty(producer.PairingSecret)
                ? "byteball:" + deviceAddress + "@" + obyteHub + "#" + producer.PairingSecret
                : default(string),
                Street = dto.Street,
                StreetNumber = dto.StreetNumber,
                Zipcode = dto.Zipcode,
                City = dto.City
            };
        }

        public static DetailedDonorDTO CreateDetailedDonorDTO(DonorDTO dto)
        {
            return new DetailedDonorDTO
            {
                AaAccount = dto.AaAccount,
                UID = dto.UID,
                Email = dto.Email,
                DeviceAddress = dto.DeviceAddress,
                WalletAddress = dto.WalletAddress,
                UserRole = UserRoleEnum.Donor.ToString()
            };
        }

        public static User CreateUser(UserCreateDTO dto, string pwd)
        {
            return new User
            {
                FirstName = dto.FirstName,
                SurName = dto.SurName,
                Email = dto.Email,
                Country = dto.Country,
                Created = DateTime.UtcNow,
                Password = pwd
            };
        }

        public static Producer CreateProducer(UserCreateDTO dto, int userid, string pairingSecret)
        {
            return new Producer
            {
                UserId = userid,
                PairingSecret = pairingSecret,
                Street = dto.Street,
                StreetNumber = dto.StreetNumber,
                Zipcode = dto.Zipcode,
                City = dto.City
            };
        }

        public static Receiver CreateReceiver(int userid)
        {
            return new Receiver
            {
                UserId = userid
            };
        }

        public static UserRole CreateProducerUserRole(int userid)
        {
            return new UserRole
            {
                UserId = userid,
                UserRoleEnum = UserRoleEnum.Producer
            };
        }

        public static UserRole CreateReceiverUserRole(int userid)
        {
            return new UserRole
            {
                UserId = userid,
                UserRoleEnum = UserRoleEnum.Receiver
            };
        }

        public static TokenDTO CreateTokenDTO(DetailedUserDTO dto, string token)
        {
            return new TokenDTO
            {
                UserDTO = dto,
                Token = token
            };
        }

        public enum ApplicationDTOType
        {
            FIND,
            READ,
            READCHECK,
            OPEN
        }

        public static ApplicationDTO CreateApplicationDTO(Application a, IPolloPolloContext _context, ApplicationDTOType type)
        {
            switch (type)
            {
                case ApplicationDTOType.OPEN:
                    return CreateReadOpenAppDTO(a);
                case ApplicationDTOType.FIND:
                    return CreateFindAppDTO(a);
                case ApplicationDTOType.READ:
                    return CreateReadAppDTO(a, _context);
                case ApplicationDTOType.READCHECK:
                    return CreateReadAppDTOCheck(a, _context);
            }
            return null;
        }

        private static ApplicationDTO CreateFindAppDTO(Application a)
        {
            return new ApplicationDTO
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
                DateOfDonation = a.DateOfDonation.ToString("yyyy-MM-dd"),
                CreationDate = a.Created.ToString("yyyy-MM-dd HH:mm:ss"),
            };
        }

        private static ApplicationDTO CreateReadOpenAppDTO(Application a)
        {
            return new ApplicationDTO
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
                CreationDate = a.Created.ToString("yyyy-MM-dd HH:mm:ss"),
            };
        }

        private static ApplicationDTO CreateReadAppDTO(Application a, IPolloPolloContext _context)
        {
            return new ApplicationDTO
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
                Bytes = (from c in _context.Contracts
                        where a.Id == c.ApplicationId
                        select c.Bytes
                                    ).FirstOrDefault(),
                BytesInCurrentDollars = BytesToUSDConverter.BytesToUSD(
                                                        (from c in _context.Contracts
                                                            where a.Id == c.ApplicationId
                                                            select c.Bytes
                                                        ).FirstOrDefault(),
                                                        (from b in _context.ByteExchangeRate
                                                            where b.Id == 1
                                                            select b.GBYTE_USD).FirstOrDefault()
                                                    ),
                ContractSharedAddress = (from c in _context.Contracts
                                        where c.ApplicationId == a.Id
                                        select c.SharedAddress
                                            ).FirstOrDefault(),
                Status = a.Status,
                CreationDate = a.Created.ToString("yyyy-MM-dd HH:mm:ss"),
                DateOfDonation = a.DateOfDonation.ToString("yyyy-MM-dd"),
            };
        }

        private static ApplicationDTO CreateReadAppDTOCheck(Application a, IPolloPolloContext _context)
        {
            Func<ApplicationStatusEnum, bool> checkStatus = status => status == ApplicationStatusEnum.Completed || status == ApplicationStatusEnum.Pending;

            return new ApplicationDTO
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
                Bytes = checkStatus(a.Status) ?
                        (from c in _context.Contracts
                        where a.Id == c.ApplicationId
                        select c.Bytes
                                    ).FirstOrDefault()
                        : 0,
                BytesInCurrentDollars = checkStatus(a.Status) ?
                        BytesToUSDConverter.BytesToUSD(
                            (from c in _context.Contracts
                                where a.Id == c.ApplicationId
                                select c.Bytes
                            ).FirstOrDefault(),
                            (from b in _context.ByteExchangeRate
                                where b.Id == 1
                                select b.GBYTE_USD).FirstOrDefault()
                        )
                        : 0,
                ContractSharedAddress = checkStatus(a.Status) ?
                        (from c in _context.Contracts
                        where c.ApplicationId == a.Id
                        select c.SharedAddress
                        ).FirstOrDefault()
                        : null,
                Status = a.Status,
                DateOfDonation = a.DateOfDonation.ToString("yyyy-MM-dd"),
                CreationDate = a.Created.ToString("yyyy-MM-dd HH:mm:ss"),
            };
        }
    }
}
