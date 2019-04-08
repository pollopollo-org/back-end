using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class ProductUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public bool Available { get; set; }

        public int Rank { get; set; }
    }
}
