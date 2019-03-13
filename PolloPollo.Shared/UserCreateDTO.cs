using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared
{
    public class UserCreateDTO
    {
        // Prevent being filled with only whitespaces
        [RegularExpression(@"\S+")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string FirstName { get; set; }

        // Prevent being filled with only whitespaces
        [RegularExpression(@"\S+")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string SurName { get; set; }

        // Check to match regular email pattern something@domain
        [EmailAddress]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string Email { get; set; }

        // Countries only contains characters.
        [RegularExpression(@"[^0-9]+")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string Country { get; set; }

        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string Password { get; set; }
    }
}
