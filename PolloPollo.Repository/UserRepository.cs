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
using Microsoft.AspNetCore.Identity;

namespace PolloPollo.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly SecurityConfig _config;
        private readonly PolloPolloContext _context;


        public UserRepository(IOptions<SecurityConfig> config, PolloPolloContext context)
        {
            _config = config.Value;
            _context = context;
        }

        public string Authenticate(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(x => x.Email == email);

            // return null if user not found
            if (user == null)
                return null;

            var validPassword = VerifyPassword(user.Id, password);

            // if password is invalid, then bail out as well
            if (!validPassword)
            {
                return null;
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.Surname}"),
                    new Claim(ClaimTypes.Email, user.Email),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
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
        public bool VerifyPassword(int userId, string plainPassword)
        {
            var user = _context.Users.SingleOrDefault(x => x.Id == userId);
            var hasher = new PasswordHasher<string>();

            var result = hasher.VerifyHashedPassword(user.Email, user.Password, plainPassword);
            return (
                result == PasswordVerificationResult.Success ||
                result == PasswordVerificationResult.SuccessRehashNeeded
            );
        }
    }
}
