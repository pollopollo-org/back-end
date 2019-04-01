using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using System.IO;

namespace PolloPollo.Repository.Utils.Tests
{
    public class ImageWriterTests
    {
        [Fact]
        public async Task UploadImageAsync_with_invalid_file_throws_ArgumentException()
        {
            var folder = "static";
            var error = "Invalid image file";
            var fileMock = new Mock<IFormFile>();
            //Setup mock file using a memory stream
            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            var imageWriter = new ImageWriter();

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => imageWriter.UploadImageAsync(folder, fileMock.Object));
            Assert.Equal(error, ex.Message);
        }
    }
}
