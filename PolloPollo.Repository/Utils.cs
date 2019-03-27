using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;

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
            var basePath = "";
            try
            {
                basePath = Path.Combine(Directory.GetCurrentDirectory(), folder);
            }
            catch (Exception)
            {
                return null;
            }
            // Get the base path of where images should be saved

            // If the file is empty, then we assume it cannot be saved
            if (basePath != "" && file?.Length > 0)
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

                            // Return the relative url path of the image
                            return $"{folder}/{fileName}";
                        }
                    }

                    // If we get here, then the image couldn't be read as an image, bail out immediately!
                    catch(Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }

            return null;
        }

        public static bool DeleteImageAsync(string relativeFilePath)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), relativeFilePath);
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
