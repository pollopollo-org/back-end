using System.Collections.Generic;

namespace PolloPollo.Shared
{
    public class ProductListDTO
    {
        public int Count { get; set; }

        public IEnumerable<ProductDTO> List { get; set; }
    }
}
