using System.Collections.Generic;

namespace PolloPollo.Shared.DTO
{
    public class ApplicationListDTO
    {
        public int Count { get; set; }

        public IEnumerable<ApplicationDTO> List { get; set; }
    }
}
