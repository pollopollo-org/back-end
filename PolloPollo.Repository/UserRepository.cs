using Microsoft.Extensions.Options;
using PolloPollo.Entities;
using PolloPollo.Shared;
using System;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PolloPollo.Services.Utils;
using PolloPollo.Shared.DTO;

namespace PolloPollo.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly SecurityConfig _config;
        private readonly PolloPolloContext _context;
        private readonly IImageWriter _imageWriter;
        private readonly string _deviceAddress;
        private readonly string _obyteHub;


        public UserRepository(IOptions<SecurityConfig> config, IImageWriter imageWriter, PolloPolloContext context)
        {
            _config = config.Value;
            _imageWriter = imageWriter;
            _context = context;
            _deviceAddress = "AymLnfCdnKSzNHwMFdGnTmGllPdv6Qxgz1fHfbkEcDKo";
            _obyteHub = "obyte.org/bb";
        }

        /// <summary>
        /// Create a full user with a role and sub entity of the given role
        /// </summary>
        /// <param name="dto"></param>
        /// <returns name="TokenDTO"></returns>
        public async Task<TokenDTO> CreateAsync(UserCreateDTO dto)
        {
            if (dto == null || dto.Password == null || dto.Password.Length < 8)
            {
                return null;
            }

            // Creates initial DTO with the static
            // user information
            var userDTO = new DetailedUserDTO
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                SurName = dto.SurName,
                Country = dto.Country,
            };

            // Wrapped into a try catch as there are many DB restrictions
            // that need to be upheld to succeed with the transaction
            try
            {
                var user = new User
                {
                    FirstName = dto.FirstName,
                    SurName = dto.SurName,
                    Email = dto.Email,
                    Country = dto.Country,
                    Created = DateTime.UtcNow,
                    // Important to hash the password
                    Password = PasswordHasher.HashPassword(dto.Email, dto.Password),
                };

                var createdUser = _context.Users.Add(user);

                // Add the user to a role and add a foreign key for the ISA relationship
                // Used to extend the information on a user and give access restrictions
                switch (dto.UserRole)
                {
                    case nameof(UserRoleEnum.Producer):

                        // Can be seperated into different method
                        var producerUserRole = new UserRole
                        {
                            UserId = createdUser.Entity.Id,
                            UserRoleEnum = UserRoleEnum.Producer
                        };

                        var producerUserRoleEntity = _context.UserRoles.Add(producerUserRole);

                        var producer = new Producer
                        {
                            UserId = producerUserRoleEntity.Entity.UserId,
                            PairingSecret = GeneratePairingSecret()
                        };

                        var producerEntity = _context.Producers.Add(producer);

                        await _context.SaveChangesAsync();

                        userDTO = new DetailedProducerDTO
                        {
                            Email = dto.Email,
                            FirstName = dto.FirstName,
                            SurName = dto.SurName,
                            Country = dto.Country,

                            // Set user role on DTO
                            UserRole = userDTO.UserRole = UserRoleEnum.Producer.ToString(),

                            // Get pairing link for OByte wallet immediately.
                            PairingLink = !string.IsNullOrEmpty(producerEntity.Entity.PairingSecret)
                            ? "byteball:" + _deviceAddress + "@" + _obyteHub + "#" + producerEntity.Entity.PairingSecret
                            : default(string)
                        };

                        break;
                    case nameof(UserRoleEnum.Receiver):
                        // Set user role on DTO
                        userDTO.UserRole = UserRoleEnum.Receiver.ToString();

                        // Can be seperated into different method
                        var receiverUserRole = new UserRole
                        {
                            UserId = createdUser.Entity.Id,
                            UserRoleEnum = UserRoleEnum.Receiver
                        };

                        var receiverUserRoleEntity = _context.UserRoles.Add(receiverUserRole);

                        await _context.SaveChangesAsync();

                        var receiver = new Receiver
                        {
                            UserId = receiverUserRoleEntity.Entity.UserId
                        };

                        _context.Receivers.Add(receiver);
                        break;
                    default:
                        // Invalid role
                        return null;
                }

                // Save changes at last,
                // to make it a transaction
                await _context.SaveChangesAsync();

                // Set generated user id after saving the changes to DB
                userDTO.UserId = user.Id;
            }
            catch (Exception)
            {
                // Could also throw an exception for more information when failing the user creation
                return null;
            }


            // Return the user information along with an authorized tokens
            // To login the user after creation
            var tokenDTO = new TokenDTO
            {
                UserDTO = userDTO,
                Token = (await Authenticate(dto.Email, dto.Password)).token,
            };

            return tokenDTO;
        }

        /// <summary>
        /// Fetches a user with all the information for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns name="DetailedUserDTO"></returns>
        public async Task<DetailedUserDTO> FindAsync(int userId)
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
                                                u.Producer.WalletAddress
                                                : default(string),
                                      PairingSecret = role == UserRoleEnum.Producer ?
                                                u.Producer.PairingSecret
                                                : default(string),
                                      u.FirstName,
                                      u.SurName,
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
                    return new DetailedProducerDTO
                    {
                        UserId = fullUser.UserId,
                        Wallet = fullUser.Wallet,
                        PairingLink = !string.IsNullOrEmpty(fullUser.PairingSecret)
                            ? "byteball:" + _deviceAddress + "@" + _obyteHub + "#" + fullUser.PairingSecret
                            : default(string),
                        FirstName = fullUser.FirstName,
                        SurName = fullUser.SurName,
                        Email = fullUser.Email,
                        Country = fullUser.Country,
                        Description = fullUser.Description,
                        City = fullUser.City,
                        Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(fullUser.Thumbnail),
                        UserRole = fullUser.UserRole.ToString()
                    };
                case UserRoleEnum.Receiver:
                    return new DetailedReceiverDTO
                    {
                        UserId = fullUser.UserId,
                        FirstName = fullUser.FirstName,
                        SurName = fullUser.SurName,
                        Email = fullUser.Email,
                        Country = fullUser.Country,
                        Description = fullUser.Description,
                        City = fullUser.City,
                        Thumbnail = ImageHelper.GetRelativeStaticFolderImagePath(fullUser.Thumbnail),
                        UserRole = fullUser.UserRole.ToString()
                    };
                default:
                    // This should never happen, there cannot be an unknown role assigned.
                    return null;
            }
        }

        /// <summary>
        /// Updates information about a user
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(UserUpdateDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Producer)
                .Include(u => u.Receiver)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId && u.Email == dto.Email);

            // Return null if user not found or password don't match
            if (user == null || !PasswordHasher.VerifyPassword(dto.Email, user.Password, dto.Password))
            {
                return false;
            }

            // Update user
            user.FirstName = dto.FirstName;
            user.SurName = dto.SurName;
            user.Country = dto.Country;
            user.Description = dto.Description;
            user.City = dto.City;

            // If new password is set, hash the new password and update
            // the users password
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                if (dto.NewPassword.Length >= 8)
                {
                    // Important to hash the password
                    user.Password = PasswordHasher.HashPassword(dto.Email, dto.NewPassword);
                }
                else
                {
                    return false;
                }
            }

            // Role specific information updated here.
            switch (dto.UserRole)
            {
                case nameof(UserRoleEnum.Producer):
                    // Fields specified for producer is updated here
                    if (!string.IsNullOrEmpty(dto.Wallet) && user.Producer != null)
                    {
                        user.Producer.WalletAddress = dto.Wallet;
                    }

                    break;
                case nameof(UserRoleEnum.Receiver):
                    // Fields specified for receiver is updated here

                    break;
                default:
                    // This should never happen, there cannot be an unknown role assigned.
                    return false;
            }

            try
            {
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Updates information about a user
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdateDeviceAddressAsync(UserPairingDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.Producer)
                .FirstOrDefaultAsync(u => u.Producer.PairingSecret == dto.PairingSecret);

            // Return null if user not found or password don't match
            if (user == null || user.UserRole.UserRoleEnum != UserRoleEnum.Producer)
            {
                return false;
            }

            // Update user
            user.Producer.DeviceAddress = dto.DeviceAddress;
            user.Producer.WalletAddress = dto.WalletAddress;

            try
            {
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Saves a new profile picture for a user on disk, and removes the old image from disk.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public async Task<string> UpdateImageAsync(int id, IFormFile image)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            var folder = ImageFolderEnum.@static.ToString();

            var oldThumbnail = user.Thumbnail;

            try
            {
                var fileName = await _imageWriter.UploadImageAsync(folder, image);

                user.Thumbnail = fileName;

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
        /// Creates an authentication token for a user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<(DetailedUserDTO userDTO, string token)> Authenticate(string email, string password)
        {
            var userEntity = await (from u in _context.Users
                                    where u.Email.Equals(email)
                                    select new
                                    {
                                        u.Id,
                                        u.Password
                                    }).SingleOrDefaultAsync();

            // return null if user not found
            if (userEntity == null)
            {
                return (null, null);
            }


            var user = await FindAsync(userEntity.Id);

            var validPassword = PasswordHasher.VerifyPassword(user.Email, userEntity.Password, password);

            // if password is invalid, then bail out as well
            if (!validPassword)
            {
                return (null, null);
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
                    new Claim(ClaimTypes.NameIdentifier, userEntity.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FirstName + " " + user.SurName),
                    new Claim(ClaimTypes.Role, user.UserRole)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                // Add unique signature signing to Token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var createdToken = tokenHandler.WriteToken(token);

            return (
                user,
                createdToken
                );
        }

        /// <summary>
        /// Retrieve count of producers
        /// </summary>
        public async Task<int> GetCountProducersAsync()
        {
            return await _context.Producers.CountAsync();
        }

        /// <summary>
        /// Retrieve count of receivers
        /// </summary>
        public async Task<int> GetCountReceiversAsync()
        {
            return await _context.Receivers.CountAsync();
        }

        private string GeneratePairingSecret()
        {
            return Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks;
        }
    }
}
