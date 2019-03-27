using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

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

        /// <summary>
        /// Helper that stores a password on the filesystem and returns a string that specifies where the file has
        /// been stored
        /// </summary>
        public static async Task<string> StoreImageAsync(IFormFile file)
        {
            var folder = "static";
            // Get the base path of where images should be saved
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), folder);

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
                            return $"{folder}/{fileName}";
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

        public static async Task<bool> DeleteImageAsync(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "static", fileName);
            if (File.Exists(filePath)) {
                File.Delete(filePath);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
