using Microsoft.AspNetCore.Http;

namespace PolloPollo.Shared
{
    public class UserImageFormDTO
    {
        public string UserId { get; set; }

        public IFormFile File { get; set; }
    }
}
