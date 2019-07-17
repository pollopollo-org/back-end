using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class UserUpdateDTO
    {
        [Required]
        public int UserId { get; set; }

        [StringLength(191)]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [StringLength(255)]
        [Required]
        public string FirstName { get; set; }

        [StringLength(255)]
        [Required]
        public string SurName { get; set; }

        [RegularExpression(@"[^0-9]+")]
        [StringLength(255)]
        [Required]
        public string Country { get; set; }

        [Required]
        public string UserRole { get; set; }

        [StringLength(255)]
        [MinLength(8)]
        [Required]
        public string Password { get; set; }

        [StringLength(255)]
        public string NewPassword { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        // City only contains characters
        [RegularExpression(@"[^0-9]+")]
        [StringLength(255)]
        public string City { get; set; }

        [StringLength(255)]
        public string Wallet { get; set; }

        // Street only contains characters
        [RegularExpression(@"[^0-9]+")]
        public string Street { get; set; }

        public string StreetNumber { get; set; }

        public string ZipCode { get; set; }
    }
}
