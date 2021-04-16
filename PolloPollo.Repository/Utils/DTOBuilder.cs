using System;
using PolloPollo.Entities;
using PolloPollo.Shared;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Services.Utils
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
                WalletAddress = dto.WalletAddress
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
    }
}