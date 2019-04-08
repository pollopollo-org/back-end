using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PolloPollo.Services.Utils
{
    public interface IImageWriter
    {
        /// <summary>
        /// Saves an image to disk
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns name="fileName"></returns>
        Task<string> UploadImageAsync(string folder, IFormFile file);

        /// <summary>
        /// Deletes a file from filesystem. 
        /// Relative path from the application root.
        /// </summary>
        /// <param name="relativeFilePath"></param>
        /// <returns>If it succeed or not</returns>
        bool DeleteImage(string relativeFilePath);
    }
}
