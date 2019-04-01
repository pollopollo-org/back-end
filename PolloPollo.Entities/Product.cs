using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        // Prevent being filled with only whitespaces
        [RegularExpression(@"\S+")]
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

        [Required]
        public bool Available { get; set; }
    }
}
