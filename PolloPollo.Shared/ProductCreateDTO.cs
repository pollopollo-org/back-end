﻿using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared
{
    public class ProductCreateDTO
    {
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Title { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int Price { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string Location { get; set; }

        [MaxLength(255)]
        public string Country { get; set; }
    }
}