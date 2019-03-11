using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared
{
    public class UserCreateDTO
    {
        [RegularExpression(@"\S+")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string FirstName { get; set; }

        [RegularExpression(@"\S+")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string SurName { get; set; }

        // Check to match regular email pattern something@domain.com
        [RegularExpression(@".+[@].+[.].+")]
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
