using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PolloPollo.Repository.Utils
{
    public class ImageWriter : IImageWriter
    {
        /// <inheritDoc/>
        public async Task<string> UploadImageAsync(string folder, IFormFile file)
        {
            if (CheckIfImageFile(file))
            {
                try
                {
                    return await WriteFileAsync(folder, file);
                }
                catch (Exception e)
                {
                    throw new IOException(e.Message);
                }
            }

            throw new ArgumentException("Invalid image file");
        }

        /// <inheritDoc/>
        public bool DeleteImage(string folder, string FileName)
        {
            var relativeFilePath = Path.Combine(folder, FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), relativeFilePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method to check if file is image file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>True if it is an image file</returns>
        private bool CheckIfImageFile(IFormFile file)
        {
            using (var ms = new MemoryStream())
            {
                try
                {
                    file.CopyTo(ms);
                    byte[] fileBytes = ms.ToArray();
                    Image<Rgba32> image = Image.Load(fileBytes);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }
        }

        /// <summary>
        /// Writes a file to the the given folder with a GUID name combined with time ticks.
        /// This is done for security reasons.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        /// <returns>The file name of the written file, or the error message if writing to disk fails</returns>
        private async Task<string> WriteFileAsync(string folder, IFormFile file)
        {
            var fileName = "";
            try
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), folder);

                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + extension;

                var filePath = Path.Combine(basePath, fileName);

                using (var bits = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(bits);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return fileName;
        }
    }
}
