using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolloPollo.Shared
{
    public class ProductImageFormDTO
    {
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public IFormFile File { get; set; }
    }
}
