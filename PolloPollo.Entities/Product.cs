using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolloPollo.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        [Required]
        public string Title { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Price { get; set; }

        [Column(TypeName = "text")]
        public string Description { get; set; }

        [StringLength(255)]
        public string Location { get; set; }

        [StringLength(255)]
        public string Country { get; set; }

        public string Thumbnail { get; set; }

        [Required]
        public bool Available { get; set; }

        public int Rank { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public ICollection<Application> Applications { get; set; }

        public Product()
        {
            Applications = new HashSet<Application>();
        }
    }
}
