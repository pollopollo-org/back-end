using Microsoft.AspNetCore.Identity;

namespace PolloPollo.Repository
{

    /// <summary>
    /// Contains a colleciton of utilities that'll be used across all Repositories
    /// </summary>
    public class Utils
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
    }
}
