﻿using System.ComponentModel.DataAnnotations;
using PolloPollo.Shared;

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

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string Surname { get; set; }

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
    }
}
