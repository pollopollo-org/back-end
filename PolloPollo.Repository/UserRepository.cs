using Microsoft.Extensions.Options;
using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.IO;

namespace PolloPollo.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly SecurityConfig _config;
        private readonly PolloPolloContext _context;
        private readonly IReceiverRepository _receiverRepo;
        private readonly IProducerRepository _producerRepo;


        public UserRepository(IOptions<SecurityConfig> config, PolloPolloContext context, IProducerRepository producerRepo, IReceiverRepository receiverRepo)
        {
            _config = config.Value;
            _context = context;
            _producerRepo = producerRepo;
            _receiverRepo = receiverRepo;
        }

        public async Task<TokenDTO> CreateAsync(UserCreateDTO dto)
        {
            var userDTO = new UserDTO()
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                Surname = dto.Surname,
                Country = dto.Country
            };

            // Uses a tranction to keep data integrity in case something failes.
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var user = new User
                    {
                        FirstName = dto.FirstName,
                        Surname = dto.Surname,
                        Email = dto.Email,
                        Country = dto.Country,
                        // Important to hash the password
                        Password = HashPassword(dto.Email, dto.Password),
                    };

                    var createdUser = _context.Users.Add(user);

                    await _context.SaveChangesAsync();

                    userDTO.UserId = user.Id;

                    // Add the user to a role and add a foreign key for the ISA relationship
                    // Used to extend the information on a user and give access restrictions
                    switch (dto.Role)
                    {
                        case nameof(UserRoleEnum.Producer):
                            userDTO.UserRole = UserRoleEnum.Producer.ToString();

                            await _producerRepo.CreateAsync(createdUser);

                            break;
                        case nameof(UserRoleEnum.Receiver):
                            userDTO.UserRole = UserRoleEnum.Receiver.ToString();

                            await _receiverRepo.CreateAsync(createdUser);

                            break;
                        default:
                            // This should never happen
                            break;
                    }

                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if any of the save commands fails
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Could also throw an exception for more information when failing the user creation
                    return null;
                }
            }

            // Return the user information along with an authorized tokens
            // To login the user after creation
            var tokenDTO = new TokenDTO
            {
                UserDTO = userDTO,
                Token = Authenticate(dto.Email, dto.Password),
            };

            return tokenDTO;
        }

        public async Task<UserDTO> FindAsync(int userId)
        {
            // Fetches all the information for a user
            // Creates a complete profile with every property
            var fullUser = await (from u in _context.Users
                      where u.Id == userId
                      where u.UserRole.UserId == userId
                      let role = u.UserRole.UserRoleEnum
                      select new
                      {
                          UserId = u.Id,
                          UserRole = role,
                          Wallet = role == UserRoleEnum.Producer ?
                                    u.Producer.Wallet
                                    : default(string),
                          u.FirstName,
                          u.Surname,
                          u.Email,
                          u.Country,
                          u.Description,
                          u.City,
                          u.Thumbnail,
                      }).SingleOrDefaultAsync();

            if (fullUser == null)
            {
                return null;
            }

            // Filter out the information based on the role
            // To only send back the profile information for the specific role
            switch (fullUser.UserRole)
            {
                case UserRoleEnum.Producer:
                    return new ProducerDTO
                    {
                        UserId = fullUser.UserId,
                        Wallet = fullUser.Wallet,
                        FirstName = fullUser.FirstName,
                        Surname = fullUser.Surname,
                        Email = fullUser.Email,
                        Country = fullUser.Country,
                        Description = fullUser.Description,
                        City = fullUser.City,
                        Thumbnail = fullUser.Thumbnail,
                        UserRole = fullUser.UserRole.ToString()
                    };
                case UserRoleEnum.Receiver:
                    return new ReceiverDTO
                    {
                        UserId = fullUser.UserId,
                        FirstName = fullUser.FirstName,
                        Surname = fullUser.Surname,
                        Email = fullUser.Email,
                        Country = fullUser.Country,
                        Description = fullUser.Description,
                        City = fullUser.City,
                        Thumbnail = fullUser.Thumbnail,
                        UserRole = fullUser.UserRole.ToString()
                    };
                default:
                    // Default to the super class with minimal information
                    return new UserDTO {
                        UserId = fullUser.UserId,
                        FirstName = fullUser.FirstName,
                        Surname = fullUser.Surname,
                        Email = fullUser.Email,
                        Country = fullUser.Country,
                        Description = fullUser.Description,
                        City = fullUser.City,
                        Thumbnail = fullUser.Thumbnail,
                        UserRole = fullUser.UserRole.ToString()
                    };
            }
        }

        public async Task<bool> UpdateAsync(UserUpdateDTO dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);

            // Return null if user not found or password don't match
            if (user == null || !user.Password.Equals(dto.Password))
            {
                return false;
            }

            // Update user
            user.FirstName = dto.FirstName;
            user.Surname = dto.Surname;
            user.Email = dto.Email;
            user.Thumbnail = await StoreImageAsync(dto.Thumbnail);
            user.Country = dto.Country;
            user.City = dto.City;
            user.Description = dto.Description;

            // If new password is not null, hash the new password and update
            // the users password
            if (dto.NewPassword != null)
            {
                user.Password = HashPassword(dto.Email, dto.NewPassword);
            }

            switch (user.UserRole.UserRoleEnum)
            {
                case UserRoleEnum.Producer:
                    var producerDTO = (ProducerUpdateDTO)dto;

                    // Fields specified for producer is updated here
                    user.Producer.Wallet = producerDTO.Wallet;
                    break;
                case UserRoleEnum.Receiver:
                    var receiverDTO = (ReceiverUpdateDTO)dto;

                    // Fields specified for receiver is updated here
                    break;
                default:
                    break;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Helper that stores a password on the filesystem and returns a string that specifies where the file has
        /// been stored
        /// </summary>
        public async Task<string> StoreImageAsync(IFormFile file)
        {
            // Get the base path of where images should be saved
            var basePath = Path.Combine(ApplicationRoot.getWebRoot(), "static");

            // If the file is empty, then we assume it cannot be saved
            if (file?.Length > 0)
            {
                using (var imageReadStream = new MemoryStream())
                {
                    try
                    {
                        // Insert the image into a memory stream
                        await file.CopyToAsync(imageReadStream);

                        // ... and attempt to convert it to an image
                        using (var potentialImage = Image.FromStream(imageReadStream))
                        {
                            // If we get here, then we have a valid image which we can safely store
                            var fileName = DateTime.Now.Ticks + "_" + file.FileName;
                            var filePath = Path.Combine(basePath, fileName);
                            potentialImage.Save(filePath);

                            // Return the absolute path to the image
                            return $"./static/{fileName}";
                        }
                    }

                    // If we get here, then the image couldn't be read as an image, bail out immediately!
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }


        public string Authenticate(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(x => x.Email == email);

            // return null if user not found
            if (user == null)
                return null;

            var validPassword = VerifyPassword(user.Email, user.Password, password);

            // if password is invalid, then bail out as well
            if (!validPassword)
            {
                return null;
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Import HmacSHa256 key to be used for creating a unique signing of the token
            // Defined in appsettings
            var key = Encoding.ASCII.GetBytes(_config.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    // Add information to Claim
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.Surname}"),
                    new Claim(ClaimTypes.Email, user.Email),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                // Add unique signature signing to Token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


        /// <summary>
        /// Internal helper that hashes a given password to prepare it for storing in the database
        /// </summary>
        public string HashPassword(string email, string password)
        {
            var hasher = new PasswordHasher<string>();

            return hasher.HashPassword(email, password);
        }

        /// <summary>
        /// Internal helper that verifies if a given password matches the hashed password of a user stored in the database
        /// </summary>
        public bool VerifyPassword(string email, string password, string plainPassword)
        {
            var hasher = new PasswordHasher<string>();

            var result = hasher.VerifyHashedPassword(email, password, plainPassword);
            return (
                result == PasswordVerificationResult.Success ||
                result == PasswordVerificationResult.SuccessRehashNeeded
            );
        }

    }
}
