using Microsoft.AspNetCore.Http;

namespace PolloPollo.Shared.DTO
{
    public class UserImageFormDTO
    {
        public string UserId { get; set; }

        public IFormFile File { get; set; }
    }
}
