using Microsoft.AspNetCore.Http;

namespace PolloPollo.Shared.DTO
{
    public class ProductImageFormDTO
    {
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public IFormFile File { get; set; }
    }
}
