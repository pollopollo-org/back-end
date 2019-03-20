using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PolloPollo.Shared
{
    public class UserUpdateDTO
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [StringLength(255, MinimumLength = 1)]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string NewPassword { get; set; }

        [RegularExpression(@"\S+")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string FirstName { get; set; }

        [RegularExpression(@"\S+")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string Surname { get; set; }

        [RegularExpression(@"[^0-9]+")]
        [StringLength(255, MinimumLength = 1)]
        [Required]
        public string Country { get; set; }

        public string Description { get; set; }

        public string City { get; set; }

        public IFormFile Thumbnail { get; set; }
    }
}
