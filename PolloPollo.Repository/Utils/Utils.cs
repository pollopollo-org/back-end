using Microsoft.AspNetCore.Identity;

namespace PolloPollo.Repository.Utils
{
    public class PasswordHasher
    {
        /// <summary>
        /// Internal helper that hashes a given password to prepare it for storing in the database
        /// </summary>
        public static string HashPassword(string email, string password)
        {
            var hasher = new PasswordHasher<string>();

            return hasher.HashPassword(email, password);
        }

        /// <summary>
        /// Internal helper that verifies if a given password matches the hashed password of a user stored in the database
        /// </summary>
        public static bool VerifyPassword(string email, string password, string plainPassword)
        {
            var hasher = new PasswordHasher<string>();

            var result = hasher.VerifyHashedPassword(email, password, plainPassword);
            return (
                result == PasswordVerificationResult.Success ||
                result == PasswordVerificationResult.SuccessRehashNeeded
            );
        }

        /// <summary>
        /// Helper that stores a password on the filesystem and returns a string that specifies where the file has
        /// been stored
        /// </summary>
    }
}
