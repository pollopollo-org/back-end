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
        public string UserRole { get; set; }

        [StringLength(255)]
        [MinLength(8)]
        [Required]
        public string Password { get; set; }

        [StringLength(255)]
        [MinLength(8)]
        public string NewPassword { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(255)]
        public string City { get; set; }

        [StringLength(255)]
        public string Wallet { get; set; }

        public IFormFile Thumbnail { get; set; }
    }
}
