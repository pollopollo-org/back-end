using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class ProductCreateDTO
    {
        [MaxLength(255)]
        [Required]
        public string Title { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, int.MaxValue,
        ErrorMessage = "The price must be a positive number")]
        public int Price { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string Location { get; set; }

        [MaxLength(255)]
        public string Country { get; set; }

        public int Rank { get; set; }
    }
}
