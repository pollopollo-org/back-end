using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PolloPollo.Shared
{
    public class ProductCreateUpdateDTO
    {
        public int Id { get; set; }

        // Prevent being filled with only whitespaces
        [RegularExpression(@"\S+")]
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

        [Required]
        public bool Available { get; set; }
    }
}
