using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        [Required]
        public string Title { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        [Required]
        public int Price { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(255)]
        public string Location { get; set; }

        [StringLength(255)]
        public string Country { get; set; }

        public string Thumbnail { get; set; }

        [Required]
        public bool Available { get; set; }

        public int Rank { get; set; }
    }
}
