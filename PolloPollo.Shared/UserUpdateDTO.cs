﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PolloPollo.Shared
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
        public string Password { get; set; }

        [StringLength(255)]
        public string NewPassword { get; set; }

        [RegularExpression(@"\S+")]
        [StringLength(255)]
        [Required]
        public string FirstName { get; set; }

        [RegularExpression(@"\S+")]
        [StringLength(255)]
        [Required]
        public string SurName { get; set; }

        [RegularExpression(@"[^0-9]+")]
        [StringLength(255)]
        [Required]
        public string Country { get; set; }

        [Required]
        public string Role { get; set; }

        public string Description { get; set; }

        public string City { get; set; }

        public IFormFile Thumbnail { get; set; }
    }
}
