using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared
{
    public class ProductUpdateDTO
    {
        public int Id { get; set; }

        [Required]
        public bool Available { get; set; }
    }
}
