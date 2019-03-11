using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared
{
    public class UserCreateDTO
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string SurName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
