using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PolloPollo.Shared
{
    public class UserCreateUpdateDTO
    {
        [Key]
        public int Id { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        private string Role { get; set; }

        public string Description { get; set; }

        public string City { get; set; }

        public string Thumbnail { get; set; }
    }
}
