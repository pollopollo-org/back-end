using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class AuthenticateDTO
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
