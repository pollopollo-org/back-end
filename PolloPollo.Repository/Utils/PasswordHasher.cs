using Microsoft.AspNetCore.Identity;
using System;

namespace PolloPollo.Services.Utils
{
    /// <summary>
    /// Static wrapper class for the Microsoft.AspNetCore.Identity hashing methods
    /// </summary>
    public class PasswordHasher
    {
        /// <summary>
        /// Wrapper for the HashPassword method from the Microsoft.AspNetCore.Identity library
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static string HashPassword(string email, string password)
        {
            var hasher = new PasswordHasher<string>();

            return hasher.HashPassword(email, password);
        }

        /// <summary>
        /// Wrapper for the VerifyPassword method from the Microsoft.AspNetCore.Identity library
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="plainPassword"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static bool VerifyPassword(string email, string password, string plainPassword)
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
