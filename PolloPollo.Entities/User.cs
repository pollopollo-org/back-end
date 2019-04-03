using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class User
    {
        [Key]
        public int Id { get; set; }

        [EmailAddress]
        [Required]
        [MaxLength(191)]
        public string Email { get; set; }

        [StringLength(255)]
        [MinLength(8)]
        [Required]
        public string Password { get; set; }

        [StringLength(255)]
        [Required]
        public string FirstName { get; set; }

        [StringLength(255)]
        [Required]
        public string SurName { get; set; }

        [StringLength(255)]
        [Required]
        public string Country { get; set; }

        public string Description { get; set; }

        public string City { get; set; }

        public string Thumbnail { get; set; }

        public UserRole UserRole { get; set; }

        // Depending on the selected role at registration, either
        // producer or receiver is assigned
        public Receiver Receiver { get; set; }

        public Producer Producer { get; set; }

        public ICollection<Product> Products { get; set; }

        public User()
        {
            Products = new HashSet<Product>();
        }
    }
}
